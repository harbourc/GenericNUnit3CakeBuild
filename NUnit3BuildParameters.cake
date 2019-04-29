using Cake.Common.Build.TeamCity.Data;

public class BuildParameters
{
	private ICakeContext Context;

	//optional todo: Get these from environment variable or team city provider
	public const string MsNugetFeed = "https://api.nuget.org/v3/index.json";

	
	public List<string> NuGetSources = new List<string>();
	
	public string Target { get; private set; }
	public string Configuration { get; private set; }
	public bool IsLocalBuild { get; private set; }
	public bool IsRunningOnUnix { get; private set; }
	public bool IsRunningOnWindows { get; private set; }
	public bool IsPaketSolution {get; private set;}
	
	public DirectoryPath ToolsDir {get; private set;}
	
	public NuGetRestoreSettings NuGetRestoreSettings {get; private set;}
	public bool ShouldRunIntegrationTests {get; private set;}
	
	public SolutionParameters Solution {get; private set;}
	public BuildParameters SetSolutionParameters(SolutionParameters parameters)
	{
		Solution = parameters;
		return this;
	}

	public TestParameters TestParameters {get; set;}
	public BuildParameters SetTestParameters(TestParameters testParameters)
	{
		TestParameters = testParameters;
		if(! Context.DirectoryExists(TestParameters.TestResultsDir)){
			Context.CreateDirectory(TestParameters.TestResultsDir);
		}
		return this;
	}
	
	public BuildParameters(ICakeContext context, BuildSystem buildSystem)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		Context = context;

		var configurationParameter = context.Argument<string>("buildconfig", null);
		if (configurationParameter == null)
		{
			Configuration = context.Argument("configuration", "Debug");
		} else
		{
			Configuration = context.Argument("configuration", configurationParameter);
		}

		Target = context.Argument("target", "Default");
		IsLocalBuild = buildSystem.IsLocalBuild;
		IsRunningOnUnix = context.IsRunningOnUnix();
		IsRunningOnWindows = context.IsRunningOnWindows();
		
		ToolsDir = context.MakeAbsolute(context.Directory("./tools"));
		

		NuGetRestoreSettings = 
			new NuGetRestoreSettings{
				Source = NuGetSources				
			};

			NuGetSources.AddRange( 
				new[]{MsNugetFeed});
	}
}

public class SolutionParameters
{
	private ICakeContext Context;
    public SolutionParameters(ICakeContext context, string name)
    {
		Name = name;
		Context = context;
        Projects = new List<ProjectParameters>();
        var csprojs = context.GetFiles("./**/*.csproj");
        foreach(var proj in csprojs){
            var project = 
                new ProjectParameters{
                    Name = proj.GetFilename(),
                    FullPath = proj.FullPath,
                    Directory = proj.GetDirectory()
                };
            Projects.Add(project);
        }

    }
	public string Name {get;}
	public FilePath DbProjectPath {get; set;}
	public FilePath Path { get { return Context.MakeAbsolute(new FilePath("./"+ Name));}}
    public List<ProjectParameters> Projects {get; set;}
}

public class ProjectParameters
{
    public FilePath Name {get; set;}
    public FilePath FullPath {get; set;}
    public DirectoryPath Directory {get; set;}    
}

public class TestParameters
{
	// TODO: Update this parameter with the file location of your NUnit3 testlist
	public TestParameters(ICakeContext context, string pathToTestListFile = "./ProjectName/")
	{
		WhereUITestMatch = context.Argument<string>("testswhere", null);				
		var testRunFileArg = context.Argument<string>("testslist", null);
		
		FilePath testRunFilePath = null;
		if(testRunFileArg != null) 
		{
			testRunFilePath = context.File($"{pathToTestListFile}{testRunFileArg}");
			TestListFP = testRunFilePath;
		}
		TestResultsDir = context.MakeAbsolute(context.Directory("./TestResults"));
	}

	public DirectoryPath TestResultsDir {get; private set;}
	public FilePath TestListFP {get; private set;}
	public string WhereUITestMatch {get; private set;}
}