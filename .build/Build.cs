using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
partial class ReactiveMarblesBuild : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<ReactiveMarblesBuild>(x => x.Default);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
            DotNetRestore(configure => configure
                    .SetProjectFile(Solution)));

    Target Build => _ => _
        .DependsOn(Restore)
        .DependsOn(Nerdbank)
        .Executes(() =>
        {
            DotNetTasks
                .DotNetBuild(configure => configure
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetVerbosity(DotNetVerbosity.Minimal));
        });

    Target Nerdbank => _ => _
        .DependsOn(Clean)
        .DependsOn(NerdbankIntegrationVersion)
        .Before(Build)
        .Executes(() =>
            NerdbankGitVersioningTasks
                .NerdbankGitVersioningGetVersion(configure =>
                    configure
                        .SetProcessWorkingDirectory("./")
                        .SetFormat(NerdbankGitVersioningFormat.json)
                        .EnableProcessLogOutput()));

    Target NerdbankIntegrationVersion => _ => _
        .DependsOn(Clean)
        .OnlyWhenStatic(() => IsServerBuild)
        .Executes(() =>
            NerdbankGitVersioningTasks
                .NerdbankGitVersioningCloud(configure =>
                    configure
                        .EnableAllVars()
                        .SetProject(Solution.Directory)
                        .SetCISystem(NerdbankGitVersioningCISystem.GitHubActions)));

    Target Test => _ => _
        .DependsOn(Build)
        .Executes(() =>
        {
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .DependsOn(Nerdbank)
        .Executes(() =>
            DotNetPack(configure =>
                configure
                    .SetProject(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetVerbosity(DotNetVerbosity.Minimal)));

    Target Default => _ => _
        .DependsOn(Clean)
        .DependsOn(Nerdbank)
        .DependsOn(Restore)
        .DependsOn(Build)
        .DependsOn(Pack);
}
