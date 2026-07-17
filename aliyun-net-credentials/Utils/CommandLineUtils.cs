using System.Collections.Generic;
using System.Text;

using Aliyun.Credentials.Exceptions;

namespace Aliyun.Credentials.Utils
{
    /// <summary>
    /// Split process_command into argv with quote support (POSIX shlex-like),
    /// so Windows paths like "C:\Program Files\tool.exe" work as one argument.
    /// </summary>
    public static class CommandLineUtils
    {
        public static string[] Split(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new CredentialException("process_command is empty");
            }

            string input = command.Trim();
            var args = new List<string>();
            var current = new StringBuilder();
            bool inSingle = false;
            bool inDouble = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (inSingle)
                {
                    if (c == '\'')
                    {
                        inSingle = false;
                    }
                    else
                    {
                        current.Append(c);
                    }
                    continue;
                }

                if (inDouble)
                {
                    if (c == '"')
                    {
                        inDouble = false;
                        continue;
                    }

                    if (c == '\\' && i + 1 < input.Length)
                    {
                        char next = input[i + 1];
                        if (next == '"' || next == '\\' || next == '$' || next == '`' || next == '\n')
                        {
                            current.Append(next);
                            i++;
                            continue;
                        }
                    }

                    current.Append(c);
                    continue;
                }

                if (c == '\\')
                {
                    if (i + 1 >= input.Length)
                    {
                        throw new CredentialException("invalid process_command: trailing backslash");
                    }

                    current.Append(input[++i]);
                    continue;
                }

                if (c == '\'')
                {
                    inSingle = true;
                    continue;
                }

                if (c == '"')
                {
                    inDouble = true;
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        args.Add(current.ToString());
                        current.Length = 0;
                    }

                    continue;
                }

                current.Append(c);
            }

            if (inSingle || inDouble)
            {
                throw new CredentialException("invalid process_command: unclosed quote");
            }

            if (current.Length > 0)
            {
                args.Add(current.ToString());
            }

            if (args.Count == 0 || string.IsNullOrEmpty(args[0]))
            {
                throw new CredentialException("process_command is empty");
            }

            return args.ToArray();
        }
    }
}
