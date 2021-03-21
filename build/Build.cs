using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.IO.PathConstruction;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
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

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(NoFetch = true)] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            NuGetTasks.NuGetRestore(c => c.SetTargetPath(Solution));
            MSBuild(_ => _
                .SetTargetPath(Solution)
                .SetTargets("Restore"));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var version = GitVersion.AssemblySemVer;
            //if (GitRepository.Branch != null && GitRepository.Branch.StartsWith("release"))
            //{
            //    version = GitRepository.Branch.Split('/')[1];
            //}

            var assemblyInfo = GlobFiles(SourceDirectory, "**/AssemblyInfo.cs");
            if (assemblyInfo.Count > 0)
            {
                foreach (var file in assemblyInfo)
                {
                    Logger.Info($"Update version at '{file}'");
                    var fileContent = File.ReadAllText(file);
                    fileContent = fileContent
                        .Replace("1.0.0.0", version)
                        .Replace("InfVer", GitVersion.InformationalVersion);
                    File.WriteAllText(file, fileContent);
                }
            }

            MSBuild(_ => _
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(version)
                .SetFileVersion(version)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetNodeReuse(IsLocalBuild));
        });

    Target Artifacts => _ => _
        .Executes(() =>
        {
            var outputDir = OutputDirectory / "DiskCover" / "Aimp_DiskCover";
            EnsureCleanDirectory(OutputDirectory);
            Directory.CreateDirectory(outputDir);
            Logger.Info("Copy SDK files to artifacts folder");

            var directories = GlobDirectories(SourceDirectory, $"**/bin/{Configuration}");

            if (directories.Count == 0)
            {
                Logger.Error($"bin/{Configuration} not found. Run Compile first.");
                return;
            }

            var files = GlobFiles(directories.First(), "*.dll");
            foreach (var file in files)
            {
                string outFile = string.Empty;
                var f = new FileInfo(file);

                if (f.Name.StartsWith("DiskCover.dll"))
                {
                    outFile = outputDir / $"{Path.GetFileNameWithoutExtension(f.Name)}_plugin.dll";
                }
                else
                {
                    outFile = outputDir / f.Name;
                }

                if (f.Name.StartsWith("aimp_dotnet"))
                {
                    outFile = outputDir / "Aimp_DiskCover.dll";
                }

                Logger.Info($"Copy '{f.Name}' to '{outFile}'");

                f.CopyTo(outFile, true);
            }

            var lang = GlobDirectories(SourceDirectory, "**/langs");
            if (lang.Count > 0)
            {
                if (!Directory.Exists(outputDir / "langs"))
                {
                    Directory.CreateDirectory(outputDir / "langs");
                }

                files = GlobFiles(lang.First(), "*.lng");

                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    var outFile = outputDir / "langs" / fi.Name;
                    Logger.Info($"Copy '{fi.Name}' to '{outFile}'");
                    File.Copy(file, outFile);
                }
            }

            Logger.Info("Compress artifacts");
            ZipFile.CreateFromDirectory(OutputDirectory / "DiskCover", OutputDirectory / "DiskCover.zip");
        });
}
