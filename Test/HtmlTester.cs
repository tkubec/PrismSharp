using Orionsoft.PrismSharp.Highlighters.Html;
using Orionsoft.PrismSharp.Tokenizing;
using System.IO;
using System.Linq;
using System.Text;

namespace Test
{
    /// <summary>
    /// Testing class to verify HTML output compatibility with PrismJS and render examples
    /// </summary>
    public static class HtmlTester
    {
        private const string docStart = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><link href=\"../../../prism-master/themes/prism.css\" rel=\"stylesheet\"/><link href=\"../../data/milligram.css\" rel=\"stylesheet\"/></head><body>";

        public static void RenderHtmlExamples(Tokenizer tokenizer, string[] langs, string testDir)
        {
            var toc = new StringBuilder();
            var hl = new HtmlHighlighter(tokenizer)
            {
                WrapByPre = true
            };

            var output = new StringBuilder();

            var languages = Directory.GetDirectories(testDir);

            foreach (var l in languages)
            {
                if (!langs.Contains(Path.GetFileName(l))) continue;

                var language = Path.GetFileNameWithoutExtension(l);
                output.Append($"<h1 id=\"{language}\">{language}</h1>\r\n");
                toc.Append($"<li><a href=\"#{language}\">{language}</a></li>");
                var examples = Directory.GetFiles(Path.Combine(testDir, l));

                foreach (var e in examples)
                {
                    output.Append($"<h2 >{Path.GetFileNameWithoutExtension(e)}</h2>\r\n");
                    var code = File.ReadAllText(e);
                    var res = hl.Highlight(code, language);
                    output.Append(res);
                }
            }
            output.Append("</body></html>");
            File.WriteAllText("examples.html",
               docStart + "<h1> All Examples for Each Language</h1> <ul>" + toc.ToString() + "</ul>" + output.ToString());
        }

        public static void RenderHtmlRange(Tokenizer tokenizer, Token token, int start, int length, string language, string path)
        {
            var hl = new HtmlHighlighter(tokenizer)
            {
                WrapByPre = true
            };

            var output = new StringBuilder();

            output.Append(docStart);

            var res = hl.HighlightRange(token, start, length, language);
            output.Append(res);

            output.Append("</body></html>");
            File.WriteAllText(path, output.ToString());
        }
    }
}