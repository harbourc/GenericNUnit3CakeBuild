# GenericNUnit3CakeBuild

This build script is used to run a collection of NUnit3 tests. The following requirements for your project must be met:

-NUnit NuGet package installed. Version 3.0 or later required
-Text file containing namespace for NUnit tests to be run must be present. See sample files below
-(Optional) Build configuration (.config) files must be present if you wish to run your project against anything other than Debug

Before you run, make note of the TODO's in the .cake files. These must be updated with your project information for this script to function. Here are some details regarding each TODO task:

NUnit3Build.cake
-TODO: Enter your .sln file name here for the project you wish to build
  -This needs to be updated with the .sln file associated with the solution containing your project.
  
-TODO: Enter file name for .dll file corresponding to the project you wish to run the project against
  -This needs to be updated with the .dll file associated with the project containing your NUnit tests.
  -For example, the C# project named "MyProject" will generate MyProject.dll on build.
  
NUnit3BuildParameters.cake
-TODO: Update this parameter with the file location of your NUnit3 testlist
  -Your .txt testlist file should be placed in a known location. The Cake build is pointing to its current directory, so if your testlist    file is not in the same directory as your .ps1 and .cake files then "pathToTestListFile" will need to be updated accordingly. 
  

To run the script:

-Open PowerShell console
-Navigate to your project directory containing your Cake build files
-Enter the following command:

  .\build.ps1 -Target TestRun --testslist=my_test_list.txt --buildconfig=App.Release.config
  
  
Here is an outline of the test parameters, --testslist and --buildconfig
