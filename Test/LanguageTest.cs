using Orionsoft.PrismSharp.Tokenizing;
using System;
using System.IO;

namespace Test
{
    /// <summary>
    /// Testing class to verify output compatibility with PrismJS
    /// </summary>
    public class LanguageTest
    {
        private readonly Tokenizer tokenizer;
        private readonly string language;

        public LanguageTest(Tokenizer tokenizer, string language)
        {
            this.tokenizer = tokenizer;
            this.language = language;
        }

        public bool Test(string dir)
        {
            var success = true;
            if (!Directory.Exists(dir)) return false;
            var files = Directory.GetFiles(dir, "*.test");

            foreach (var f in files)
            {
                if (f.EndsWith(".html.test")) continue;
                var singleTest = new SingleTest(tokenizer, language);
                var res = singleTest.Test(File.ReadAllText(f));
                success &= res;
                if (!res)
                {
                    Console.WriteLine("FAILED: " + language + " " + Path.GetFileName(f));
                    //singleTest.Dump()
                }
            }
            return success;
        }

        public bool TestHtml(string dir)
        {
            var success = true;
            if (!Directory.Exists(dir)) return false;
            var files = Directory.GetFiles(dir, "*.htmlTest");

            foreach (var f in files)
            {
                var singleTest = new SingleHtmlTest(tokenizer, language);
                var res = singleTest.Test(File.ReadAllText(f));
                success &= res;
                if (!res)
                {
                    Console.WriteLine("FAILED: " + language + " " + Path.GetFileName(f));
                    singleTest.Dump();
                }
            }
            return success;
        }
    }
}