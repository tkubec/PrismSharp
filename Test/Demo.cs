using Orionsoft.PrismSharp.Highlighters.Html;
using Orionsoft.PrismSharp.Highlighters.Rtf;
using Orionsoft.PrismSharp.Themes;
using System.IO;

namespace Test
{
    /// <summary>
    /// Samples used in the documentaton
    /// </summary>
    internal static class Demo
    {
        internal static void Html()
        {
            var code = "Console.WriteLine(\"Hello, World!\"); // demo";
            var beginning = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\">" +
                "<link href=\"https://cdnjs.cloudflare.com/ajax/libs/prism/1.27.0/themes/prism.min.css\" rel=\"stylesheet\"/</head><body>";
            var ending = "</body></html>";

            var highlighter = new HtmlHighlighter
            {
                WrapByPre = true
            };
            var res = highlighter.Highlight(code, "csharp");

            File.WriteAllText("output.html", beginning + res + ending);
        }

        internal static void Rtf()
        {
            var code = "Console.WriteLine(\"Hello, World!\"); // demo";

            var highlighter = new RtfHighlighter(ThemeNames.Vs)
            {
                Font = "Consolas"
            };
            var res = highlighter.Highlight(code, "csharp");

            File.WriteAllText("output.rtf", res);
        }
    }
}