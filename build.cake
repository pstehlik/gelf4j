#tool nuget:?package=NUnit.Runners&version=2.6.4
#tool nuget:?package=ilmerge
#addin "Cake.FileHelpers"

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

void GenerateMergedDll(string outputFile, string primaryDll, IEnumerable<FilePath> assemblyPaths, bool internalize)
{
    ILMerge(outputFile, 
            primaryDll, 
            assemblyPaths, 
            new ILMergeSettings { Internalize = internalize });
}

void PushPackage(string packageName, string newVersion)
{
    Console.WriteLine("push " + newVersion);
/*
    var packagePath = $"{distDir}/{packageName}.{newVersion}.nupkg";
    var apiKey = FileReadText(File("./private/nugetapikey.txt"));
    Console.WriteLine(apiKey);
    // Push the package.
    NuGetPush(packagePath, new NuGetPushSettings {
        Source = "https://www.nuget.org/api/v2/package",
        ApiKey = apiKey
    });
*/
}

string getAssemblyVersionFile(string packageName)
{
    var assemblyFile = $"./src/{packageName}/Properties/AssemblyInfo.cs";
    var assemblyInfo = ParseAssemblyInfo(assemblyFile);
    var version = assemblyInfo.AssemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]);

    return string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);
}

string updateAssemblyFile(string packageName)
{
    var assemblyFile = $"./src/{packageName}/Properties/AssemblyInfo.cs";

    var assemblyVersion = getAssemblyVersionFile(packageName);
    var version = assemblyVersion.Split('.');
    var buildNumber = int.Parse(version[3]) + 1;

    var newVersion = string.Format("{0}.{1}.{2}.{3}", version[0], version[1], version[2], buildNumber);

    CreateAssemblyInfo(assemblyFile, new AssemblyInfoSettings {
        Title = $"{packageName}",
        Product = $"{packageName}",
        Copyright = $"MIT {DateTime.Now.Year}",
        Version = newVersion,
        FileVersion = newVersion,
        InformationalVersion = newVersion
    });

    return newVersion;
}

void uploadGelf4net(bool upload = true)
{
    var packageName = appName;
    var newVersion = getAssemblyVersionFile(packageName);

    if(upload){
        newVersion = updateAssemblyFile(packageName);
    }

    var net45Path = $"src/{packageName}/bin/Release/";

    var assemblyPaths = new string[] {
        $"{net45Path}Newtonsoft.Json.dll",
        $"{net45Path}RabbitMQ.Client.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{binDir}/{packageName}.dll", $"{net45Path}{packageName}.dll", assemblyPaths, true);

    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"{binDir}/{packageName}.dll", Target = @"lib\net45"},
                                        new NuSpecContent {Source = $"src/{packageName}.Portable/bin/Release/{packageName}.dll", Target = @"lib\netstandard1.5"}
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/{packageName}/package.nuspec", nuGetPackSettings);

    if(upload){
        PushPackage(packageName, newVersion);
    }
}

void uploadGelf4netAmqpAppender(bool upload = true)
{
    var packageName = $"{appName}.AmqpAppender";

    var newVersion = getAssemblyVersionFile(packageName);

    if(upload){
        newVersion = updateAssemblyFile(packageName);
    }

    var net45Path = $"src/{packageName}/bin/Release/";
    var portablePath = $"src/{packageName}.Portable/bin/Release/";


    var assemblyPaths = new string[] {
        $"{net45Path}{appName}.Core.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{net45Path}/{packageName}.Merged.dll", $"{net45Path}{packageName}.dll", assemblyPaths, false);

    assemblyPaths = new string[] {
        $"{net45Path}Newtonsoft.Json.dll",
        $"{net45Path}RabbitMQ.Client.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{binDir}/{packageName}.dll", $"{net45Path}/{packageName}.Merged.dll", assemblyPaths, true);


    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"{binDir}/{packageName}.dll", Target = @"lib\net45"},
                                        new NuSpecContent {Source = $"{portablePath}{packageName}.dll", Target = @"lib\netstandard1.5"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.Core.dll", Target = @"lib\netstandard1.5"},
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/{packageName}/package.nuspec", nuGetPackSettings);

    if(upload){
        PushPackage(packageName, newVersion);
    }
}


void uploadGelf4netHttpAppender(bool upload = true)
{
    var packageName = $"{appName}.HttpAppender";

    var newVersion = getAssemblyVersionFile(packageName);

    if(upload){
        newVersion = updateAssemblyFile(packageName);
    }

    var net45Path = $"src/{packageName}/bin/Release/";
    var portablePath = $"src/{packageName}.Portable/bin/Release/";

    var assemblyPaths = new string[] {
        $"{net45Path}{appName}.Core.dll"
    }.Select(x => new FilePath(x));


    GenerateMergedDll($"{net45Path}/{packageName}.Merged.dll", $"{net45Path}{packageName}.dll", assemblyPaths, false);

    assemblyPaths = new string[] {
        $"{net45Path}Newtonsoft.Json.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{binDir}/{packageName}.dll", $"{net45Path}/{packageName}.Merged.dll", assemblyPaths, true);

    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"{binDir}/{packageName}.dll", Target = @"lib\net45"},
                                        new NuSpecContent {Source = $"{portablePath}{packageName}.dll", Target = @"lib\netstandard1.5"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.Core.dll", Target = @"lib\netstandard1.5"},
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/{packageName}/package.nuspec", nuGetPackSettings);

    if(upload){
        PushPackage(packageName, newVersion);
    }
}

void uploadGelf4netUdpAppender(bool upload = true)
{
    var packageName = $"{appName}.UdpAppender";

    var newVersion = getAssemblyVersionFile(packageName);
    if(upload){
        newVersion = updateAssemblyFile(packageName);
    }

    var appenderName = "UdpAppender";
    
    var net45Path = $"src/{packageName}/bin/Release/";
    var portablePath = $"src/{packageName}.Portable/bin/Release/";


    var assemblyPaths = new string[] {
        $"{net45Path}{appName}.Core.dll"
    }.Select(x => new FilePath(x));


    GenerateMergedDll($"{net45Path}/{packageName}.Merged.dll", $"{net45Path}{packageName}.dll", assemblyPaths, false);

    assemblyPaths = new string[] {
        $"{net45Path}Newtonsoft.Json.dll"
    }.Select(x => new FilePath(x));

    GenerateMergedDll($"{binDir}/{packageName}.dll", $"{net45Path}/{packageName}.Merged.dll", assemblyPaths, true);

    var nuGetPackSettings   = new NuGetPackSettings {
                                     Version =  newVersion,
                                     Files = new [] {
                                        new NuSpecContent {Source = $"{binDir}/{packageName}.dll", Target = @"lib\net45"},
                                        new NuSpecContent {Source = $"{portablePath}{packageName}.dll", Target = @"lib\netstandard1.5"},
                                        new NuSpecContent {Source = $"{portablePath}{appName}.Core.dll", Target = @"lib\netstandard1.5"},
                                    },
                                     BasePath   = ".",
                                     OutputDirectory = distDir
                                 };

    NuGetPack($"./src/{packageName}/package.nuspec", nuGetPackSettings);

    if(upload){
        PushPackage(packageName, newVersion);
    }
}

void Build()
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      //ToolPath hardcoded until https://github.com/cake-build/cake/pull/1520 is accepted to use vswhere
        MSBuild($"./src/{appName}.sln", new MSBuildSettings {
            Configuration = "Release",
            ToolPath = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
        });
    }
    else
    {
      // Use XBuild
      XBuild($"./src/{appName}.sln", settings =>
        settings.SetConfiguration(configuration));
    }
}

void RunTests()
{
    NUnit("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnitSettings {
        NoResults = true
    });
}

void BuildUploadPackage(bool uploadPackage)
{
    uploadGelf4net(uploadPackage);
    uploadGelf4netAmqpAppender(uploadPackage);
    uploadGelf4netHttpAppender(uploadPackage);
    uploadGelf4netUdpAppender(uploadPackage);
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
    //.IsDependentOn("UpdateVersion")
    .Does(() =>
{
    Build();
});

Task("UpdateVersion")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() => {

    var packageName = $"{appName}";
    updateAssemblyFile(packageName);

    packageName = $"{appName}.AmqpAppender";
    updateAssemblyFile(packageName);

    packageName = $"{appName}.HttpAppender";
    updateAssemblyFile(packageName);

    packageName = $"{appName}.UdpAppender";
    updateAssemblyFile(packageName);

});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    RunTests();
});


Task("BuildPackage")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    BuildUploadPackage(false);
});

Task("PushToNuget")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    BuildUploadPackage(true);

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
