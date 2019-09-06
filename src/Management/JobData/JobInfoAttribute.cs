using System;

namespace Hangfire.Scripting.Dashboard.Management.JobData
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class JobParameterAttribute : Attribute
    {
        public string LabelText { get; set; }
        public string PlaceholderText { get; set; } = "";

        public bool Hidden { get; set; } = false;
        
        public JobParameterAttribute(string labelText)
        {
            this.LabelText = labelText;
        }

        public JobParameterAttribute(bool hidden)
        {
            this.Hidden = hidden;
        }
    }


}