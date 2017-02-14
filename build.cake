#tool nuget:?package=NUnit.Runners&version=2.6.4
#tool nuget:?package=ilmerge
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var appName = "Gelf4Net";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var binDir = MakeAbsolute(Directory("./bin"));
var binDirPortable = MakeAbsolute(Directory("./bin/portable"));
var distDir = MakeAbsolute(Directory("./deploy"));


void GenerateMergedDll(string outputFile, string primaryDll, IEnumerable<FilePath> assemblyPaths, bool internalize){
    ILMerge(outputFile, 
            primaryDll, 
            assemblyPaths, 
            new ILMergeSettings { Internalize = internalize });
}

void uploadGelf4net() {
    var assemblyFile = $"./src/{appName}/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;
    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Title = "${appName}",
        Product = $"{appName}",
        Copyright = $"Copyright © {DateTime.Now.Year}",
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    var net45Path = $"src/{appName}/bin/Release/";

    var assemblyPaths = new string[] {
        $"{net45Path}Newtonsoft.Json.dll",
        $"{net45Path}RabbitMQ.Client.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{binDir}/{appName}.dll", $"{net45Path}{appName}.dll", assemblyPaths, true);

    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"{binDir}/{appName}.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"src/{appName}.Portable/bin/Release/{appName}.dll", Target = "lib/netstandard1.5"}
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/{appName}/package.nuspec", nuGetPackSettings);

    //var package = string.Format("./pack/{appName}.{0}.nupkg", newVersion);
    Console.WriteLine($"./pack/{appName}.{newVersion}.nupkg");

    // Push the package.
    //NuGetPush(package, new NuGetPushSettings {
    //    Source = "http://localhost:9997/api/odata",
    //    ApiKey = "4258a1b5-9b40-4d74-b731-6e2dda147450"
    //});
}

void uploadGelf4netAmqpAppender() {
    var appenderName = "AmqpAppender";
    var assemblyFile = $"./src/{appName}.{appenderName}/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;
    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Title = "${appName}.{appenderName}",
        Product = $"{appName}.{appenderName}",
        Copyright = $"Copyright © {DateTime.Now.Year}",
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    var net45Path = $"src/{appName}.{appenderName}/bin/Release/";
    var portablePath = $"src/{appName}.{appenderName}.Portable/bin/Release/";


    var assemblyPaths = new string[] {
        $"{net45Path}{appName}.Core.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{net45Path}/{appName}.{appenderName}.Merged.dll", $"{net45Path}{appName}.{appenderName}.dll", assemblyPaths, false);

    assemblyPaths = new string[] {
        $"{net45Path}Newtonsoft.Json.dll",
        $"{net45Path}RabbitMQ.Client.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{binDir}/{appName}.{appenderName}.dll", $"{net45Path}/{appName}.{appenderName}.Merged.dll", assemblyPaths, true);

//    assemblyPaths = new string[] {
//        $"{portablePath}{appName}.Core.dll"
//    }.Select(x => new FilePath(x));

//    GenerateMergedDll($"{binDirPortable}/{appName}.{appenderName}.dll", $"{portablePath}{appName}.{appenderName}.dll", assemblyPaths, false);


    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"{binDir}/{appName}.{appenderName}.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.{appenderName}.dll", Target = "lib/netstandard1.5"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.Core.dll", Target = "lib/netstandard1.5"},
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/{appName}.{appenderName}/package.nuspec", nuGetPackSettings);

    //var package = string.Format("./pack/{appName}.{0}.nupkg", newVersion);
    Console.WriteLine($"./pack/{appName}.{appenderName}.{newVersion}.nupkg");

    // Push the package.
    //NuGetPush(package, new NuGetPushSettings {
    //    Source = "http://localhost:9997/api/odata",
    //    ApiKey = "4258a1b5-9b40-4d74-b731-6e2dda147450"
    //});
}

void uploadGelf4netHttpAppender() {
    var appenderName = "HttpAppender";
    var assemblyFile = $"./src/{appName}.{appenderName}/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;
    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Title = "${appName}.{appenderName}",
        Product = $"{appName}.{appenderName}",
        Copyright = $"Copyright © {DateTime.Now.Year}",
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    var net45Path = $"src/{appName}.{appenderName}/bin/Release/";
    var portablePath = $"src/{appName}.{appenderName}.Portable/bin/Release/";


    var assemblyPaths = new string[] {
        $"{net45Path}{appName}.Core.dll"
    }.Select(x => new FilePath(x));


    GenerateMergedDll($"{net45Path}/{appName}.{appenderName}.Merged.dll", $"{net45Path}{appName}.{appenderName}.dll", assemblyPaths, false);

    assemblyPaths = new string[] {
        $"{net45Path}Newtonsoft.Json.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{binDir}/{appName}.{appenderName}.dll", $"{net45Path}/{appName}.{appenderName}.Merged.dll", assemblyPaths, true);


//    assemblyPaths = new string[] {
//        $"{portablePath}{appName}.Core.dll"
//    }.Select(x => new FilePath(x));
//
//    GenerateMergedDll($"{binDirPortable}/{appName}.{appenderName}.dll", $"{portablePath}{appName}.{appenderName}.dll", assemblyPaths, false);


    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"{binDir}/{appName}.{appenderName}.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.{appenderName}.dll", Target = "lib/netstandard1.5"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.Core.dll", Target = "lib/netstandard1.5"},
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/{appName}.{appenderName}/package.nuspec", nuGetPackSettings);

    //var package = string.Format("./pack/{appName}.{0}.nupkg", newVersion);
    Console.WriteLine($"./pack/{appName}.{appenderName}.{newVersion}.nupkg");

    // Push the package.
    //NuGetPush(package, new NuGetPushSettings {
    //    Source = "http://localhost:9997/api/odata",
    //    ApiKey = "4258a1b5-9b40-4d74-b731-6e2dda147450"
    //});
}

void uploadGelf4netUdpAppender() {
    var appenderName = "UdpAppender";
    var assemblyFile = $"./src/{appName}.{appenderName}/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;
    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Title = "${appName}.{appenderName}",
        Product = $"{appName}.{appenderName}",
        Copyright = $"Copyright © {DateTime.Now.Year}",
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    var net45Path = $"src/{appName}.{appenderName}/bin/Release/";
    var portablePath = $"src/{appName}.{appenderName}.Portable/bin/Release/";


    var assemblyPaths = new string[] {
        $"{net45Path}{appName}.Core.dll"
    }.Select(x => new FilePath(x));


    GenerateMergedDll($"{net45Path}/{appName}.{appenderName}.Merged.dll", $"{net45Path}{appName}.{appenderName}.dll", assemblyPaths, false);

    assemblyPaths = new string[] {
        $"{net45Path}Newtonsoft.Json.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{binDir}/{appName}.{appenderName}.dll", $"{net45Path}/{appName}.{appenderName}.Merged.dll", assemblyPaths, true);

//    assemblyPaths = new string[] {
//        $"{portablePath}{appName}.Core.dll"
//    }.Select(x => new FilePath(x));

//    GenerateMergedDll($"{binDirPortable}/{appName}.{appenderName}.dll", $"{portablePath}{appName}.{appenderName}.dll", assemblyPaths, false);


    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"{binDir}/{appName}.{appenderName}.dll", Target = "lib/net45"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.{appenderName}.dll", Target = "lib/netstandard1.5"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.Core.dll", Target = "lib/netstandard1.5"},
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/{appName}.{appenderName}/package.nuspec", nuGetPackSettings);

    //var package = string.Format("./pack/{appName}.{0}.nupkg", newVersion);
    Console.WriteLine($"./pack/{appName}.{appenderName}.{newVersion}.nupkg");

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
    CreateDirectory(binDirPortable);
    CreateDirectory(distDir);
    CleanDirectory(binDir);
    CleanDirectory(distDir);
    CleanDirectory(binDirPortable);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore($"./src/{appName}.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{

    if(IsRunningOnWindows())
    {
      // Use MSBuild
        MSBuild($"./src/{appName}.sln", settings =>
            settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild($"./src/{appName}.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnitSettings {
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
