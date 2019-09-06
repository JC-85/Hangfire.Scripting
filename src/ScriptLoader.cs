using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.IO;
using System.ComponentModel;
using Hangfire.Scripting.Dashboard.Management.JobData;

namespace Hangfire.Scripting
{


    public class Globals
    {
        public string GetConnectionString(string name) => name + "Connection";
        public Hangfire.Server.PerformContext Context;
        public string CheckProp { get; set; } = "Prop Working";
        public string CheckPred => "Pred Working";
    }

    public class ScriptWathcer
    {
        public static void Watch(string path)
        {
            Watch(new DirectoryInfo(path));
        }
        public static void Watch(DirectoryInfo folder)
        {
            System.IO.FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(folder.FullName);
            fileSystemWatcher.Changed += (o, e) => {

                BackgroundJob.Enqueue<ScriptLoader>((s) => s.UpdateScript(null, e.FullPath));
                System.Console.WriteLine("TEST " + e.ChangeType.ToString());
            };
            fileSystemWatcher.EnableRaisingEvents = true;
        }

    }

    public class ScriptLoader
    {
       
        [DisplayName("Recompiling {1}")]
        [AutomaticRetry(Attempts = 0)]
        public void UpdateScript(Hangfire.Server.PerformContext context, string path)
        {
            var script = CompileScriptFile(path);
            DeployScript(script);
        }

        [DisplayName("Script: {1}")]
        public void ScriptRunner(Server.PerformContext context,
            [JobParameter(hidden: true)] string scriptName
            )
        {
            if(scripts.TryGetValue(scriptName, out Script script))
            {
                var comp = script.GetCompilation();
                var diag = comp.GetDiagnostics();
                
                var glob = new Globals()
                {
                    Context = context
                };
                
                script.RunAsync(globals: glob);
            }
        }

        void DeployScript(Script script, string name = null)
        {
            var scriptName = name ?? new FileInfo(script.Options.FilePath).Name ?? Guid.NewGuid().ToString();

            if (scripts.ContainsKey(scriptName))
                scripts[scriptName] = script;
            else scripts.Add(scriptName, script);

            var gc = script.GetCompilation();

            var errors = gc.GetDiagnostics();

            var job = new JobMetadata(scriptName)
            {
                DisplayName = scriptName,
                Category = "Scripts",
                MethodInfo = this.GetType().GetMethod("ScriptRunner")
            };

            job.DefaultArguments.Add("scriptName", scriptName);

            Hangfire.Scripting.GlobalConfigurationExtension.Jobs.Add(job);
        }

        Script CompileScriptFile(string filePath)
        {
            return CompileScriptFile(new FileInfo(filePath));
        }

        Script CompileScriptFile(FileInfo file)
        {
            var scriptVal = System.IO.File.ReadAllText(file.FullName);

            var scriptName = file.Name.Replace(".csx", "");

            var opt = ScriptOptions.Default
                .WithFilePath(file.FullName)
                .AddImports("Hangfire", "Hangfire.Console")
                .AddReferences(
                    typeof(Hangfire.Server.PerformContext).Assembly,
                    typeof(Hangfire.Console.ConsoleExtensions).Assembly);
            
            var script = CSharpScript.Create(scriptVal, globalsType: typeof(Globals), options: opt);


            var comp = script.Compile();
            var diag = script.GetCompilation().GetDiagnostics();
            if (diag.Any()) throw new Exception("Script compilation failed:\r\n" + diag.First().ToString());
            return script;
        }

        static Dictionary<string, Script> scripts = new Dictionary<string, Script>();
        public void LoadScripts(string path)
        {
            foreach (var file in System.IO.Directory.GetFiles(path, "*.csx"))
            {
                FileInfo fileInfo = new FileInfo(file);
                try
                {
                    var script = CompileScriptFile(fileInfo);
                    DeployScript(script);
                }
                catch (Exception)
                {

                }
            }

            GlobalConfigurationExtension.AddManagementPage(
                new Dashboard.Management.JobCategory("Scripts"));
        }
    }
}
