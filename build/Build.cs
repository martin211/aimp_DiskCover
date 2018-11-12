using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Solution("DiskCover.sln")] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    readonly IEnumerable<string> _excludedArtifactFiles = new[]
    {
        "AutoMapper.dll",
        "Colorful.Console.dll",
        "Glob.dll",
        "Newtonsoft.Json.dll",
        "JetBrains.Annotations.dll"
    };

    Target Clean => _ => _
        .Executes(() =>
        {
            DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
            DeleteDirectories(GlobDirectories(TestsDirectory, "**/bin", "**/obj"));
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Restore"));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
                .SetFileVersion(GitVersion.GetNormalizedFileVersion())
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetNodeReuse(IsLocalBuild));
        });

    Target Copy => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var inputFolder = GlobDirectories(SourceDirectory, $"**/bin/{Configuration}").FirstOrDefault();

            if (string.IsNullOrWhiteSpace(inputFolder) || !Directory.Exists(inputFolder))
            {
                Logger.Error("Output folder not found or empty. Run Compile task...");
                return;
            }

            var files = Directory.GetFiles($"{inputFolder}", "*.dll")
                .Where(c => !_excludedArtifactFiles.Contains(Path.GetFileName(c.ToString())));

            foreach (var file in files)
            {
                var newFile = OutputDirectory / Path.GetFileName(file);
                if (file.EndsWith("aimp_dotnet.dll"))
                {
                    newFile = OutputDirectory / "dotnet_diskcover.dll";
                }

                Logger.Info($"Copy '{file}' to '{newFile}'");

                File.Copy(file, newFile);
            }

            FileSystemTasks.CopyDirectoryRecursively(SourceDirectory / "langs", OutputDirectory / "langs");
        });

    Target Version => _ => _
        .Executes(() =>
        {
            ProcessTasks.StartProcess(GitVersionTasks.GitVersionPath, $"/updateassemblyinfo {SourceDirectory / "Properties" / "AssemblyInfo.cs"}");
        });
}
