#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#load "local:?path=tools/cakeAddIns/RunSimultaneously.cake"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/Example/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("a")
.Does(() => 
{
    RunSimultaneously(
    () =>
    {
        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start(); 
        Warning("START 1");
        Thread.Sleep(3000);
        stopWatch.Stop();
        long duration = stopWatch.ElapsedMilliseconds;
        Warning("FINISHED 1 took:" + duration);
    },
    () =>
    {
        RunSimultaneously(() =>
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start(); 
            Warning("START 2");
            Thread.Sleep(1000);
            Warning("FINISHED 2");
            long duration = stopWatch.ElapsedMilliseconds;
            Warning("FINISHED 1 took:" + duration);
        },
        () =>
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            Warning("START 3");
            Thread.Sleep(1000);
            long duration = stopWatch.ElapsedMilliseconds;
            Warning("FINISHED 3" + duration);
            stopWatch.Start();
            Warning("START 4");
            Thread.Sleep(1000); 
            duration = stopWatch.ElapsedMilliseconds;
            Warning("FINISHED 4 took:" + duration);
        });
    }
    );
});



Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/Example.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./src/Example.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./src/Example.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        NoResults = true
        });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
