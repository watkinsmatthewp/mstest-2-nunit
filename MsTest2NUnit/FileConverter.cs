using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MsTest2NUnit
{
    public abstract class FileConverter
    {
        private Dictionary<string, Regex> _regexDictionary = new Dictionary<string, Regex>();
        
        protected string SourceFilePath { get; private set; }
        public int Replacements { get; protected set; }

        public bool FileContentWasChanged
        {
            get
            {
                return Replacements > 0;
            }
        }

        private string[] _fileLines = null;
        protected string[] FileLines
        {
            get
            {
                if (_fileLines == null)
                {
                    _fileLines = File.ReadAllLines(SourceFilePath);
                }
                return _fileLines;
            }
        }


        public FileConverter(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Could not find the file at: " + filePath);
            }
            SourceFilePath = filePath;
        }

        public void Run()
        {
            ConvertFileLines();
            if (FileContentWasChanged)
            {
                Console.WriteLine("Converting " + SourceFilePath);
                File.WriteAllLines(SourceFilePath, FileLines);
            }
        }

        public abstract void ConvertFileLines();

        protected void StringReplace(ref string input, string searchString, string replacementString)
        {
            if (input.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            {
                input = input.Replace(searchString, replacementString);
                Replacements++;
            }
        }

        protected void StringReplaceRegex(ref string input, string regexSearchPattern, string regexReplacementPattern)
        {
            if (!_regexDictionary.ContainsKey(regexSearchPattern))
            {
                _regexDictionary.Add(regexSearchPattern, new Regex(regexSearchPattern));
            }
            Regex regex = _regexDictionary[regexSearchPattern];

            if (regex.IsMatch(input))
            {
                input = regex.Replace(input, regexReplacementPattern);
                Replacements++;
            }
        }
    }
}
