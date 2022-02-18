using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Importer
{
    public static class ExtensionMapper
    {
        public static Dictionary<string, string> ListExtentedLanguages()
        {
            var res = new Dictionary<string, string>();
            var files = Directory.GetFiles(@"..\..\..\prism-master\components\", "*.js").Where(x => !x.EndsWith(".min.js") && !x.Contains("prism-core"));

            foreach (var f in files)
            {
                var txt = File.ReadAllText(f);
                var match = Regex.Match(txt, "Prism.languages.extend\\('([^']*)'");
                if (match.Success)
                {
                    res.Add(Path.GetFileNameWithoutExtension(f).Replace("prism-", ""), match.Groups[1].Value);
                }
            }
            var goOn = true;
            while (goOn)
            {
                goOn = false;
                foreach (var l in res.ToList())
                {
                    if (res.Keys.Contains(l.Value) && res[l.Key] != res[l.Value])
                    {
                        res[l.Key] = res[l.Value];
                        goOn = true;
                    }
                }
            }
            return res;
        }

        internal static void PrintCLike()
        {
            var list = ExtensionMapper.ListExtentedLanguages();
            var res = list.Where(x => x.Value == "clike").Select(x => "'" + x.Key + "'");

            Console.WriteLine("'clike', " + string.Join(", ", res.ToArray()));
        }
    }
}