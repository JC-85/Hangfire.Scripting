using Hangfire.Scripting.Dashboard.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if DEBUG
namespace TestApp
{
    public class ExampleJob : IJob
    {
        public void ExampleMethod1()
        {

        }
    }

    [JobCategory("Second example")]
    public class ExampleWithDecorators : IJob
    {

    }
}
#endif