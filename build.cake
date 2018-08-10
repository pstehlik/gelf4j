
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
    // CreateDirectory(binDir);
    // CreateDirectory(binDirPortable);
    // CreateDirectory(distDir);
    // CleanDirectory(binDir);
    // CleanDirectory(distDir);
    // CleanDirectory(binDirPortable);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    // NuGetRestore($"./src/{appName}.sln");
});

Task("Build")
    //.IsDependentOn("UpdateVersion")
    .Does(() =>
{
    DotNetCoreBuild("./gelf4net.sln");
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
    DotNetCoreTest("./test/Gelf4Net.Tests/");
});


Task("BuildPackage")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    // BuildUploadPackage(false);
});

Task("PushToNuget")
    .IsDependentOn("Run-Unit-Tests")
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
