using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace PySharp
{
    public class Program
    {
        private const int TAB_SIZE = 4;

        private const string VS_DEV_BATCH_PATH = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\Common7\\Tools\\VsDevCmd.bat";

        private const string SOURCE_PATH = "E:\\PSCompiler.ps";
        private const string OUTPUT_PATH = "E:\\PSCompiler.exe";

        private static void Main(string[] args)
        {
            // Read the Py# source code line by line
            List<string> pysharpLines = new List<string>();

            using (StreamReader sr = new StreamReader(SOURCE_PATH))
            {
                while (!sr.EndOfStream)
                {
                    pysharpLines.Add(sr.ReadLine());
                }
            }

            // Write temporary C# code
            string tempCSharpPath = Path.GetTempFileName();

            using (StreamWriter sw = new StreamWriter(tempCSharpPath))
            {
                sw.Write(ConvertToCSharp(pysharpLines));
            }

            // Show the C# code
            Process.Start("notepad++", tempCSharpPath);

            // Run the C# compiler
            Process.Start("cmd", string.Format("/c (\"{0}\") && csc \"{1}\" /out:\"{2}\" || pause", VS_DEV_BATCH_PATH, tempCSharpPath, OUTPUT_PATH));
        }

        private static string ConvertToCSharp(List<string> pysharpLines)
        {
            List<Definition> definitions = new List<Definition>();
            List<string> csharpLines = new List<string>();

            int lastScope = 0;

            foreach (string line in pysharpLines)
            {
                // Replace tabs with spaces and remove trailing whitespace
                string lineCleaned = line.Replace("\t", " ".Repeat(TAB_SIZE)).TrimEnd(' ');

                // Empty line
                if (lineCleaned.Length == 0)
                {
                    continue;
                }

                // Line contains a definition
                if (Definition.TryParse(lineCleaned, out Definition newDef))
                {
                    definitions.Add(newDef);
                    continue;
                }

                // Insert curly brackets
                int scopeLevel = GetScopeLevel(lineCleaned);
                csharpLines.AddRange(GenerateCurlyBrackets(lastScope, scopeLevel));
                lastScope = scopeLevel;

                // Resolve definitions
                string lineWithResolvedDefinitions = lineCleaned;

                foreach (Definition def in definitions)
                {
                    lineWithResolvedDefinitions = lineWithResolvedDefinitions.Replace(def.Name, def.Value);
                }

                // Append the code line
                csharpLines.Add(lineWithResolvedDefinitions);
            }

            // Insert end-of-scope curly brackets (end of class, end of method, etc.)
            csharpLines.AddRange(GenerateCurlyBrackets(lastScope, 0));

            // Insert missing semicolons
            for (int i = 0; i < csharpLines.Count - 1; i++)
            {
                string thisLine = csharpLines[i];
                string nextLine = csharpLines[i + 1];

                if (!nextLine.EndsWith("{") && !thisLine.EndsWith("}") &&
                    !thisLine.EndsWith(";") &&
                    GetScopeLevel(thisLine) >= GetScopeLevel(nextLine))
                {
                    csharpLines[i] += ";";
                }
            }

            return string.Join(Environment.NewLine, csharpLines);
        }

        private static List<string> GenerateCurlyBrackets(int oldScope, int newScope)
        {
            List<string> lines = new List<string>();

            if (newScope > oldScope)
            {
                for (int i = oldScope; i < newScope; i++)
                {
                    lines.Add(" ".Repeat(i * TAB_SIZE) + "{");
                }
            }
            else if (newScope < oldScope)
            {
                for (int i = oldScope; i > newScope; i--)
                {
                    lines.Add(" ".Repeat((i - 1) * TAB_SIZE) + "}");
                }
            }

            return lines;
        }

        private static int GetScopeLevel(string line) => line.TakeWhile(x => x == ' ').Count() / TAB_SIZE;
    }
}