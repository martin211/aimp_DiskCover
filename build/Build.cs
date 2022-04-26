using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Execution;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.IO.PathConstruction;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    //readonly Configuration Configuration = Configuration.Debug;

    [Parameter] readonly MSBuildTargetPlatform TargetPlatform;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(NoFetch = true)] readonly GitVersion GitVersion;

    string _version = "1.0.0.0";
    readonly string ReleaseBranchPrefix = "release";
    [Parameter]
    readonly string MsBuildPath =
        @"c:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MsBuild.exe";

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

    Target Version => _ => _
        .Executes(() =>
        {
            _version = GitRepository.Branch.StartsWith(ReleaseBranchPrefix)
                ? GitRepository.Branch.Split("/")[1]
                : GitVersion.AssemblySemVer;

            Log.Information("Version: {_version}", _version);
            var assemblyInfo = SourceDirectory / "AssemblyInfo.cs";
            if (File.Exists(assemblyInfo))
            {
                Serilog.Log.Information("Update version for '{assemblyInfo}'", assemblyInfo);
                var fileContent = File.ReadAllText(assemblyInfo);
                fileContent = fileContent.Replace("1.0.0.0", _version);
                File.WriteAllText(assemblyInfo, fileContent);
            }
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            NuGetTasks.NuGetRestore(c => c.SetTargetPath(Solution));
            MSBuild(s => s
                .SetProcessToolPath(MsBuildPath)
                .SetTargetPath(Solution)
                .SetConfiguration(Configuration)
                .SetTargets("Restore"));
        });

    Target Compile => _ => _
        .DependsOn(Clean, Restore, Version)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetProcessToolPath(MsBuildPath)
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(_version)
                .SetFileVersion(_version)
                .SetInformationalVersion($"{_version}-{GitRepository.Commit}")
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetTargetPlatform(TargetPlatform)
                .SetNodeReuse(IsLocalBuild));
        });

    Target Artifacts => _ => _
    .Executes(() =>
    {
        var pluginOutput = SourceDirectory / (TargetPlatform == MSBuildTargetPlatform.x86
            ? "Aimp_DiskCover"
            : "Aimp_DiskCover-x64") / "bin" / "Aimp_DiskCover";

        EnsureExistingDirectory(pluginOutput);

        Log.Information("Create artifacts {pluginOutput}", pluginOutput);

        var artifactFile = OutputDirectory / $"DiskCover-{TargetPlatform}.zip";

        if (File.Exists(artifactFile))
        {
            File.Delete(artifactFile);
        }

        ZipFile.CreateFromDirectory(pluginOutput, artifactFile);
    });
}
