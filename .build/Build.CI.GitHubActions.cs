using Nuke.Common.CI.GitHubActions;

[GitHubActions("build",
    GitHubActionsImage.WindowsLatest,
    OnPushBranches = new[] { "main" },
    OnPullRequestBranches = new []{"main"},
    AutoGenerate = true)]
partial class ReactiveMarblesBuild
{
}
