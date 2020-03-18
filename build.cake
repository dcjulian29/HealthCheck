#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=OpenCover&version=4.7.922"
#tool "nuget:?package=ReportGenerator&version=4.5.0"
#tool "nuget:?package=GitVersion.CommandLine&version=3.6.5"

var target = Argument("target", "Default");

var configuration = Argument("configuration", "Debug");

if ((target == "Default") && (TeamCity.IsRunningOnTeamCity)) {
    target = "TeamCity";
    configuration = "Release";
}

if ((target == "Default") && (AppVeyor.IsRunningOnAppVeyor)) {
    target = "AppVeyor";
    configuration = "Release";
}

var projectName = "HealthCheck";

var baseDirectory = MakeAbsolute(Directory("."));

var buildDirectory = baseDirectory + "\\.build";
var outputDirectory = buildDirectory + "\\output";
var packageDirectory = baseDirectory + "\\packages";

var solutionFile = String.Format("{0}\\{1}.sln", baseDirectory, projectName);

IEnumerable<string> stdout;
StartProcess ("git", new ProcessSettings {
    Arguments = "describe --tags --abbrev=0",
    RedirectStandardOutput = true,
}, out stdout);
List<String> result = new List<string>(stdout);
var version = String.IsNullOrEmpty(result[0]) ? "0.0.0" : result[0];

StartProcess ("git", new ProcessSettings {
    Arguments = "rev-parse --short=8 HEAD",
    RedirectStandardOutput = true,
}, out stdout);
result = new List<string>(stdout);
var packageId = String.IsNullOrEmpty(result[0]) ? "unknown" : result[0];

var branch = "unknown";
if (AppVeyor.IsRunningOnAppVeyor) {
    branch = EnvironmentVariable("APPVEYOR_REPO_BRANCH");

    if (branch != "master") {
        AppVeyor.UpdateBuildVersion($"{version}-{branch}.{packageId}");
    } else {
        AppVeyor.UpdateBuildVersion(version);
    }
} else {
    StartProcess ("git", new ProcessSettings {
        Arguments = "symbolic-ref --short HEAD",
        RedirectStandardOutput = true,
    }, out stdout);
    result = new List<string>(stdout);
    branch = String.IsNullOrEmpty(result[0]) ? "unknown" : result[0];
}

if (branch != "master") {
    version = $"{version}-{branch}.{packageId}";
}

var msbuildSettings = new MSBuildSettings {
    Configuration = configuration,
    ToolVersion = MSBuildToolVersion.VS2019,
    NodeReuse = false,
    WarningsAsError = false
}.WithProperty("OutDir", outputDirectory);

Setup(setupContext =>
{
    if (setupContext.TargetTask.Name == "Package")
    {
        Information("Switching to Release Configuration for packaging...");
        configuration = "Release";

        msbuildSettings.Configuration = "Release";
    }
});

TaskSetup(setupContext =>
{
    if (TeamCity.IsRunningOnTeamCity)
    {
        TeamCity.WriteStartBuildBlock(setupContext.Task.Description ?? setupContext.Task.Name);
        TeamCity.WriteStartProgress(setupContext.Task.Description ?? setupContext.Task.Name);
    }
});

TaskTeardown(teardownContext =>
{
    if (TeamCity.IsRunningOnTeamCity)
    {
        TeamCity.WriteEndBuildBlock(teardownContext.Task.Description ?? teardownContext.Task.Name);
        TeamCity.WriteEndProgress(teardownContext.Task.Description ?? teardownContext.Task.Name);
    }
});

Task("Default")
    .IsDependentOn("Compile");

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(buildDirectory);
        MSBuild(solutionFile, msbuildSettings.WithTarget("Clean"));
    });

Task("Init")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        CreateDirectory(buildDirectory);
        CreateDirectory(outputDirectory);
    });

Task("Version")
    .IsDependentOn("Init")
    .Does(() =>
    {
        var gitversion = GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo = false,
            OutputType = GitVersionOutput.Json
        });

        Information("Marking this build as version: " + version);

        var assemblyVersion = "0.0.0";;

        if (branch == "master") {
            assemblyVersion = version;
        }

        CreateAssemblyInfo(buildDirectory + @"\CommonAssemblyInfo.cs", new AssemblyInfoSettings {
            Version = assemblyVersion,
            FileVersion = assemblyVersion,
            InformationalVersion = gitversion.FullBuildMetaData,
            Copyright = String.Format("(c) Julian Easterling {0}", DateTime.Now.Year),
            Company = String.Empty,
            Configuration = configuration
        });
    });

Task("PackageClean")
    .Does(() =>
    {
        CleanDirectories(packageDirectory);
    });

Task("PackageRestore")
    .IsDependentOn("Init")
    .Does(() =>
    {
        NuGetRestore(solutionFile);

        // In a CI environment, there really isn't any value to the packages' PDB files and it confuses the code coverage task
        var files = GetFiles(packageDirectory + "/**/*.pdb");
        DeleteFiles(files);
    });

Task("Compile")
    .IsDependentOn("PackageRestore")
    .IsDependentOn("Version")
    .Does(() =>
    {
        MSBuild(solutionFile, msbuildSettings.WithTarget("ReBuild"));
    });

Task("Test")
    .IsDependentOn("UnitTest");

Task("UnitTest")
    .IsDependentOn("Compile")
    .Does(() =>
    {
        var dotNetTestSettings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
            Logger = "console;verbosity=normal"
        };

        DotNetCoreTest(outputDirectory + "\\UnitTests.dll", dotNetTestSettings);
    });

Task("Coverage")
    .IsDependentOn("Compile")
    .Does(() =>
    {
        CreateDirectory(buildDirectory + "\\coverage");

        var dotNetTestSettings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
            Logger = "console;verbosity=normal"
        };

        var openCoverSettings = new OpenCoverSettings() { Register = "user" }
                .WithFilter(@"+[*]*")
                .WithFilter(@"-[UnitTests]*")
                .WithFilter(@"-[xunit.*]*")
                .ExcludeByAttribute("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute")
                .ExcludeByFile(@"*\\*Designer.cs;*\\*.g.cs;*.*.g.i.cs");

        OpenCover(context => context.DotNetCoreTest(outputDirectory + "\\UnitTests.dll", dotNetTestSettings),
            buildDirectory + "\\coverage\\coverage.xml", openCoverSettings);

        ReportGenerator(buildDirectory + "\\coverage\\coverage.xml", buildDirectory + "\\coverage");
    });

Task("TeamCity")
    .Does(() =>
    {
        if (DirectoryExists(baseDirectory + "\\UnitTests")) {
            RunTarget("coverage");

            // Write class coverage
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageAbsCCovered' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@visitedClasses")));
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageAbsCTotal' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@numClasses")));
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageC' value='{0:N2}']",
                (
                    Convert.ToDouble(XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@visitedClasses")) /
                    Convert.ToDouble(XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@numClasses"))
                ) * 100));

            // Report method coverage
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageAbsMCovered' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@visitedMethods")));
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageAbsMTotal' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@numMethods")));
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageM' value='{0:N2}']",
                (
                    Convert.ToDouble(XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@visitedMethods")) /
                    Convert.ToDouble(XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@numMethods"))
                ) * 100));

            // Report branch coverage
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageAbsBCovered' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@visitedBranchPoints")));
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageAbsBTotal' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@numBranchPoints")));
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageB' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@branchCoverage")));

            // Report statement coverage
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageAbsSCovered' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@visitedSequencePoints")));
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageAbsSTotal' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@numSequencePoints")));
            Information(String.Format(
                "##teamcity[buildStatisticValue key='CodeCoverageS' value='{0}']",
                XmlPeek(buildDirectory + "\\coverage\\coverage.xml", "/CoverageSession/Summary/@sequenceCoverage")));
        } else {
            RunTarget("default");
        }
    });

Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
    {
        CreateDirectory(buildDirectory + "\\packages");

        var nuGetPackSettings = new NuGetPackSettings {
            Version = version,
            OutputDirectory = buildDirectory + "\\packages"
        };

        var nuspecFiles = GetFiles(baseDirectory + "\\*.nuspec");

        NuGetPack(nuspecFiles, nuGetPackSettings);
    });

Task("AppVeyor")
    .IsDependentOn("Package")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
    {
        GetFiles(buildDirectory + "\\packages\\*.nupkg")
            .ToList()
            .ForEach(f => AppVeyor.UploadArtifact(f, new AppVeyorUploadArtifactsSettings { DeploymentName = "packages" }));
    });

RunTarget(target);
