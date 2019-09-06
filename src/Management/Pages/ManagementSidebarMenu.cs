using System;
using System.Collections.Generic;
using Hangfire.Dashboard;

namespace Hangfire.Scripting.Dashboard.Management.Pages
{
    public static class ManagementSidebarMenu
    {
        public static List<Func<RazorPage, MenuItem>> Items { get; } = new List<Func<RazorPage, MenuItem>>();
    }
}