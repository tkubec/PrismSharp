using Orionsoft.PrismSharp.Tokenizing;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Test
{
    public partial class SimpleStreamParser
    {
        public Token Parse(string input)
        {
            var res = new Token();
            var array = JSArray.FromText(input);
            ParseArray(array, res);
            return res;
        }

        private void ParseArray(JSArray item, Token res)
        {
            if (item.FinalContent != null)
            {
                res.Text = StripQuotes(item.FinalContent);
                return;
            }

            if (item.Items.Count == 2 && item.Items[0].FinalContent != null && item.Items[0].FinalContent.StartsWith("!"))
            {
                // it's a pair

                res.Type = StripQuotes(item.Items[0].FinalContent);

                if (item.Items[1].FinalContent != null)
                {
                    res.Text = StripQuotes(item.Items[1].FinalContent);
                    return;
                }

                if (item.Items[1].Items.Count == 1 && item.Items[1].Items[0].FinalContent != null)
                {
                    res.Text = StripQuotes(item.Items[1].Items[0].FinalContent);
                    return;
                }

                if (item.Items[1].Items.Count == 0)
                {
                    var nt = new Token
                    {
                        Text = ""
                    };
                    res.Tokens.Add(nt);
                    res.Text = "";
                }
                foreach (var i in item.Items[1].Items)
                {
                    var nt = new Token();
                    res.Tokens.Add(nt);
                    ParseArray(i, nt);
                }
                if (res.Tokens.Count == 0 && res.Text == null)
                {
                    res.Text = "";
                }
                return;
            }

            foreach (var i in item.Items)
            {
                var nt = new Token();
                res.Tokens.Add(nt);
                ParseArray(i, nt);
            }
            if (res.Tokens.Count == 1 && res.Tokens[0].Tokens.Count == 0 && res.Tokens[0].Type == null)
            {
                res.Text = res.Tokens[0].Text;
                res.Tokens.RemoveAt(0);
            }
        }

        private string StripQuotes(string v)
        {
            if (v.StartsWith("!")) v = v.Substring(1);
            return Regex.Unescape(v.Substring(1, v.Length - 2));
        }

        public static bool IsPrettyEqual(string readStream, string parsedStream)
        {
            var same = StripStream(readStream, rawTemplate: true) == StripStream(parsedStream);
            return same;
        }

        internal static string StripStream(string streamText, bool rawTemplate = false)
        {
            var split = streamText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var res = string.Join("", split.Select(x => x.Trim()));

            if (rawTemplate)
            {
                var singleArray = new Regex("\\[(([\"'])(?:\\\\(?:\\r\\n|[\\s\\S])|(?!\\2)[^\\\\\\r\\n])*\\2)\\]");

                res = singleArray.Replace(res, "$1", 999);
                res = res.Replace("\\\"", "\"");
            }

            res = res.Replace("\\\\", "\\");
            res = res.Replace(", ", ",");
            return res;
        }
    }
}