using System;

namespace Hangfire.Scripting.Dashboard.Management
{
    public class JobCategory
    {
        public JobCategory(string menuName)
        {
            this.MenuName = menuName;
        }

        public JobCategory(JobCategoryAttribute jobCategoryAttribute)
        {
            this.MenuName = jobCategoryAttribute.MenuName;
        }

        public string MenuName { get; internal set; }
        public string Title { get; internal set; }
    }

    public class JobCategoryAttribute : Attribute
    {
        public JobCategoryAttribute(string menuName)
        {
            this.MenuName = menuName;
        }

        public string MenuName { get; }
    }
}