using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RulesMSBuild.Tools.Bazel;

namespace tar
{
    public class FileEditor
    {
        private readonly Regex _regex;

        public FileEditor(string markerName)
        {
            _regex = new Regex(
                $@"(?<prefix>\n([^\n]+?)(rules_tsql:{markerName} start[^\n]*\n))(?<public>.*?)(?<suffix>((\n([^\n]+?)(rules_tsql:{markerName} end))|$))",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        public bool ReplaceContent(string filepath, string content)
        {
            var contents = File.ReadAllText(filepath);
            var span = contents.AsSpan();
            var builder = new StringBuilder();
            foreach (var match in _regex.Matches(contents).Cast<Match>())
            {
                builder.Append(span[0..match.Index]);
                span = span[(match.Index + match.Length)..];
                builder.Append(match.Groups["prefix"].Value);
                builder.Append(content);
                builder.Append(match.Groups["suffix"].Value);
            }

            if (builder.Length == 0) return false;
            builder.Append(span);

            File.WriteAllText(filepath, builder.ToString());
            return true;
        }

        public bool HideOtherContent(string filepath, out string destPath)
        {
            destPath = Path.Combine(BazelEnvironment.GetTmpDir(), Guid.NewGuid().ToString());
            var contents = File.ReadAllText(filepath);
            var builder = new StringBuilder();
            foreach (var match in _regex.Matches(contents).Cast<Match>())
            {
                builder.Append(match.Groups["public"].Value);
            }

            if (builder.Length == 0) return false;

            File.WriteAllText(destPath, builder.ToString());
            return true;
        }
    }
}