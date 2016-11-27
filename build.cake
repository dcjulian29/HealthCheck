#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=OpenCover"
#tool "nuget:?package=ReportGenerator"

var target = Argument("target", "Default");

if (TeamCity.IsRunningOnTeamCity) {
    target = "teamcity";
}

var configuration = Argument("configuration", "Release");

var projectName = "HealthCheck";

var baseDirectory = MakeAbsolute(Directory("."));

var buildDirectory = baseDirectory + "\\build";
var outputDirectory = buildDirectory + "\\output";
var packageDirectory = baseDirectory + "\\packages";

var solutionFile = String.Format("{0}\\{1}.sln", baseDirectory, projectName);

IEnumerable<string> stdout;
StartProcess ("git", new ProcessSettings {
    Arguments = "describe --tags --abbrev=0",
    RedirectStandardOutput = true,
}, out stdout);
List<String> result = new List<string>(stdout);
var version = result.Count == 0 ? "0.0.0" : result[0];

stdout = Enumerable.Empty<string>();
result.Clear();

StartProcess ("git", new ProcessSettings {
    Arguments = "rev-parse --abbrev-ref HEAD",
    RedirectStandardOutput = true,
}, out stdout);
result = new List<string>(stdout);
var branch = result.Count == 0 ? "undetermined" : result[0];

Task("Default")
    .IsDependentOn("Compile");

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(buildDirectory);
        MSBuild(solutionFile, new MSBuildSettings {
            Configuration = configuration,
            }.WithTarget("Clean"));
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
        Information("MARKING THIS BUILD AS VERSION " + version);

        CreateAssemblyInfo(buildDirectory + @"\CommonAssemblyInfo.cs", new AssemblyInfoSettings {
            Version = version,
            FileVersion = version,
            InformationalVersion = version,
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
        MSBuild(solutionFile, new MSBuildSettings()
            .SetConfiguration(configuration)
            .WithProperty("OutDir", outputDirectory)
            .WithProperty("TreatWarningsAsErrors", "True")
            .UseToolVersion(MSBuildToolVersion.VS2015)
            .SetNodeReuse(false));
    });

Task("Test")
    .IsDependentOn("UnitTest");

Task("UnitTest")
    .IsDependentOn("Compile")
    .Does(() =>
    {
        XUnit2(outputDirectory + "\\UnitTests.dll",
            new XUnit2Settings {
                Parallelism = ParallelismOption.All,
                ShadowCopy = false
            });
    });

Task("Coverage")
    .IsDependentOn("Compile")
    .Does(() =>
    {
        CreateDirectory(buildDirectory + "\\coverage");

        OpenCover(tool => {
            tool.XUnit2(outputDirectory + "\\UnitTests.dll",
                new XUnit2Settings {
                    Parallelism = ParallelismOption.All,
                    ShadowCopy = false });
            },
            new FilePath(buildDirectory + "\\coverage\\coverage.xml"),
            new OpenCoverSettings() { Register = "user" }
                .WithFilter(@"+[*]*")
                .WithFilter(@"-[UnitTests]*")
                .ExcludeByAttribute("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute")
                .ExcludeByFile(@"*\\*Designer.cs;*\\*.g.cs;*.*.g.i.cs"));

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
        var nuGetPackSettings = new NuGetPackSettings {
            Version = version,
            OutputDirectory = buildDirectory
        };

        var nuspecFiles = GetFiles(baseDirectory + "\\*.nuspec");

        NuGetPack(nuspecFiles, nuGetPackSettings);
    });

RunTarget(target);
