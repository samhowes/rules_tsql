using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RulesMSBuild.Tools.Bazel;
using static release.Util;

namespace release
{
    enum Action
    {
        Release,
        Clean,
        Test,
    }
    class Program
    {
        private static Action _action;
        private static string _work;
        private static string _root;
        private static string _version;

        static async Task<int> Main(string[] args)
        {
            Setup(args);

            if (_action == Action.Clean)
            {
                Run($"gh release delete \"{_version}\" -y");
                Run($"git push --delete origin \"{_version}\"");
                return 0;
            }

            VerifyNewRelease();

            var (tarAlias, tarSha) = BuildTar(_work, _version);

            var url =
                $"https://github.com/samhowes/rules_tsql/releases/download/{_version}/rules_tsql-{_version}.tar.gz";

            var usage = $@"http_archive(
    name = ""rules_tsql"",
    sha256 = ""{tarSha}"",
    urls = [""{url}""],
)
";
            
            await MakeNotes(usage);

            if (_action == Action.Release)
            {
                UpdateVersion();
                Info("Creating release...");
                Run($"gh release create {_version} ",
                    "--prerelease",
                    "--draft",
                    $"--title v{_version}",
                    "-F ReleaseNotes.md",
                    tarAlias);
            }

            return 0;
        }
        
        private static async Task MakeNotes(string usage)
        {
            const string releaseNotes = "ReleaseNotes.md";
            var originalNotes = await File.ReadAllTextAsync(releaseNotes);
            const string marker = "<!--marker-->";
            var markerIndex = originalNotes.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0) Die("Failed to find marker in release notes");

            var notes = new StringBuilder(originalNotes[..(markerIndex + marker.Length)]);

            notes.AppendLine();
            notes.AppendLine("```python");
            notes.Append(usage);
            notes.AppendLine("```");

            // var lastRelease = RunJson<GitHubRelease>("gh release view --json");
            // var prs = RunJson<List<GitHubPr>>(
            //     $"gh pr list --search \"is:closed closed:>={lastRelease.CreatedAt}\" --json");
            //
            // notes.AppendLine("Changelog:");
            // for (var i = 0; i < prs.Count; i++)
            // {
            //     var pr = prs[i];
            //     notes.AppendLine($"{i + 1}. [PR #{pr.Number}: {pr.Title}]({pr.Url})");
            //     var toMatch = pr.Title + pr.Body;
            //     var closed = string.Join(", ", Regex.Matches(toMatch, @"#\d+").Select(m => m.Value));
            //     notes.Append("  Issues: ");
            //     notes.AppendLine(closed);
            // }

            await File.WriteAllTextAsync(releaseNotes, notes.ToString());
        }

        private static void UpdateVersion()
        {
            var versionParts = _version.Split(".").Select(int.Parse).ToArray();
            versionParts[^1]++;
            var newString = string.Join(".", versionParts);
            File.WriteAllText(Path.Combine(_root, "version.bzl"), $"VERSION = \"{newString}\"");
        }

        private static void Setup(string[] args)
        {
            _action = Action.Release;
            if (args.Length > 0)
            {
                if (!Enum.TryParse<Action>(args[0], true, out _action))
                    Die($"Failed to parse action from {args[0]}");
            }

            _work = Path.Combine(Directory.GetCurrentDirectory(), "_work");
            if (Directory.Exists(_work))
                Directory.Delete(_work, true);
            Directory.CreateDirectory(_work);

            Info($"Work directory: {_work}");
            _root = BazelEnvironment.GetWorkspaceRoot();
            Directory.SetCurrentDirectory(_root);
            
            var versionContents = File.ReadAllText(Path.Combine(_root, "version.bzl"));
            var versionMatch = Regex.Match(versionContents, @"VERSION.*?=.*?""([^""]+)""");
            if (!versionMatch.Success) Die("Failed to parse version from version.bzl");
            _version = versionMatch.Groups[1].Value;
            Info($"Using version: {_version}");
        }

        private static (string tarAlias, string tarSha) BuildTar(string work, string version)
        {
            foreach (var file in Directory.GetFiles("bazel-bin", "rules_tsql.*"))
            {
                Console.WriteLine($"Removing old artifact: {file}");
                File.Delete(file);
            }
        
            var outputs = Bazel("build //:tar");
            var tarSource = outputs[0];
            var tarSha = File.ReadAllText(outputs[1]);
            var tarAlias = Path.Combine(work, $"rules_tsql-{version}.tar.gz");
            Run($"ln -s {tarSource} {tarAlias}");
            return (tarAlias, tarSha);
        }

        private static void VerifyNewRelease()
        {
            Info("Checking for existing release...");
            var existingRelease = TryRun($"gh release view {_version}");
            if (existingRelease != null)
            {
                Die($"Failed to release: {_version} already exists");
            }
        }
    }

    public class GitHubPr
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public string Number { get; set; }
    }

    public class GitHubRelease
    {
        public string CreatedAt { get; set; }
    }

    public class DevOpsResource
    {
        [JsonProperty("_links")] public Dictionary<string, Link> Links { get; set; }
    }

    public class Link
    {
        public string Href { get; set; }
    }
}