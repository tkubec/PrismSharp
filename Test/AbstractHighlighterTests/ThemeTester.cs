using Orionsoft.PrismSharp.Tokenizing;
using System.IO;
using System.Text;

namespace Test
{
    /// <summary>
    /// Generates a test output of short code fragment with various themes applied
    /// </summary>
    public static class ThemeTester
    {
        private const string docStart = "<html><head><link href=\"../../data/theme.css\" rel=\"stylesheet\"/></head><body style=\"background:lightgray\"><link href=\"../../data/milligram.css\" rel=\"stylesheet\"/></head>";
        private const string docEnd = "</body></html>";

        public static void ThemesTests(Tokenizer tokenizer)
        {
            var themes = Directory.GetFiles(@"..\..\..\PrismSharp\data\themes\");
            var language = "javascript";
            var sb = new StringBuilder();
            sb.Append(docStart);

            foreach (var t in themes)
            {
                sb.Append("<h1>" + Path.GetFileNameWithoutExtension(t) + "</h1>\r\n\r\n");
                var uhl = new FlatHighlighter(tokenizer, t);
                var hlt = uhl.Highlight(File.ReadAllText(@"..\..\data\theme.js"), language);
                sb.Append(hlt);
            }
            sb.Append(docEnd);

            File.WriteAllText("themes.html", sb.ToString());
        }
    }
}