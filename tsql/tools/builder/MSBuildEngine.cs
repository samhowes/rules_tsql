using System;
using System.Collections;
using Microsoft.Build.Framework;

namespace builder
{
    public class MSBuildEngine : IBuildEngine
    {
        private readonly string _basePath;
        private readonly bool _debugEnabled;

        public MSBuildEngine(string basePath)
        {
            _basePath = basePath;
            var debug = Environment.GetEnvironmentVariable("RULES_TSQL_DEBUG");

            _debugEnabled = !string.IsNullOrEmpty(debug) && debug != "0";
        }
        
        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            if (e.File != null)
            {
                LogFileError(e);
            }
            else
            {
                Console.WriteLine(e.Message);
            }
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            if (e.File != null)
            {
                LogFileError(e);
            }
            else
            {
                Console.WriteLine(e.Message);
            }
        }

        private void LogFileError(LazyFormattedBuildEventArgs e)
        {
            string level;
            string src;
            string code;
            int column;
            int line;
            switch (e)
            {
                case BuildWarningEventArgs warn:
                    src = warn.File;
                    level = "Warning";
                    line = warn.LineNumber;
                    column = warn.ColumnNumber;
                    code = warn.Code;
                    break;
                case BuildErrorEventArgs error:
                    src = error.File;
                    level = "Error";
                    line = error.LineNumber;
                    column = error.ColumnNumber;
                    code = error.Code;
                    break;
                case BuildMessageEventArgs unknown:
                    src = unknown.File;
                    level = "Warning";
                    line = unknown.LineNumber;
                    column = unknown.ColumnNumber;
                    code = unknown.Code;
                    break;
                default:
                    throw new NotImplementedException("Please file an issue.");
            }

            if (src.StartsWith(_basePath))
                src = src[(_basePath.Length+1)..];
            
            Console.WriteLine($"{src}:{line}:{column}: {level} {code}: {e.Message}");
        }
        
        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            if (e.File != null)
            {
                LogFileError(e);
            }

            if (_debugEnabled)
            {
                Console.WriteLine(e.Message);   
            }
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
            IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public bool ContinueOnError { get; }
        public int LineNumberOfTaskNode { get; }
        public int ColumnNumberOfTaskNode { get; }
        public string ProjectFileOfTaskNode { get; }
    }
}