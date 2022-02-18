using Orionsoft.PrismSharp.Highlighters.Abstract;
using Orionsoft.PrismSharp.Highlighters.Html;
using Orionsoft.PrismSharp.Themes;
using Orionsoft.PrismSharp.Tokenizing;
using System.Text;

namespace Test
{
    /// <summary>
    /// Demonstration of a highlighter that uses nested output, the output nested objects inherit formatting from their parents, unless they have another formatting
    /// </summary>

    public class TreeHighlighter : AbstractHighlighter<string>
    {
        private StringBuilder sb;

        public bool ReturnFullBody { get; set; }

        public TreeHighlighter(Tokenizer tokenizer, string themeFile)
        {
            Construct(tokenizer, themeFile);
        }

        protected override ThemeStyle BeginDocument(string language, ThemeStyle docStyle)
        {
            sb = new StringBuilder();
            if (ReturnFullBody) sb.Append(HtmlHelper.docStart);
            sb.Append($"<pre style=\"{HtmlHelper.CreateStyleString(docStyle)}\">");
            return docStyle;
        }

        protected override void EndDocument()
        {
            sb.Append("</pre>");
            if (ReturnFullBody) sb.Append(HtmlHelper.docEnd);
            Result = sb.ToString();
        }

        protected override ThemeStyle BeginContainer(Token token, ThemeStyle style, ThemeStyle parentStyle)
        {
            sb.Append($"<span style=\"{HtmlHelper.CreateStyleString(style)}\">");
            return parentStyle.Clone();
        }

        protected override void EndContainer()
        {
            sb.Append("</span>");
        }

        protected override void AddSpan(string text, Token token, ThemeStyle style, ThemeStyle parentStyle)
        {
            sb.Append($"<span style=\"{HtmlHelper.CreateStyleString(style)}\">{HtmlHighlighter.Encode(text)}</span>");
        }
    }
}