
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  DotNetCoreClean("./gelf4net.sln");
});

Task("Restore-NuGet-Packages")
  .IsDependentOn("Clean")
  .Does(() =>
{
  DotNetCoreRestore("./gelf4net.sln");
});

Task("Build")
  //.IsDependentOn("UpdateVersion")
  .Does(() =>
{
  DotNetCoreBuild("./gelf4net.sln", new DotNetCoreBuildSettings
  {
    Configuration = configuration,
  });
});

Task("UpdateVersion")
  .IsDependentOn("Restore-NuGet-Packages")
  .Does(() => {

  // var packageName = $"{appName}";
  // updateAssemblyFile(packageName);

  // packageName = $"{appName}.AmqpAppender";
  // updateAssemblyFile(packageName);

  // packageName = $"{appName}.HttpAppender";
  // updateAssemblyFile(packageName);

  // packageName = $"{appName}.UdpAppender";
  // updateAssemblyFile(packageName);

});

Task("Run-Unit-Tests")
  .IsDependentOn("Build")
  .Does(() =>
{
  var settings = new DotNetCoreTestSettings
  {
    Framework = "net47",
    Configuration = configuration,
    NoBuild = true,
    NoRestore = true,
  };

  var projectFiles = GetFiles("./test/**/*.csproj");
  foreach(var file in projectFiles)
  {
    DotNetCoreTest(file.FullPath, settings);
  }
});


Task("BuildPackage")
  .IsDependentOn("Run-Unit-Tests")
  .Does(() =>
{
  var settings = new DotNetCorePackSettings
  {
    Configuration = configuration,
    OutputDirectory = "./artifacts/"
  };

  DotNetCorePack("gelf4net.sln", settings);
});

Task("PushToNuget")
  .IsDependentOn("BuildPackage")
  .Does(() =>
{
  // BuildUploadPackage(true);
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
