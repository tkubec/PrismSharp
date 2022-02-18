using Orionsoft.PrismSharp.Highlighters.Abstract;
using Orionsoft.PrismSharp.Highlighters.Html;
using Orionsoft.PrismSharp.Themes;
using Orionsoft.PrismSharp.Tokenizing;
using System.Text;

namespace Test
{
    /// <summary>
    /// Demonstration of a highlighter that does not use nested output, it renders just text spans with formatting
    /// </summary>
    public class FlatHighlighter : AbstractHighlighter<string>
    {
        private readonly StringBuilder sb = new StringBuilder();

        public bool ReturnFullBody { get; set; }

        public FlatHighlighter(Tokenizer tokenizer, Theme theme)
        {
            Construct(tokenizer, theme);
        }

        public FlatHighlighter(Tokenizer tokenizer, string themeFileName)
        {
            Construct(tokenizer, themeFileName);
        }

        protected override ThemeStyle BeginDocument(string language, ThemeStyle docStyle)
        {
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
            return style?.MergeWith(parentStyle) ?? parentStyle.Clone();
        }

      
        protected override void AddSpan(string text, Token token, ThemeStyle style, ThemeStyle parentStyle)
        {
            var st = style?.MergeWith(parentStyle) ?? parentStyle.Clone();
            sb.Append($"<span style=\"{HtmlHelper.CreateStyleString(st)}\">{HtmlHighlighter.Encode(text)}</span>");
        }
    }
}