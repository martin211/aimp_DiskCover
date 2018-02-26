using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Nuke.Common.Tools;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.NuGet;
using Nuke.Core;
using Nuke.Core.Tooling;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Core.IO.FileSystemTasks;
using static Nuke.Core.IO.PathConstruction;

class Build : NukeBuild
{
    readonly IEnumerable<string> _excludedArtifactFiles = new[]
    {
        "AutoMapper.dll",
        "Colorful.Console.dll",
        "Glob.dll",
        "Newtonsoft.Json.dll",
        "JetBrains.Annotations.dll"
    };

    // Console application entry. Also defines the default target.
    public static int Main() => Execute<Build>(x => x.Compile);

    // Auto-injection fields:

    //[GitVersion] readonly GitVersion GitVersion;
    // Semantic versioning. Must have 'GitVersion.CommandLine' referenced.

    // [GitRepository] readonly GitRepository GitRepository;
    // Parses origin, branch name and head from git config.

    // [Parameter] readonly string MyGetApiKey;
    // Returns command-line arguments and environment variables.

    Target Clean => _ => _
            .OnlyWhen(() => false) // Disabled for safety.
            .Executes(() =>
            {
                DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
                EnsureCleanDirectory(OutputDirectory);
            });

    Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() =>
            {
                MSBuild(s => DefaultMSBuildRestore);
                NuGetTasks.NuGetRestore();
            });

    Target Compile => _ => _
            .DependsOn(Restore)
            .DependsOn(Version)
            .Executes(() =>
            {
                MSBuild(s => DefaultMSBuildCompile);
            });

    Target CopyArtifacts => _ => _
        //.DependsOn(Compile)
        .Executes(() =>
        {
            DeleteDirectories(new[] { ArtifactsDirectory.ToString() });
            var outputFolder = ArtifactsDirectory / "dotnet_diskcover";
            Directory.CreateDirectory(outputFolder);
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
                var newFile = outputFolder / Path.GetFileName(file);
                if (file.EndsWith("aimp_dotnet.dll"))
                {
                    newFile = outputFolder / "dotnet_diskcover.dll";
                }

                Logger.Info($"Copy '{file}' to '{newFile}'");

                File.Copy(file, newFile);
            }

            ProcessTasks.StartProcess(@"c:\Program Files\7-Zip\7z.exe", $"a {ArtifactsDirectory / "dotnet_diskcover.zip"} {ArtifactsDirectory / "*"}");
        });

    Target Version => _ => _
        .Executes(() =>
        {
            ProcessTasks.StartProcess(GitVersionTasks.DefaultGitVersion.ToolPath, $"/updateassemblyinfo {SourceDirectory / "Properties" / "AssemblyInfo.cs"}");
        });
}
