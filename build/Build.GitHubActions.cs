﻿using Nuke.Common.CI.GitHubActions;

[GitHubActions(
    "build",
    GitHubActionsImage.Ubuntu2204,
    OnPushBranches = ["main"],
    OnPushTags = ["v*.*.*"],
    OnPushIncludePaths = ["**/*"],
    OnPushExcludePaths = ["docs/**/*", "package.json", "package-lock.json", "readme.md"],
    PublishArtifacts = true,
    InvokedTargets = [nameof(Compile), nameof(Publish)],
    ImportSecrets = [nameof(NuGetApiKey)],
    CacheKeyFiles = []
)]
public partial class Build;