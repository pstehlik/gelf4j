#tool nuget:?package=NUnit.Runners&version=2.6.4
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var appName = "Gelf4net";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var binDir = MakeAbsolute(Directory("./bin"));
var distDir = MakeAbsolute(Directory("./deploy"));

void uploadGelf4net() {
    var assemblyFile = "./src/Gelf4net/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;
    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = "src/Gelf4net/bin/Release/Gelf4net.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = "src/Gelf4net.Portable/bin/Release/Gelf4net.dll", Target = "lib/netstandard1.5"}
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack("./src/Gelf4net/package.nuspec", nuGetPackSettings);

    //var package = string.Format("./pack/Gelf4net.{0}.nupkg", newVersion);
    Console.WriteLine(string.Format("./pack/Gelf4net.{0}.nupkg", newVersion));

    // Push the package.
    //NuGetPush(package, new NuGetPushSettings {
    //    Source = "http://localhost:9997/api/odata",
    //    ApiKey = "4258a1b5-9b40-4d74-b731-6e2dda147450"
    //});
}

void uploadGelf4netAmqpAppender() {
    var appenderName = "AmqpAppender";
    var assemblyFile = $"./src/Gelf4net.{appenderName}/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;
    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}/bin/Release/Gelf4net.{appenderName}.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}/bin/Release/Gelf4net.Core.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}.Portable/bin/Release/Gelf4net.{appenderName}.dll", Target = "lib/netstandard1.5"},
                                        new NuSpecContent {Source = "src/Gelf4net.Core.Portable/bin/Release/Gelf4net.Core.dll", Target = "lib/netstandard1.5"}
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/Gelf4net.{appenderName}/package.nuspec", nuGetPackSettings);

    //var package = string.Format("./pack/Gelf4net.{0}.nupkg", newVersion);
    Console.WriteLine($"./pack/Gelf4net.{appenderName}.{newVersion}.nupkg");

    // Push the package.
    //NuGetPush(package, new NuGetPushSettings {
    //    Source = "http://localhost:9997/api/odata",
    //    ApiKey = "4258a1b5-9b40-4d74-b731-6e2dda147450"
    //});
}

void uploadGelf4netHttpAppender() {
    var appenderName = "HttpAppender";
    var assemblyFile = $"./src/Gelf4net.{appenderName}/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;
    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}/bin/Release/Gelf4net.{appenderName}.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}/bin/Release/Gelf4net.Core.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}.Portable/bin/Release/Gelf4net.{appenderName}.dll", Target = "lib/netstandard1.5"},
                                        new NuSpecContent {Source = "src/Gelf4net.Core.Portable/bin/Release/Gelf4net.Core.dll", Target = "lib/netstandard1.5"}
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/Gelf4net.{appenderName}/package.nuspec", nuGetPackSettings);

    //var package = string.Format("./pack/Gelf4net.{0}.nupkg", newVersion);
    Console.WriteLine($"./pack/Gelf4net.{appenderName}.{newVersion}.nupkg");

    // Push the package.
    //NuGetPush(package, new NuGetPushSettings {
    //    Source = "http://localhost:9997/api/odata",
    //    ApiKey = "4258a1b5-9b40-4d74-b731-6e2dda147450"
    //});
}

void uploadGelf4netUdpAppender() {
    var appenderName = "UdpAppender";
    var assemblyFile = $"./src/Gelf4net.{appenderName}/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;
    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}/bin/Release/Gelf4net.{appenderName}.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}/bin/Release/Gelf4net.Core.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"src/Gelf4net.{appenderName}.Portable/bin/Release/Gelf4net.{appenderName}.dll", Target = "lib/netstandard1.5"},
                                        new NuSpecContent {Source = "src/Gelf4net.Core.Portable/bin/Release/Gelf4net.Core.dll", Target = "lib/netstandard1.5"}
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/Gelf4net.{appenderName}/package.nuspec", nuGetPackSettings);

    //var package = string.Format("./pack/Gelf4net.{0}.nupkg", newVersion);
    Console.WriteLine($"./pack/Gelf4net.{appenderName}.{newVersion}.nupkg");

    // Push the package.
    //NuGetPush(package, new NuGetPushSettings {
    //    Source = "http://localhost:9997/api/odata",
    //    ApiKey = "4258a1b5-9b40-4d74-b731-6e2dda147450"
    //});
}


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CreateDirectory(binDir);
    CreateDirectory(distDir);
    CleanDirectory(binDir);
    CleanDirectory(distDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/Gelf4net.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{

    if(IsRunningOnWindows())
    {
      // Use MSBuild
        MSBuild("./src/Gelf4net.sln", settings =>
            settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./src/Gelf4net.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit("./src/**/bin/" + configuration + "/*Tests.dll", new NUnitSettings {
        NoResults = true
    });
});


Task("PushToNuget")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    uploadGelf4net();
    uploadGelf4netAmqpAppender();
    uploadGelf4netHttpAppender();
    uploadGelf4netUdpAppender();

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
