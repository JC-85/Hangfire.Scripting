using Hangfire.Scripting.Dashboard.Management.JobData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Hangfire.Scripting.Dashboard.Management
{
    public static class JobsHelper
    {
        internal static (List<JobMetadata>, List<JobCategory>) GetAllJobs(Assembly assembly)
        {
            var Metadata = new List<JobMetadata>();
            var Pages = new List<JobCategory>();

            var jobMethods = assembly.GetTypes().Where(x => !x.IsInterface && typeof(IJob).IsAssignableFrom(x) && x.Name != (typeof(IJob).Name));

            foreach (Type ti in jobMethods)
            {
                var q="default";
                var title = "Default";

                if (ti.GetCustomAttributes(true).OfType<JobCategoryAttribute>().Any())
                {
                    
                    JobCategory attr = new JobCategory(ti.GetCustomAttribute<JobCategoryAttribute>());
                    //q =  attr.Queue;
                    title = attr.Title;
                    if(!Pages.Any(x => x.Title == title)) Pages.Add(attr);
                }

                foreach (MethodInfo methodInfo in ti.GetMethods().Where(m => m.DeclaringType == ti))
                {
                    string jobName = methodInfo.Name;

                    var meta = new JobMetadata(jobName) { MethodInfo = methodInfo, Queue = q, Category = title};
                    meta.MethodInfo = methodInfo;
                    if (methodInfo.GetCustomAttributes(true).OfType<DescriptionAttribute>().Any())
                    {
                        meta.Description = methodInfo.GetCustomAttribute<DescriptionAttribute>().Description;
                    }

                    if (methodInfo.GetCustomAttributes(true).OfType<DisplayNameAttribute>().Any())
                    {
                        
                        meta.DisplayName = methodInfo.GetCustomAttribute<DisplayNameAttribute>().DisplayName;
                    }

                    Metadata.Add(meta);
                }
            }

            return (Metadata, Pages);
        }
    }
}
