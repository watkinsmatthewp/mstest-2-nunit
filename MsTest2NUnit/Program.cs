using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MsTest2NUnit
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- MSTEST 2 NUNIT ---");
            try
            {
                Run(GetDirectory(args));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("Goodbye");
            Console.ReadKey();
        }

        private static string GetDirectory(string[] args)
        {
            string directory = null;
            if (args != null && args.Length > 0 && !String.IsNullOrWhiteSpace(args[0]))
            {
                return ValidateDirectory(args[0].Trim());
            }
            else
            {
                do
                {
                    Console.WriteLine("Please enter a file directory to scan and convert:");
                    directory = ValidateDirectory(Console.ReadLine());
                }
                while (directory == null);
            }
            return directory;
        }

        private static string ValidateDirectory(string directory)
        {
            if (String.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                Console.WriteLine("The specified directory is blank, does not exist, or is inaccessible");
                return null;
            }
            else
            {
                return directory;
            }
        }

        private static void Run(string directory)
        {
            Console.WriteLine();
            
            int numberOfChangedFiles = 0;
            int numberOfReplacementsMade = 0;

            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
            Parallel.ForEach(Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories), options, filePath => 
            {
                // TODO: Handle csproj files, too
                SourceFileConverter fileConverter = new SourceFileConverter(filePath);
                fileConverter.Run();
                if (fileConverter.FileContentWasChanged)
                {
                    numberOfChangedFiles++;
                    numberOfReplacementsMade += fileConverter.Replacements;
                }
            });

            Console.WriteLine();
            Console.WriteLine("--- STATS ---");
            Console.WriteLine("Made " + numberOfReplacementsMade + " replacements across " + numberOfChangedFiles + " files");
        }
    }
}
