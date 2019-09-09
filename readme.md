## Hangfire.Scripting ##

Hangfire.Scripting lets you run CScharp scripts with Roslyn using Microsoft.CodeAnalysis.CSharp.Scripting nuget package. 
This hangfire extension uses a modified version of the https://github.com/pjrharley/Hangfire.Core.Dashboard.Management extension to enable scheduling and manual execution of your scripts.

## How to use ##

Initialize scripting by calling UseScripting() on Hangfire.GlobalConfiguration.Configuration. The extension supports call chaining.

    GlobalConfiguration.Configuration
    	.UseSqlServerStorage(@"Server=.\SQLEXPRESS; Database=Hangfire.Sample; Integrated Security=True")
		.UseScripting();
		
The extension requires a folder named Scripts to exists in the working directory. Alternatively a path can be provided as ScriptingOptions
when calling UseScripts()


	var options = new ScriptingOptions(){
		Path = "C:\\PathToYourScriptDir"
	};
	GlobalConfiguration.Configuration.UseScripting(options);
	
## Writing scripts ##

Scripts should be placed in the Scripts folder of your sollution (or in the path provided in ScriptingOptions) and need to have csx as file extension.
A FileSystemWatcher is tracking the folder for changes, and if detected, recompiles the script file. Compilation is done as a Hangfire job so you can track the status in your Hangfire Dashboard.

Example script file:

test.csx

    using System;
    
    Console.WriteLine("TEST2");

    int TestFunction()
    {
        Console.WriteLine("This is a test");
     	return 42;
	}

	var retVal = TestFunction();

	Context.WriteLine("PerformContext output");

Make sure your scripts are set to "Copy Allways" or "Copy if newer" if you're deploying from Visual Studio.

A PerformContext object with the Hangfire.Console extension methods is provided as a global variable named Context which lets your print output to your Hangfire job.
