using System.Reflection;
using System.Collections.Generic;
using System;

namespace Hangfire.Scripting.Dashboard.Management.JobData
{
    public class JobMetadata
    {
        public JobMetadata(string scriptName)
        {
            this.JobName = scriptName;
        }

        public string Category { get; internal set; }
        public string JobName { get; internal set; }
        public MethodInfo MethodInfo { get; internal set; }
        public Dictionary<string, object> DefaultArguments { get; } = new Dictionary<string, object>();
        public string DisplayName { get; internal set; }
        public string Description { get; internal set; }
        public string Queue { get; internal set; } = "default";
    }
}