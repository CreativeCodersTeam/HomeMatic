using System;
using System.Diagnostics.CodeAnalysis;
using CreativeCoders.NukeBuild;
using CreativeCoders.NukeBuild.BuildActions;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;

[PublicAPI]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[SuppressMessage("ReSharper", "ConvertToAutoProperty")]
class Build : NukeBuild, IBuildInfo
{
    public static int Main () => Execute<Build>(x => x.RunBuild);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Parameter] string DevNuGetSource;
    
    [Parameter] string DevNuGetApiKey;
    
    [Parameter] string NuGetSource;
    
    [Parameter] string NuGetApiKey;

    [Solution] readonly Solution Solution;

    [GitRepository] readonly GitRepository GitRepository;

    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "source";

    AbsolutePath ArtifactsDirectory => RootDirectory / ".artifacts";

    AbsolutePath TestBaseDirectory => RootDirectory / ".tests";

    AbsolutePath TestResultsDirectory => TestBaseDirectory / "results";

    AbsolutePath TestProjectsBasePath => SourceDirectory / "UnitTests";

    AbsolutePath CoverageDirectory => TestBaseDirectory / "coverage";

    AbsolutePath TempNukeDirectory => RootDirectory / ".nuke" / "temp";

    const string PackageProjectUrl = "https://github.com/CreativeCodersTeam/HomeMatic";

    Target Clean => _ => _
        .Before(Restore)
        .UseBuildAction<CleanBuildAction>(this);

    Target Restore => _ => _
        .Before(Compile)
        .UseBuildAction<RestoreBuildAction>(this);

    Target Compile => _ => _
        .After(Clean)
        .UseBuildAction<DotNetCompileBuildAction>(this);

    Target Test => _ => _
        .After(Compile)
        .UseBuildAction<DotNetTestAction>(this,
            x => x
                .SetTestProjectsBaseDirectory(TestProjectsBasePath)
                .SetProjectsPattern("**/*.csproj")
                .SetResultsDirectory(TestResultsDirectory)
                .UseLogger("trx")
                .SetResultFileExt("trx")
                .EnableCoverage()
                .SetCoverageDirectory(CoverageDirectory));

    Target Pack => _ => _
        .After(Compile)
        .UseBuildAction<PackBuildAction>(this, x => x
            .SetPackageLicenseExpression(PackageLicenseExpressions.ApacheLicense20)
            .SetPackageProjectUrl(PackageProjectUrl)
            .SetCopyright($"{DateTime.Now.Year} CreativeCoders")
            .SetEnableNoBuild(false));

    Target PushToDevNuGet => _ => _
        .Requires(() => DevNuGetSource)
        .Requires(() => DevNuGetApiKey)
        .UseBuildAction<PushBuildAction>(this,
            x => x
                .SetSource(DevNuGetSource)
                .SetApiKey(DevNuGetApiKey));

    Target PushToNuGet => _ => _
        .Requires(() => NuGetApiKey)
        .UseBuildAction<PushBuildAction>(this,
            x => x
                .SetSource(NuGetSource)
                .SetApiKey(NuGetApiKey));

    Target RunBuild => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .DependsOn(Compile);

    Target RunTest => _ => _
        .DependsOn(RunBuild)
        .DependsOn(Test);

    Target CreateNuGetPackages => _ => _
        .DependsOn(RunTest)
        .DependsOn(Pack);

    Target DeployToDevNuGet => _ => _
        .DependsOn(CreateNuGetPackages)
        .DependsOn(PushToDevNuGet);

    Target DeployToNuGet => _ => _
        .DependsOn(CreateNuGetPackages)
        .DependsOn(PushToNuGet);

    string IBuildInfo.Configuration => Configuration;

    Solution IBuildInfo.Solution => Solution;

    GitRepository IBuildInfo.GitRepository => GitRepository;

    IVersionInfo IBuildInfo.VersionInfo => new GitVersionWrapper(GitVersion, "0.0.0", 1);

    AbsolutePath IBuildInfo.SourceDirectory => SourceDirectory;

    AbsolutePath IBuildInfo.ArtifactsDirectory => ArtifactsDirectory;
}
