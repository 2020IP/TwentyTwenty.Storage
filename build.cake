#tool "dotnet:?package=GitVersion.Tool"

GitVersion versionInfo = null;
var target = Argument("target", "Default");
var outputDir = "./artifacts/";
var configuration = Argument("configuration", "Release");
var nugetFeedUrl = "https://api.nuget.org/v3/index.json";
var nugetApiKey = EnvironmentVariable("Nuget_ApiKey");

Task("Clean")
    .Does(() => {
        if (DirectoryExists(outputDir))
        {
            DeleteDirectory(outputDir, new DeleteDirectorySettings { Recursive = true });
        }
        CreateDirectory(outputDir);
    });

Task("Version")
    .IsDependentOn("Clean")
    .Does(() => {
        GitVersion(new GitVersionSettings
        {
            UpdateAssemblyInfo = true,
            OutputType = GitVersionOutput.BuildServer,
            NoFetch = true,
        });

        versionInfo = GitVersion(new GitVersionSettings
        { 
            OutputType = GitVersionOutput.Json,
            NoFetch = true
        });
    });

Task("Build")
    .IsDependentOn("Version")
    .Does(() => {
        DotNetBuild(".", new DotNetBuildSettings
        {
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
            DotNetTest(proj.FullPath, new DotNetTestSettings
            {
                Configuration = configuration,
                NoBuild = true,
            });
        }
    });

Task("Package")
    .IsDependentOn("Test")
    .Does(() => {
        var settings = new DotNetPackSettings
        {
            OutputDirectory = outputDir,
            NoBuild = true,
            Configuration = configuration,
            ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.NuGetVersion)
        };

        DotNetPack("src/TwentyTwenty.Storage/", settings);
        DotNetPack("src/TwentyTwenty.Storage.Amazon/", settings);
        DotNetPack("src/TwentyTwenty.Storage.Azure/", settings);
        DotNetPack("src/TwentyTwenty.Storage.Google/", settings);
        DotNetPack("src/TwentyTwenty.Storage.Local/", settings);
    });

Task("Package")
    .IsDependentOn("Test")
    .Does(() => {
        var settings = new DotNetPackSettings
        {
            OutputDirectory = outputDir,
            NoBuild = true,
            Configuration = configuration,
            ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.NuGetVersion)
        };

        DotNetPack("src/TwentyTwenty.Storage/", settings);
        DotNetPack("src/TwentyTwenty.Storage.Amazon/", settings);
        DotNetPack("src/TwentyTwenty.Storage.Azure/", settings);
        DotNetPack("src/TwentyTwenty.Storage.Google/", settings);
        DotNetPack("src/TwentyTwenty.Storage.Local/", settings);
    });

Task("Publish")
    .IsDependentOn("Package")
    .Does(() => {
        var settings = new DotNetCoreNuGetPushSettings
        {
            Source = nugetFeedUrl,
            ApiKey = nugetApiKey,
        };

        DotNetNuGetPush(outputDir + "*.nupkg", settings);
    });

Task("Default")
    .IsDependentOn("Publish");

RunTarget(target);