using Orionsoft.PrismSharp.Themes;
using System.Text;

namespace Test
{
    public static class HtmlHelper
    {
        public const string docStart = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><link href=\"../../data/theme.css\" rel=\"stylesheet\"/></head><body style=\"background:lightgray\">";
        public const string docEnd = "</body></html>";

        public static string CreateStyleString(ThemeStyle style)
        {
            if (style == null) return "";
            var sb = new StringBuilder();
            if (style.Color != null) sb.Append("color: " + style.Color.ToColorString() + ";");
            if (style.Background != null) sb.Append("background: " + style.Background.ToColorString() + ";");
            return sb.ToString();
        }
    }
}