using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Helpers
{
    public static class StringFuncs
    {
        public static string RemoveWhitespace(this string In)
        {
            int offset = 0;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i + offset < In.Length; i++)
            {
                while (char.IsWhiteSpace(In[i + offset]))
                    offset++;
                builder.Append(In[i + offset]);
            }
            return builder.ToString();
        }

        private static Regex Trimmer = new Regex(@"^\s*(.+)\s*$", RegexOptions.Compiled);
        public static string Trim(this string In)
        {
            var match = Trimmer.Match(In);
            return match.Success ? match.Groups[1].Value : In;
        }

        public static string Combine(this string Base, params string[] Others)
        {
            return Others.Aggregate((a, b) => a + Base + b);
        }

        public static string Slice(this string Base, int Start = 0, int Stop = -1, int Step = 1)
        {
            return new string(Base.Skip(Start).TakeWhile((c, i) => i % Step == 0 && (Stop == -1 || i < Start + Stop)).ToArray());
        }

        public static string QuickFormat(this string Base, params object[] Params)
        {
            return string.Format(Base, Params);
        }

        public static string Repeat(this string Base, int Repetitions)
        {
            return string.Concat(Enumerable.Repeat(Base, Repetitions));
        }
    }
}
