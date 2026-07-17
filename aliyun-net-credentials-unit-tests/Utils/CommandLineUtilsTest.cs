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
        public void TestEscapedSpace()
        {
            Assert.Equal(
                new[] {"tool", "arg with space"},
                CommandLineUtils.Split(@"tool arg\ with\ space"));
        }

        [Fact]
        public void TestEscapedQuoteInsideDoubleQuotes()
        {
            Assert.Equal(
                new[] {"tool", "say \"hi\""},
                CommandLineUtils.Split("tool \"say \\\"hi\\\"\""));
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
        public void TestTrailingBackslash()
        {
            var ex = Assert.Throws<CredentialException>(() => CommandLineUtils.Split(@"tool\"));
            Assert.Contains("trailing backslash", ex.Message);
        }
    }
}
