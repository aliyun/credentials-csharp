using System;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Utils;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Utils
{
    public class CommandLineUtilsTest
    {
        [Fact]
        public void TestSimple()
        {
            Assert.Equal(new[] {"cmd", "arg1", "arg2"}, CommandLineUtils.Split("cmd arg1 arg2"));
        }

        [Fact]
        public void TestExtraWhitespace()
        {
            Assert.Equal(new[] {"cmd", "arg1", "arg2"}, CommandLineUtils.Split("  cmd   arg1\targ2  "));
        }

        [Fact]
        public void TestWindowsQuotedPath()
        {
            Assert.Equal(
                new[] {@"C:\Program Files\tool\cred.exe", "get", "--profile", "default"},
                CommandLineUtils.Split(@"""C:\Program Files\tool\cred.exe"" get --profile default"));
        }

        [Fact]
        public void TestUnixSingleQuotedPath()
        {
            Assert.Equal(
                new[] {"/usr/local/my tools/cred", "arg"},
                CommandLineUtils.Split("'/usr/local/my tools/cred' arg"));
        }

        [Fact]
        public void TestQuotedArgument()
        {
            Assert.Equal(
                new[] {"tool", "--name", "First Last"},
                CommandLineUtils.Split("tool --name \"First Last\""));
        }

        [Fact]
        public void TestEscapedSpaceUnix()
        {
            Assert.Equal(
                new[] {"tool", "arg with space"},
                CommandLineUtils.Split(@"tool arg\ with\ space", false));
        }

        [Fact]
        public void TestEscapedQuoteInsideDoubleQuotesUnix()
        {
            Assert.Equal(
                new[] {"tool", "say \"hi\""},
                CommandLineUtils.Split("tool \"say \\\"hi\\\"\"", false));
        }

        [Fact]
        public void TestSingleQuotedKeepsBackslashesUnix()
        {
            // printf-style octal escapes must survive tokenizing when single quoted
            Assert.Equal(
                new[] {"/usr/bin/printf", @"\173\042mode\042\175"},
                CommandLineUtils.Split(@"/usr/bin/printf '\173\042mode\042\175'", false));
        }

        [Fact]
        public void TestWindowsUnquotedPathKeepsBackslashes()
        {
            Assert.Equal(
                new[] {@"C:\tools\cred.exe", "get"},
                CommandLineUtils.Split(@"C:\tools\cred.exe get", true));
        }

        [Fact]
        public void TestWindowsQuotedProgramFilesPath()
        {
            Assert.Equal(
                new[] {@"C:\Program Files\tool\cred.exe", "get"},
                CommandLineUtils.Split(@"""C:\Program Files\tool\cred.exe"" get", true));
        }

        [Fact]
        public void TestWindowsEscapedQuoteInsideDoubleQuotes()
        {
            Assert.Equal(
                new[] {"tool", "say \"hi\""},
                CommandLineUtils.Split("tool \"say \\\"hi\\\"\"", true));
        }

        [Fact]
        public void TestWindowsBackslashInsideDoubleQuotesLiteral()
        {
            Assert.Equal(
                new[] {@"C:\Program Files\tool.exe"},
                CommandLineUtils.Split(@"""C:\Program Files\tool.exe""", true));
        }

        [Fact]
        public void TestEmptyDoubleQuotedArgument()
        {
            Assert.Equal(new[] {"tool", "", "arg"}, CommandLineUtils.Split("tool \"\" arg"));
        }

        [Fact]
        public void TestEmptySingleQuotedArgument()
        {
            Assert.Equal(new[] {"tool", "", "arg"}, CommandLineUtils.Split("tool '' arg"));
        }

        [Fact]
        public void TestAdjacentQuotedSegmentsFormOneArgument()
        {
            Assert.Equal(new[] {"tool", "a bc d"}, CommandLineUtils.Split("tool \"a b\"'c d'"));
        }

        [Fact]
        public void TestBackslashNewlineOutsideQuotesIsLineContinuationUnix()
        {
            Assert.Equal(
                new[] {"tool", "arg1", "arg2"},
                CommandLineUtils.Split("tool arg1 \\\n arg2", false));
        }

        [Fact]
        public void TestBackslashNewlineInsideDoubleQuotesIsLineContinuationUnix()
        {
            Assert.Equal(
                new[] {"tool", "ab"},
                CommandLineUtils.Split("tool \"a\\\nb\"", false));
        }

        [Fact]
        public void TestEmpty()
        {
            var ex = Assert.Throws<CredentialException>(() => CommandLineUtils.Split("   "));
            Assert.Contains("process_command is empty", ex.Message);
        }

        [Fact]
        public void TestEmptyQuotedArgv0()
        {
            var ex = Assert.Throws<CredentialException>(() => CommandLineUtils.Split("\"\""));
            Assert.Contains("process_command is empty", ex.Message);
        }

        [Fact]
        public void TestUnclosedQuote()
        {
            var ex = Assert.Throws<CredentialException>(() =>
                CommandLineUtils.Split(@"""C:\Program Files\tool.exe"));
            Assert.Contains("unclosed quote", ex.Message);
        }

        [Fact]
        public void TestTrailingBackslashUnix()
        {
            var ex = Assert.Throws<CredentialException>(() => CommandLineUtils.Split(@"tool\", false));
            Assert.Contains("trailing backslash", ex.Message);
        }
    }
}
