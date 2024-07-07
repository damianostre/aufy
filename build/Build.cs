using System;
using System.Linq;
using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Parameter("The key to push to Nuget")]
    [Secret]
    readonly string NuGetApiKey;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    
    string FullTagVersion => GitRepository.Tags.SingleOrDefault(x => x.StartsWith("v"))?[1..];
    bool IsTaggedBuild => !string.IsNullOrWhiteSpace(FullTagVersion);
    string TagVersion => IsTaggedBuild ? FullTagVersion.Split('-')[0] : "0.0.0";
    string VersionSuffix;
    
    protected override void OnBuildInitialized()
    {
        var versionParts = FullTagVersion?.Split('-');
        VersionSuffix = !IsTaggedBuild
            ? $"preview-{DateTime.UtcNow:yyyyMMdd-HHmm}"
            : versionParts is { Length: > 1 }
                ? versionParts[1]
                : string.Empty;

        if (IsLocalBuild)
        {
            VersionSuffix = $"dev-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        }

        Log.Information("BUILD SETUP");
        Log.Information("Configuration:\t{Configuration}", Configuration);
        Log.Information("Version suffix:\t{VersionSuffix}", VersionSuffix);
        Log.Information("Tagged build:\t{IsTaggedBuild}", IsTaggedBuild);
    }
    
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
        });
    
    Target IntegrationTests => _ => _
        .After(Compile)
        .Executes(() =>
        {
            var testProjects = new[] { "TestWebApi.IntegrationTests" };
            DotNetTest(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .SetLoggers(GitHubActions.Instance is not null ? new[] { "GitHubActions" } : Array.Empty<string>())
                .CombineWith(testProjects, (_, testProject) => _
                    .SetProjectFile(Solution.GetAllProjects(testProject).First())
                )
            );
        });
    
    Target Pack => _ => _
        .After(Compile, IntegrationTests)
        .DependsOn(IntegrationTests)
        // .OnlyWhenDynamic(() => RunAllTargets || HasSourceChanges)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution.GetProject("Aufy.Core"))
                .SetAssemblyVersion(TagVersion)
                .SetFileVersion(TagVersion)
                .SetInformationalVersion(TagVersion)
                .SetVersionSuffix(VersionSuffix)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
            
            DotNetPack(s => s
                .SetProject(Solution.GetProject("Aufy.EntityFrameworkCore"))
                .SetAssemblyVersion(TagVersion)
                .SetFileVersion(TagVersion)
                .SetInformationalVersion(TagVersion)
                .SetVersionSuffix(VersionSuffix)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
            
            DotNetPack(s => s
                .SetProject(Solution.GetProject("Aufy.FluentEmail"))
                .SetAssemblyVersion(TagVersion)
                .SetFileVersion(TagVersion)
                .SetInformationalVersion(TagVersion)
                .SetVersionSuffix(VersionSuffix)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
        });
    
    Target Publish => _ => _
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => IsTaggedBuild)
        .ProceedAfterFailure()
        .Executes(() =>
        {
            var packages = ArtifactsDirectory.GlobFiles("*.nupkg");

            Assert.NotEmpty(packages);

            DotNetNuGetPush(s => s
                .SetApiKey(NuGetApiKey)
                .EnableSkipDuplicate()
                .SetSource("https://api.nuget.org/v3/index.json")
                .EnableNoSymbols()
                .CombineWith(packages,
                    (v, path) => v.SetTargetPath(path)));
        });

}
