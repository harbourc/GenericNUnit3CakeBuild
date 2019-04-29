#tool "nuget:?package=Nunit.ConsoleRunner&version=3.8.0"
#tool "nuget:?package=ReportUnit&version=1.2.1"
#load "Parameters.cake"
// Using statements
using System.Text.RegularExpressions;

BuildParameters Parameters = new BuildParameters(Context, BuildSystem);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup( ctx =>
{
// TODO: Enter your .sln file name here for the project you wish to build
	Parameters.SetSolutionParameters(new SolutionParameters(ctx, "Solution.sln"));
	Information("Building {2} ({0}, {1})",
		Parameters.Configuration,
		Parameters.Target,
		Parameters.Solution.Name);
});


//////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////
Func<IFileSystemInfo, bool> exceptPackagesConfig = 
    fileSystemInfo => !fileSystemInfo.Path.FullPath.ToLower().EndsWith("packages.config");


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Clean")
	.Does(() => 
	{
		if(Parameters.Solution.Projects != null){
			foreach(var path in Parameters.Solution.Projects.Select(p=> p.Directory)){
				Information("Cleaning {0}", path);
				CleanDirectories(path + "/**/bin/");
				CleanDirectories(path + "/**/obj/");
			}
		}
	})
	 .ReportError(exception =>
    {  
        // Report the error.
        Error(exception);
			
    });


Task("CleanAll")
    .IsDependentOn("Clean")
    .Does(()=>
    {
        var packagesFolder = MakeAbsolute(Directory("./packages"));
        if (packagesFolder != null && DirectoryExists(packagesFolder))
        {
            CleanDirectory(packagesFolder, exceptPackagesConfig);
        }
    });


Task("Restore")
	.Does(()=>
	{
		NuGetRestore(Parameters.Solution.Path, Parameters.NuGetRestoreSettings);
	})
    .ReportError(exception =>
    {  
        // Report the error.
        Error(exception);
    });


Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
	{
		Information("Building {0}", Parameters.Solution.Name);
 		MSBuild(Parameters.Solution.Path, 
            new MSBuildSettings() {
				Configuration = Parameters.Configuration,
				PlatformTarget = PlatformTarget.MSIL,
				WarningsAsError = false,
				Verbosity = Verbosity.Minimal,
				NodeReuse = false,
				//ToolVersion = MSBuildToolVersion.NET46,
			}
        );
	});

Task("BuildClean")
.IsDependentOn("Clean")
.IsDependentOn("Build");

 Task("UITests")
 	.IsDependentOn("Build")
 	.Does(() =>
 	{
	// TODO: Enter file name for .dll file corresponding to the project you wish to run the project against
		var testAssembly = GetFiles("./ProjectName/bin/" + Parameters.Configuration + "/ProjectName.dll");
		Parameters.SetTestParameters(new TestParameters(Context));

		if (DirectoryExists(Parameters.TestParameters.TestResultsDir))
        {
            CleanDirectory(Parameters.TestParameters.TestResultsDir);
        }

 		NUnit3(testAssembly, 
		 	new NUnit3Settings {
				Where = Parameters.TestParameters.WhereUITestMatch,
				TestList = MakeAbsolute(Parameters.TestParameters.TestListFP),
				Work = Parameters.TestParameters.TestResultsDir,
				Labels = NUnit3Labels.All
					}
		);
		ReportUnit(Parameters.TestParameters.TestResultsDir); 
 	})
	 .OnError(exception =>
    {  
		Information(exception);
    });

Task("ListTasks")
	.Does(()=> {
		var t = Tasks;
		var tLen = t.Count();
			
		for(var i=0; i< tLen; i++){
			Information(t[i].Name);
		}
	});


Task("Show-Info")
	.Does(() =>
	{
		Information("Target: {0}", Parameters.Target);
		Information("Configuration: {0}", Parameters.Configuration);
		Information("Solution FilePath: {0}", Parameters.Solution.Path);
	});

Task("Default")
  .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Parameters.Target);