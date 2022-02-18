using ExCSS;
using Newtonsoft.Json;
using Orionsoft.PrismSharp.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Importer
{
    public class ThemeImporter
    {
        public Theme Theme { get; private set; }
        public List<ThemeStyle> Styles { get; private set; }

        public void Import(string srcFile, string targetFile)
        {
            Theme = new Theme();
            Styles = new List<ThemeStyle>();
            var parser = new StylesheetParser();
            var stylesheet = parser.Parse(File.ReadAllText(srcFile));
            GetDocumentStyles(stylesheet);
            GetTokenStyles(stylesheet);

            Theme.Styles = Styles;

            // patches
            if (srcFile.Contains("synthwave84"))
            {
                var p = Styles.FirstOrDefault(x => x.Type == "~");
                p.Background = new RgbaColor { R = 30, G = 30, B = 60, A = 1 };
            }

            File.WriteAllText(targetFile, JsonConvert.SerializeObject(Theme, Formatting.Indented));
        }

        private void GetTokenStyles(Stylesheet stylesheet)
        {
            var tokenRules = stylesheet.StyleRules.Where(x => x.SelectorText.Contains("token"));

            foreach (var rule in tokenRules)
            {
                ThemeStyle styling = null;
                var selectors = rule.Selector.Text.Split(new[] { ',' });

                foreach (var selector in selectors)
                {
                    var selectorParts = selector.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (selectorParts.Count() > 2)
                    {
                        Console.WriteLine("Warning: selector not supported:" + selector);
                        continue;
                    }

                    string language = null;
                    if (selectorParts.Count() == 2)
                    {
                        if (Regex.IsMatch(selectorParts.ToArray()[0], "^\\.language-\\w*$"))

                        {
                            language = selectorParts.ToArray()[0].Substring(10);
                            selectorParts = selectorParts.Skip(1).ToArray();
                        }
                        else
                        {
                            Console.WriteLine("Warning: selector not supported:" + selector);
                            continue;
                        }
                    }

                    var classes = selectorParts[0].Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Where(x => x != "token");

                    if (classes.Count() > 1)
                    {
                        Console.WriteLine("Warning: selector not supported:" + selector);
                    }

                    if (!classes.Any()) continue;
                    styling = styling ?? GetStyling(rule);
                    ApplyStyling(classes.First(), language, styling);
                }
            }
        }

        private void GetDocumentStyles(Stylesheet stylesheet)
        {
            var languageRules = stylesheet.StyleRules.Where(x => Regex.Match(x.SelectorText, "(\\bpre\\b)|(\\bcode\\b)").Success);

            foreach (var r in languageRules)
            {
                ThemeStyle styling = null;
                var selectors = r.Selector.Text.Split(new[] { ',' });

                foreach (var s in selectors)
                {
                    var match = Regex.Match(s, "^(?:pre|code)\\[class\\*?=\"language-([^\"]*)\"\\](?!.*:)");
                    if (match.Success)
                    {
                        var lang = match.Groups[1].Value.ToString();
                        lang = string.IsNullOrEmpty(lang) ? null : lang;
                        styling = styling ?? GetStyling(r);
                        ApplyStyling("~", lang, styling);
                        continue;
                    }

                    match = Regex.Match(s, "^(?:pre|code)\\.language-(\\w*)$");
                    if (match.Success)
                    {
                        var lang = match.Groups[1].Value.ToString();
                        lang = string.IsNullOrEmpty(lang) ? null : lang;
                        styling = styling ?? GetStyling(r);
                        ApplyStyling("~", lang, styling);
                    }
                }
            }
        }

        private void ApplyStyling(string className, string language, ThemeStyle styling)
        {
            var style = Styles.FirstOrDefault(x => x.Type == className && x.Language == language);

            if (style != null)
            {
                style.Color = style.Color ?? styling.Color?.Clone();
                style.Background = style.Background ?? styling.Background?.Clone();
                style.Bold = style.Bold ?? styling.Bold;
                style.Italic = style.Italic ?? styling.Italic;
                style.Underline = style.Underline ?? styling.Underline;
                style.Opacity = style.Opacity ?? styling.Opacity;
            }
            else
            {
                style = styling.Clone();
                style.Type = className;
                style.Language = language;
                Styles.Add(style);
            }
        }

        private static ThemeStyle GetStyling(IStyleRule r)
        {
            var format = new ThemeStyle();
            foreach (var a in r.Style)
            {
                switch (a.Name)
                {
                    case "color":
                        {
                            if (format.Color != null) Console.WriteLine("warning, opacity mix");
                            format.Color = ParseColor(a.Value); break;
                        }

                    case "background-color": format.Background = ParseColor(a.Value); break;
                    case "font-weight":
                        {
                            if (a.Value == "bold" || a.Value == "700")
                            {
                                format.Bold = true;
                            }
                            else if (a.Value == "normal" || a.Value == "400")
                            {
                                format.Bold = false;
                            }
                            else
                            {
                                Console.WriteLine("Warning unknown font weight " + a.Value);
                            }

                            break;
                        }
                    case "font-style":
                        {
                            if (a.Value != "italic") Console.WriteLine("warning: " + a.Value);
                            format.Italic = true;
                            break;
                        }

                    case "text-decoration":
                        {
                            if (a.Value != "underline") Console.WriteLine("warning: " + a.Value);
                            format.Underline = true;
                            break;
                        }

                    case "opacity":
                        {
                            format.Opacity = double.Parse(a.Value, CultureInfo.InvariantCulture);
                            break;
                        }

                    case "cursor": break;
                    default:
                        {
                            Debug.WriteLine("Skipping: " + a.Name + ": " + a.Value);
                            break;
                        }
                }
            }
            return format;
        }

        private static RgbaColor ParseColor(string value)
        {
            RgbaColor res;

            // rgb()
            var match = Regex.Match(value, "^rgb\\((\\d{1,3}),\\s*(\\d{1,3}),\\s*(\\d{1,3})\\)$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return new RgbaColor
                {
                    R = byte.Parse(match.Groups[1].Value),
                    G = byte.Parse(match.Groups[2].Value),
                    B = byte.Parse(match.Groups[3].Value),
                    A = 1
                };
            }

            // rgba()
            match = Regex.Match(value, "^rgba\\((\\d{1,3}),\\s*(\\d{1,3}),\\s*(\\d{1,3}),\\s*([\\d.]+)\\)$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return new RgbaColor
                {
                    R = byte.Parse(match.Groups[1].Value),
                    G = byte.Parse(match.Groups[2].Value),
                    B = byte.Parse(match.Groups[3].Value),
                    A = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture)
                };
            }

            // hsl
            match = Regex.Match(value, @"^hsl\((\d+)deg,\s*(\d+)%,\s+(\d+)%\)$");
            if (match.Success)
            {
                res = ColorFromHSL(double.Parse(match.Groups[1].Value), double.Parse(match.Groups[2].Value), double.Parse(match.Groups[3].Value));
                res.A = 1;
                return res;
            }

            // hsla
            match = Regex.Match(value, @"^hsla\((\d+)deg,\s*(\d+)%,\s+(\d+)%,\s*([0-9.]+)\)$");
            if (match.Success)
            {
                res = ColorFromHSL(double.Parse(match.Groups[1].Value), double.Parse(match.Groups[2].Value), double.Parse(match.Groups[3].Value));
                res.A = (double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture));
                return res;
            }
            if (value == "initial") return null;
            if (value == "inherit") return null;
            throw new ArgumentException("Unrecognized color " + value);
        }

        /// <summary>
        /// deg, pct, pct
        /// </summary>
        public static RgbaColor ColorFromHSL(double h, double s, double l)
        {
            h /= 360.0;
            s /= 100.0;
            l /= 100.0;
            double r = 0, g = 0, b = 0;
            if (l != 0)
            {
                if (s == 0)
                    r = g = b = l;
                else
                {
                    double temp2;
                    if (l < 0.5)
                        temp2 = l * (1.0 + s);
                    else
                        temp2 = l + s - (l * s);

                    double temp1 = 2.0 * l - temp2;

                    r = GetColorComponent(temp1, temp2, h + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, h);
                    b = GetColorComponent(temp1, temp2, h - 1.0 / 3.0);
                }
            }
            return new RgbaColor { R = (byte)(255 * r), G = (byte)(255 * g), B = (byte)(255 * b) };
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;

            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }

        internal void ImportDir(string dir, string target)
        {
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            var files = Directory.GetFiles(dir, "*.css");
            foreach (var f in files.Where(x => !x.EndsWith(".min.css") && !x.Contains("coy-without-shadows")))
            {
                Console.WriteLine("Converting :" + Path.GetFileName(f));
                Import(f, Path.Combine(target, Path.GetFileNameWithoutExtension(f)).Replace("prism-", "") + ".json");
            }
        }
    }
}