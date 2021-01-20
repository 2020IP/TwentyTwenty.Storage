#tool nuget:?package=GitVersion.CommandLine&version=5.0.1

GitVersion versionInfo = null;
var target = Argument("target", "Default");
var outputDir = "./artifacts/";
var configuration   = Argument("configuration", "Release");

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
        DotNetCoreBuild(".", new DotNetCoreBuildSettings
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
        DotNetCorePack("src/TwentyTwenty.Storage.Local/", settings);

        if (AppVeyor.IsRunningOnAppVeyor)
        {
            foreach (var file in GetFiles(outputDir + "**/*"))
                AppVeyor.UploadArtifact(file.FullPath);
        }
    });

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);