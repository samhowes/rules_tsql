using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using RulesMSBuild.Tools.Bazel;

namespace build_test
{
    class Program
    {
        static int Main(string[] args)
        {
            var runfiles = Runfiles.Create<Program>();
            var path = args[0];
            string dacpac;
            if (!Path.IsPathRooted(path))
            {
                dacpac = runfiles.Rlocation("rules_tsql/" + path);
            }
            else
            {
                dacpac = path;
            }

            // Console.WriteLine(dacpac);
            Console.WriteLine(string.Join(",", args));
            var expectations = JObject.Parse(File.ReadAllText(runfiles.Rlocation("rules_tsql/" + args[1])));

            var zip = ZipFile.OpenRead(dacpac);
            var files = new Dictionary<string, ZipArchiveEntry>();
            foreach (var entry in zip.Entries)
            {
                files[entry.FullName] = entry;
            }

            try
            {
                Assert(files, expectations);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        private static void Assert(Dictionary<string, ZipArchiveEntry> files, JObject expectations)
        {
            foreach (var file in new[] {"model.xml", "DacMetadata.xml", "Origin.xml", "[Content_Types].xml"})
            {
                files.Should().ContainKey(file);
            }

            var modelEntry = files["model.xml"];

            XDocument doc;
            using (var reader = new XmlTextReader(modelEntry.Open()))
            {
                reader.Namespaces = false;
                doc = XDocument.Load(reader);
            }

            var root = doc.Root;
            root.Should().NotBeNull("Missing root document element");

            var tester = new Tester(root, expectations);
            tester.Assert();
        }
    }

    public class Tester
    {
        private readonly XElement _model;
        private readonly JObject _expectations;
        private readonly Stack<string> _path;

        public Tester(XElement model, JObject expectations)
        {
            _model = model;
            _expectations = expectations;
            _path = new Stack<string>();
        }

        public void Assert()
        {
            AssertImpl(_model, _expectations);
        }

        private void AssertImpl(XElement element, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = (JObject)token;
                    foreach (var prop in obj.Properties())
                    {
                        _path.Push(prop.Name);
                        if (prop.Value.Type == JTokenType.String)
                        {
                            AssertXpath(element, prop.Name, prop.Value.Value<string>());
                        }
                        else
                        {
                            var next = element.XPathSelectElements(prop.Name).ToList();

                            next.Count().Should().Be(1, $"Expected exactly one result for xpath: {prop.Name} at {GetPath()}");
                            AssertImpl(next[0], prop.Value);    
                        }

                        _path.Pop();
                    }
                    break;
                default:
                    throw new NotImplementedException(token.Type.ToString());
            }
        }

        private string GetPath()
        {
            return string.Join('/', _path.Reverse());
        }

        private void AssertXpath(XElement element, string xpath, string expectedValue)
        {
            var value = element.XPathEvaluate(xpath);
            switch (value)
            {
                case IEnumerable en:
                    var values = en.Cast<XObject>().ToList();
                    values.Count.Should().Be(1, $"Expected only one result for relpath: {xpath} fullpath: {GetPath()}");
                    var found = values.Single();
                    switch (found)
                    {
                        case XAttribute att:
                            att.Value.Should().Be(expectedValue, $"relpath: {xpath}; fullpath: {GetPath()}");
                            break;
                        default:
                            throw new NotImplementedException(found.ToString());
                            break;
                    }
                    break;
                default:
                    throw new NotImplementedException(value.ToString());
                    break;
            }
        }
    }
}
