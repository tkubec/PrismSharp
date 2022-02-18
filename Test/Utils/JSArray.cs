using Orionsoft.PrismSharp.Tokenizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Test
{
    public class JSArray
    {
        public List<JSArray> Items { get; set; } = new List<JSArray>();
        public string FinalContent { get; set; }

        private JSArray()
        { }

        private JSArray(List<string> simpleTokens)
        {
            var subBlocks = GetBlocks(simpleTokens);
            foreach (var b in subBlocks)
            {
                if (b.Count == 1)
                {
                    FinalContent = b[0];
                }
                else
                    Items.Add(new JSArray(b));
            }
        }

        public static JSArray FromText(string input)
        {
            var simpleTokens = Tokenize(input);

            var blocks = JSArray.GetBlocks(simpleTokens);
            if (blocks.Count != 1) throw new TokenizerException("Invalid simple stream");

            var array = new JSArray();
            Parse(blocks[0], array);
            return array;
        }

        private static List<List<string>> GetBlocks(List<string> tokens)
        {
            var res = new List<List<string>>();
            var level = 0;

            foreach (var token in tokens)
            {
                if (token == "[" && level == 0)
                {
                    level++;
                    res.Add(new List<string>());
                    continue;
                }

                if (token == "[" && level > 0)
                {
                    level++;
                    res.Last().Add(token);
                    continue;
                }
                if (token == "]" && level == 1)
                {
                    level--;
                    continue;
                }
                if (token == "]" && level == 0)
                {
                    break;
                }

                if (token == "]" && level > 1)
                {
                    level--;
                    res.Last().Add(token);
                    continue;
                }

                if (level > 0)
                {
                    res.Last().Add(token);
                    continue;
                }
                if (level == 0)
                {
                    res.Add(new List<string>());
                    res.Last().Add(token);
                    continue;
                }
                throw new TokenizerException("Should not get here");
            }
            return res;
        }

        private static List<string> Tokenize(string input)
        {
            var strRx = new Regex("([\"'])(?:\\\\(?:\\r\\n|[\\s\\S])|(?!\\1)[^\\\\\\r\\n])*\\1");
            var nodeRx = new Regex("(\\[|\\])");
            var delimiterRx = new Regex(",");
            var ws = new Regex("\\s+");
            input = input.Replace("\r", "").Replace("\n", "");
            var pos = 0;

            var output = new List<string>();

            while (pos < input.Length)
            {
                var match = strRx.Match(input, pos);
                if (match.Success && match.Index == pos)
                {
                    var v = ((pos > 0 && input[pos - 1] == '[') ? "!" : "") + match.Groups[0].Value;
                    output.Add(v);
                    pos += match.Groups[0].Length;
                    continue;
                }

                match = nodeRx.Match(input, pos);
                if (match.Success && match.Index == pos)
                {
                    output.Add(match.Groups[1].Value);
                    pos += match.Groups[0].Length;
                    continue;
                }

                match = delimiterRx.Match(input, pos);
                if (match.Success && match.Index == pos && match.Groups[0].Length > 0)
                {
                        pos += match.Groups[0].Length;
                        continue;
                }

                match = ws.Match(input, pos);
                if (match.Success && match.Index == pos)
                {
                    pos += match.Groups[0].Length;
                    continue;
                }
                throw new TokenizerException("SimpleStream tokenization error");
            }
            return output;
        }

        private static int Parse(List<string> simpleTokens, JSArray output, int pos = 0)
        {
            while (pos < simpleTokens.Count)
            {
                if (simpleTokens[pos] == "[")
                {
                    pos++;
                    var si = new JSArray();
                    output.Items.Add(si);
                    pos = Parse(simpleTokens, si, pos);
                    continue;
                }

                if (simpleTokens[pos] == "]")
                {
                    pos++;
                    return pos;
                }
                var ti = new JSArray
                {
                    FinalContent = simpleTokens[pos]
                };
                output.Items.Add(ti);
                pos++;
            }
            return pos;
        }

        public override string ToString()
        {
            if (FinalContent != null) return FinalContent;
            return "Items: " + Items.Count;
        }
    }
}