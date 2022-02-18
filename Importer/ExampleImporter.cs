using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Importer
{
    public class ExampleImporter
    {
        public List<(string Name, string code)> ExtractSnippets(string filename)
        {
            var res = new List<(string Name, string Code)>();

            string name = null;
            var doc = new HtmlDocument();
            doc.Load(filename, Encoding.UTF8);
            var nodes = doc.DocumentNode.SelectNodes("//pre/code");
            foreach (var node in nodes)
            {
                var prev = node.ParentNode.PreviousSibling;
                if (prev.Name == "#text") prev = prev.PreviousSibling;

                if (prev.Name.ToLower() == "h2")
                {
                    name = prev.InnerText.Trim();
                }

                if (name == null)
                {
                    name = "Untitled " + nodes.IndexOf(node);
                }

                var code = node.InnerText;
                code = code.StartsWith("\r\n") ? code = code.Substring(2) : code;
                code = code.StartsWith("\n") ? code = code.Substring(1) : code;

                code = code.EndsWith("\r\n") ? code = code.Substring(0, code.Length - 2) : code;
                code = code.EndsWith("\n") ? code = code.Substring(0, code.Length - 1) : code;

                res.Add((name, System.Net.WebUtility.HtmlDecode(code)));
            }
            return res;
        }

        public void ExtractAllSnippets(string sourcePath, string outputPath, bool asTest)
        {
            Directory.CreateDirectory(outputPath);
            var files = Directory.GetFiles(sourcePath, "*.html");

            foreach (var f in files)
            {
                var snippets = ExtractSnippets(f);

                var lang = Path.GetFileNameWithoutExtension(f);
                lang = lang.StartsWith("prism-") ? lang.Substring(6) : lang;
                var dir = Path.Combine(outputPath, lang);
                Directory.CreateDirectory(dir);

                foreach (var (Name, code) in snippets)
                {
                    var name = Name.Replace("\"", "");

                    if (asTest)
                    {
                        File.WriteAllText(Path.Combine(dir, name) + ".test", code + "\r\n\r\n" + new string('-', 50) + "\r\n\r\n");
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(dir, name) + ".code", code);
                    }
                }
            }
        }
    }
}