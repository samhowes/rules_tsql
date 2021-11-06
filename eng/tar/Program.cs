using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using RulesMSBuild.Tools.Bazel;

namespace tar
{
    class Program
    {
        static Regex ReleaseRegex = new Regex(@"(?<name>.*)(\.release)(?<ext>\..*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static async Task<int> Main(string[] argsArray)
        {
            var args = argsArray.Select(a => a.TrimStart('-').Split('='))
                .ToDictionary(p => p[0], p => p[1]);

            var root = BazelEnvironment.TryGetWorkspaceRoot();
            if (root != null)
                Directory.SetCurrentDirectory(root);
            else
                root = Directory.GetCurrentDirectory();

            var process = Process.Start(new ProcessStartInfo("git", "ls-files") {RedirectStandardOutput = true});

            if (args.TryGetValue("tar", out var outputName))
            {
                outputName = outputName.Split(" ")[0];
            }
            else
                outputName = "test.tar.gz";

            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (;;)
            {
                var actual = await process!.StandardOutput.ReadLineAsync();
                if (actual == null) break;
                if (Path.GetFileName(actual) == outputName) continue;

                var tarValue = actual;
                actual = Path.Combine(root, actual);
                var match = ReleaseRegex.Match(actual);
                if (match.Success)
                {
                    tarValue = match.Groups["name"].Value + match.Groups["ext"].Value;
                    tarValue = Path.GetRelativePath(root, tarValue);
                }

                files[tarValue] = actual;
            }

            // wait for exit AFTER reading all the standard output:
            // https://stackoverflow.com/questions/439617/hanging-process-when-run-with-net-process-start-whats-wrong
            await process!.WaitForExitAsync();
            if (process.ExitCode != 0) return process.ExitCode;

            var runfiles = Runfiles.Create();
            var publishDir = runfiles.Rlocation("rules_tsql/tsql/tools/builder/publish/net5.0");
            var published = RecordPublishFiles(files, publishDir);
            var tmp = UpdateBuildRelease(files, published);

            await using (var output = File.Create(outputName))
            await using (var gzoStream = new GZipOutputStream(output))
            using (var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
            {
                foreach (var tarEntry in files.Keys.OrderBy(k => k))
                {
                    var entry = TarEntry.CreateEntryFromFile(files[tarEntry]);
                    // https://github.com/dotnet/runtime/issues/24655#issuecomment-566791742
                    await using (var stream = File.OpenRead(files[tarEntry]))
                    {
                        entry.TarHeader.Size = stream.Length;
                    }

                    entry.Name = tarEntry;
                    Console.WriteLine(entry.Name);

                    tarArchive.WriteEntry(entry, false);
                }

                tarArchive.Close();
            }

            File.Delete(tmp);

            string hashValue;
            await using (var outputRead = File.OpenRead(outputName))
            using (var sha = SHA256.Create())
            {
                outputRead.Position = 0;
                var hash = await sha.ComputeHashAsync(outputRead);
                hashValue = Convert.ToHexString(hash).ToLower();
            }

            Console.WriteLine($"SHA256 = {hashValue}");
            await File.WriteAllTextAsync(outputName + ".sha256", hashValue);

            return 0;
        }

        private static string UpdateBuildRelease(Dictionary<string, string> files, List<string> published)
        {
            var tmp = Path.Combine(BazelEnvironment.GetTmpDir(), Guid.NewGuid().ToString());
            var buildReleaseContents = File.ReadAllText("tsql/tools/builder/BUILD.release.bazel");
            var str = string.Join("\",\n        \"", published.OrderBy(p => p));
            var replaced = buildReleaseContents.Replace("@@prebuilt_files@@", str);
            File.WriteAllText(tmp, replaced);
            files["tsql/tools/builder/BUILD.bazel"] = tmp;
            return tmp;
        }

        private static List<string> RecordPublishFiles(Dictionary<string, string> files, string publishDir)
        {
            var list = new List<string>();

            void WalkDirectory(string path)
            {
                foreach (var subDir in Directory.EnumerateDirectories(path))
                {
                    WalkDirectory(subDir);
                }

                foreach (var file in Directory.EnumerateFiles(path))
                {
                    var rel = string.Join("/", "prebuilt", file[(publishDir.Length + 1)..]);
                    list.Add(rel);
                    files[string.Join('/', "tsql/tools/builder", rel)] = file;
                }
            }

            WalkDirectory(publishDir);
            return list;
        }
    }
}