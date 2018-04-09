#tool nuget:?package=GitVersion.CommandLine&version=3.6.5
#addin nuget:?package=Newtonsoft.Json&version=9.0.1

GitVersion versionInfo = null;
var target = Argument("target", "Default");
var outputDir = "./artifacts/";
var configuration   = Argument("configuration", "Release");

Task("Clean")
    .Does(() => {
        if (DirectoryExists(outputDir))
        {
            DeleteDirectory(outputDir, recursive:true);
        }
        CreateDirectory(outputDir);
    });

Task("Version")
    .Does(() => {
        GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo = true,
            OutputType = GitVersionOutput.BuildServer
        });
        versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
        Information(Newtonsoft.Json.JsonConvert.SerializeObject(versionInfo, Newtonsoft.Json.Formatting.Indented));
    });

Task("Restore")
    .IsDependentOn("Version")
    .Does(() => {        
        DotNetCoreRestore(new DotNetCoreRestoreSettings
        {
            ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.NuGetVersion)
        });        
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() => {
        DotNetCoreBuild(".", new DotNetCoreBuildSettings
        {
            NoRestore = true,
            Configuration = configuration,
            ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.NuGetVersion)
        });
    });
Task("Test")
    .IsDependentOn("Build")
    .Does(() => {
        var testProjects = GetFiles("./test/**/*.csproj");
        foreach(var proj in testProjects)
        {
            DotNetCoreTest(proj.FullPath, new DotNetCoreTestSettings
            {
                Configuration = configuration,
                NoBuild = true,
            });
        }
    });

Task("Package")
    .IsDependentOn("Test")
    .Does(() => {
        var settings = new DotNetCorePackSettings
        {
            OutputDirectory = outputDir,
            NoBuild = true,
            Configuration = configuration,
            ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.NuGetVersion)
        };

        DotNetCorePack("src/TwentyTwenty.Storage/", settings);
        DotNetCorePack("src/TwentyTwenty.Storage.Amazon/", settings);
        DotNetCorePack("src/TwentyTwenty.Storage.Azure/", settings);
        DotNetCorePack("src/TwentyTwenty.Storage.Google/", settings);

        if (AppVeyor.IsRunningOnAppVeyor)
        {
            foreach (var file in GetFiles(outputDir + "**/*"))
                AppVeyor.UploadArtifact(file.FullPath);
        }
    });

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);