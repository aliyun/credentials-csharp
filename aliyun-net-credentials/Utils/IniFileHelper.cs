using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aliyun.Credentials.Utils
{
    public class IniFileHelper
    {
        private readonly Dictionary<string, Dictionary<string, string>> ini =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

        public IniFileHelper(string file)
        {
            var txt = File.ReadAllText(file);
            var currentSection =
                new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            ini[""] = currentSection;

            foreach (var line in txt.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim()))
            {
                if (line.StartsWith(";"))
                {
                    continue;
                }

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    ini[line.Substring(1, line.LastIndexOf("]", StringComparison.Ordinal) - 1).Trim()] = currentSection;
                    continue;
                }

                var idx = line.IndexOf("=", StringComparison.Ordinal);
                if (idx == -1)
                {
                    currentSection[line.Trim()] = "";
                }
                else
                {
                    currentSection[line.Substring(0, idx).Trim()] = line.Substring(idx + 1).Trim();
                }
            }
        }

        public string GetValue(string key)
        {
            return GetValue(key, "", "");
        }

        public string GetValue(string key, string section)
        {
            return GetValue(key, section, "");
        }

        public string GetValue(string key, string section, string @default)
        {
            if (!ini.ContainsKey(section))
            {
                return @default;
            }

            if (!ini[section].ContainsKey(key))
            {
                return @default;
            }

            return ini[section][key].Trim();
        }

        public string[] GetKeys(string section)
        {
            if (!ini.ContainsKey(section))
            {
                return new string[0];
            }

            return ini[section].Keys.ToArray();
        }

        public string[] GetSections()
        {
            return ini.Keys.Where(t => t != "").ToArray();
        }

        public Dictionary<string, Dictionary<string, string>> Ini
        {
            get
            {
                return ini;
            }
        }
    }
}
