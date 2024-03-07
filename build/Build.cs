using System;
using System.Collections.Generic;
using System.Linq;
using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using CreativeCoders.NukeBuild.BuildActions;
using CreativeCoders.NukeBuild.Components.Parameters;
using CreativeCoders.NukeBuild.Components.Targets;
using CreativeCoders.NukeBuild.Components.Targets.Settings;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

#pragma warning disable S1144 // remove unused private members
#pragma warning disable S3903 // move class to namespace
[PublicAPI]
[UnsetVisualStudioEnvironmentVariables]
[GitHubActions("integration", GitHubActionsImage.UbuntuLatest,
    OnPushBranches = ["feature/**"],
    InvokedTargets = [NukeTargets.DeployNuGet],
    EnableGitHubToken = true,
    PublishArtifacts = true,
    FetchDepth = 0
)]
[GitHubActions("pull-request", GitHubActionsImage.UbuntuLatest, GitHubActionsImage.WindowsLatest,
    OnPullRequestBranches = ["main"],
    InvokedTargets = [NukeTargets.Rebuild, NukeTargets.CodeCoverage, NukeTargets.Pack],
    EnableGitHubToken = true,
    PublishArtifacts = true,
    FetchDepth = 0
)]
[GitHubActions("main", GitHubActionsImage.UbuntuLatest,
    OnPushBranches = ["main"],
    InvokedTargets = [NukeTargets.DeployNuGet],
    EnableGitHubToken = true,
    PublishArtifacts = true,
    FetchDepth = 0
)]
[GitHubActions(ReleaseWorkflow, GitHubActionsImage.UbuntuLatest,
    OnPushTags = ["v**"],
    InvokedTargets = [NukeTargets.DeployNuGet],
    ImportSecrets = ["NUGET_ORG_TOKEN"],
    EnableGitHubToken = true,
    PublishArtifacts = true,
    FetchDepth = 0
)]
class Build : NukeBuild,
    IGitRepositoryParameter,
    IConfigurationParameter,
    IGitVersionParameter,
    ISourceDirectoryParameter,
    IArtifactsSettings,
    ICleanTarget, IBuildTarget, IRestoreTarget, ICodeCoverageTarget, IPushNuGetTarget, IRebuildTarget,
    IDeployNuGetTarget
{
    public const string ReleaseWorkflow = "release";

    readonly GitHubActions GitHubActions = GitHubActions.Instance;

    [Parameter(Name = "GITHUB_TOKEN")] string DevNuGetApiKey;

    [Parameter(Name = "NUGET_ORG_TOKEN")] string NuGetOrgApiKey;

    IList<AbsolutePath> ICleanSettings.DirectoriesToClean =>
        this.As<ICleanSettings>().DefaultDirectoriesToClean
            .AddRange(this.As<ITestSettings>().TestBaseDirectory);

    public IEnumerable<Project> TestProjects => GetTestProjects();

    bool IPushNuGetSettings.SkipPush => GitHubActions?.IsPullRequest == true;

    string IPushNuGetSettings.NuGetFeedUrl =>
        GitHubActions?.Workflow == ReleaseWorkflow
            ? "nuget.org"
            : "https://nuget.pkg.github.com/CreativeCodersTeam/index.json";

    string IPushNuGetSettings.NuGetApiKey =>
        GitHubActions?.Workflow == ReleaseWorkflow
            ? NuGetOrgApiKey
            : DevNuGetApiKey;

    string IPackSettings.PackageProjectUrl => "https://github.com/CreativeCodersTeam/Core";

    string IPackSettings.PackageLicenseExpression => PackageLicenseExpressions.ApacheLicense20;

    string IPackSettings.Copyright => $"{DateTime.Now.Year} CreativeCoders";

    Project[] GetTestProjects() =>
        this.TryAs<ISolutionParameter>(out var solutionParameter)
            ? solutionParameter.Solution.GetAllProjects("*")
                .Where(x => ((string)x.Path)?.StartsWith(RootDirectory / "tests") ?? false).ToArray()
            : Array.Empty<Project>();

    public static int Main() => Execute<Build>(x => ((ICodeCoverageTarget)x).CodeCoverage);
}
