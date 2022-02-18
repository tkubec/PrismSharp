using Orionsoft.PrismSharp.Highlighters.Html;
using Orionsoft.PrismSharp.Tokenizing;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Test
{    /// <summary>
     /// Testing class to verify output compatibility with PrismJS
     /// </summary>
    public class SingleHtmlTest
    {
        private readonly Tokenizer tokenizer;
        private readonly string language;
        private string code;
        private string rawTemplateStream;
        private string res;

        public SingleHtmlTest(Tokenizer tokenizer, string language)
        {
            this.tokenizer = tokenizer;
            this.language = language;
        }

        public bool Test(string test)
        {
            var split = Regex.Split(test, "^-{20,}\\r?$", RegexOptions.Multiline);
            if (split.Length < 2) throw new ArgumentException("Invalid test");

            code = split[0].Trim();
            rawTemplateStream = split[1].Trim();

            var hl = new HtmlHighlighter(tokenizer);
            res = hl.Highlight(code, language);

            return res.Replace("\r\n", "\n") == rawTemplateStream.Replace("\r\n", "\n");
        }

        public bool TestFile(string path)
        {
            return Test(File.ReadAllText(path));
        }

        public void Dump()
        {
            Console.WriteLine(code);
            Console.WriteLine("----------------------------------------------");

            Console.WriteLine("Raw Template:");
            Console.WriteLine(rawTemplateStream);

            Console.WriteLine("\r\nResult:");
            Console.WriteLine(res);
        }
    }
}