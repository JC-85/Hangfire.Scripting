using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Hangfire.Scripting.Dashboard.Management;
using Hangfire.Scripting.Dashboard.Management.JobData;
using Hangfire.Scripting.Dashboard.Management.Pages;

namespace Hangfire.Scripting
{
    public static class GlobalConfigurationExtension
    {
        static GlobalConfigurationExtension()
        {
            JobCategories = new System.Collections.ObjectModel.ObservableCollection<JobCategory>();
            Jobs = new System.Collections.ObjectModel.ObservableCollection<JobMetadata>();
        }

        public static System.Collections.ObjectModel.ObservableCollection<JobCategory> JobCategories { get; private set; }
        public static System.Collections.ObjectModel.ObservableCollection<JobMetadata> Jobs { get; private set; }

        static ScriptingOptions Options;

        private static void InitJobs(Assembly assembly)
        {
            var jobs = JobsHelper.GetAllJobs(assembly);
            var methodMeta = jobs.Item1;
            var pages = jobs.Item2;

            pages.ForEach(x => JobCategories.Add(x));
            methodMeta.ForEach(x => Jobs.Add(x));

            Jobs.CollectionChanged += (sender, e) => {
                if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach(JobMetadata v in e.NewItems)
                    {
                        //Add item to manager
                        //ManagementBasePage.Jobs.Add(v);
                        ManagementBasePage.AddCommands(v);
                    }
                }
            };

        }

        public static void AddManagementPage(JobCategory pageInfo)
        {
            //ManagementBasePage.AddCommands(pageInfo.Queue);

            ManagementSidebarMenu.Items.Add(p => new MenuItem(pageInfo.MenuName, p.Url.To($"{ManagementPage.UrlRoute}/{pageInfo.MenuName}"))
            {
                Active = p.RequestPath.StartsWith($"{ManagementPage.UrlRoute}/{pageInfo.MenuName}")
            });

            DashboardRoutes.Routes.AddRazorPage($"{ManagementPage.UrlRoute}/{pageInfo.MenuName}", x => new ManagementBasePage(pageInfo, Jobs));
        }

        public static IGlobalConfiguration UseScripting(this IGlobalConfiguration config, ScriptingOptions options = null,  Assembly assembly = null)
        {
            Options = options;
            DirectoryInfo scriptPath;
            if(options != null)
            {
                scriptPath = new DirectoryInfo(options.Path);
            }
            else
            {
                scriptPath = new DirectoryInfo(".\\Scripts");
            }

            var loader = new ScriptLoader();
            loader.LoadScripts(scriptPath.FullName);

            ScriptWatcher.Watch(scriptPath);

            if (assembly == null) assembly = System.Reflection.Assembly.GetExecutingAssembly();

            InitJobs(assembly);

            foreach (var pageInfo in JobCategories)
            {
                AddManagementPage(pageInfo);
            }
            
            //note: have to use new here as the pages are dispatched and created each time. If we use an instance, the page gets duplicated on each call
            DashboardRoutes.Routes.AddRazorPage(ManagementPage.UrlRoute, x => new ManagementPage());
            
            // can't use the method of Hangfire.Console as it's usage overrides any similar usage here. Thus
            // we have to add our own endpoint to load it and call it from our code. Actually is a lot less work
            DashboardRoutes.Routes.Add("/jsm", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.Scripting.Content.management.js"));

            Func<RazorPage, MenuItem> f =  page => new MenuItem(ManagementPage.Title, page.Url.To(ManagementPage.UrlRoute))
            {
                Active = page.RequestPath.StartsWith(ManagementPage.UrlRoute)
            };

            NavigationMenu.Items.Add(f);

            //ManagementBasePage.Jobs.AddRange(Jobs);

            return config;
        }
    }
}
