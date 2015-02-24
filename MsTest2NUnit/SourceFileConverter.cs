using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MsTest2NUnit
{
    public class SourceFileConverter : FileConverter
    {
        private const string MS_TEST_REF_1 = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";
        private const string MS_TEST_REF_2 = "Microsoft.VisualStudio.TestTools.UnitTesting";
        private const string NUNIT_TEST_REF = "NUnit.Framework";

        private const string REGEX_EXPECTED_EXCEPTION_SEARCH_PATTERN = "(.*)\\[(.*)ExpectedException\\(typeof\\((.+)\\),(\\s*)\\\"(.+)\\\"\\)";
        private const string REGEX_EXPECTED_EXCEPTION_REPLACEMENT_PATTERN = "$1[$2ExpectedException(typeof($3),$4ExpectedMessage = \"$5\")";

        private const string REGEX_ASSERT_IS_TYPE_SEARCH_PATTERN = "(.*)Assert.IsInstanceOfType\\((.*),(\\s*)typeof\\((.+)\\)(.+);";
        private const string REGEX_ASSERT_IS_TYPE_REPLACEMENT_PATTERN = "$1Assert.IsInstanceOfType(typeof($4), $2$5;";

        private bool? _isMsTestSourceFile = null;
        public bool IsMsTestSourceFile
        {
            get
            {
                if (!_isMsTestSourceFile.HasValue)
                {
                    _isMsTestSourceFile = GetIsMsTestSourceFile();
                }
                return _isMsTestSourceFile.Value;
            }
        }
        
        public SourceFileConverter(string filePath) : base(filePath)
        {
            if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Wrong file extension");
            }
        }

        public override void ConvertFileLines()
        {
            if (IsMsTestSourceFile)
            {
                // Iterate over every line
                for (int i = 0; i < FileLines.Length; i++)
                {
                    ConvertSourceLine(ref FileLines[i]);
                }
            }
        }

        #region Helpers

        private void ConvertSourceLine(ref string line)
        {
            string trimmedLine = line.Trim();
            ReplaceMsTestUsingStatemnent(trimmedLine, ref line);
            ReplaceMsTestAttributes(trimmedLine, ref line);
            ReplaceAsserts(trimmedLine, ref line);
        }

        private void ReplaceMsTestUsingStatemnent(string trimmedLine, ref string line)
        {
            if (IsMsTestUsingStatement(trimmedLine))
            {
                StringReplace(ref line, MS_TEST_REF_1, NUNIT_TEST_REF);
                StringReplace(ref line, MS_TEST_REF_2, NUNIT_TEST_REF);
            }
        }

        private void ReplaceMsTestAttributes(string trimmedLine, ref string line)
        {
            if (IsPossiblyAnAttributeLine(trimmedLine))
            {
                StringReplace(ref line, "TestMethod", "Test");
                StringReplace(ref line, "TestClass", "TestFixture");
                StringReplace(ref line, "ClassInitialize", "TestFixtureSetUp");
                StringReplace(ref line, "ClassCleanup", "TestFixtureTearDown");
                StringReplace(ref line, "TestInitialize", "SetUp");
                StringReplace(ref line, "TestCleanup", "TearDown");
                StringReplace(ref line, "TestCategory", "Category");
                StringReplace(ref line, "AssemblyInitialize", String.Empty);
                StringReplace(ref line, "AssemblyCleanUp", String.Empty);
                StringReplaceRegex(ref line, REGEX_EXPECTED_EXCEPTION_SEARCH_PATTERN, REGEX_EXPECTED_EXCEPTION_REPLACEMENT_PATTERN);
            }
        }

        private void ReplaceAsserts(string trimmedLine, ref string line)
        {
            if (IsMsTestAssert(trimmedLine))
            {
                StringReplace(ref line, "Assert.Inconclusive", "Assert.Ignore");
                StringReplaceRegex(ref line, REGEX_ASSERT_IS_TYPE_SEARCH_PATTERN, REGEX_ASSERT_IS_TYPE_REPLACEMENT_PATTERN);
            }
        }

        #region Checks

        private bool GetIsMsTestSourceFile()
        {
            foreach (string fileLine in FileLines)
            {
                if (!String.IsNullOrWhiteSpace(fileLine))
                {
                    string trimmedLine = fileLine.TrimStart();
                    if (!trimmedLine.StartsWith("//") && !trimmedLine.StartsWith("/*"))
                    {
                        if (IsUsingStatement(trimmedLine))
                        {
                            if (IsMsTestUsingStatement(trimmedLine))
                            {
                                // This is an MSTest using statement. We know this is a test file (we'll convert this line later)
                                return true;
                            }
                        }
                        else
                        {
                            // This is a line that is not a using statement, not a comment, and not blank-- we've probably reached the code.
                            return false;
                        }
                    }
                }
            }

            // Nothing to read, nothing to convert
            return false;
        }

        private bool IsMsTestUsingStatement(string line)
        {
            return IsUsingStatement(line) && (line.Contains(MS_TEST_REF_1, StringComparison.OrdinalIgnoreCase) || line.Contains(MS_TEST_REF_2, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsUsingStatement(string line)
        {
            return line.StartsWith("using");
        }

        private bool IsPossiblyAnAttributeLine(string line)
        {
            return line.StartsWith("[") && line.EndsWith("]");
        }

        private bool IsMsTestAssert(string line)
        {
            return line.Contains("Assert.") && line.Contains("(");
        }

        #endregion

	    #endregion
    }
}
