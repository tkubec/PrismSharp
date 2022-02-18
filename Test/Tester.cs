using Orionsoft.PrismSharp.Tokenizing;
using System;

namespace Test
{
    /// <summary>
    /// Testing class to verify output compatibility with PrismJS
    /// </summary>
    public static class Tester
    {
        public static void RunAllTests(Tokenizer tokenizer)
        {
            MultipleLanguagesTest(tokenizer, tokenizer.LanguageList.ToArray(), @"..\..\data\tests\core\token\basic\");
            MultipleLanguagesTest(tokenizer, tokenizer.LanguageList.ToArray(), @"..\..\data\tests\examples\token\basic\");

            MultipleLanguagesHtmlTest(tokenizer, tokenizer.LanguageList.ToArray(), @"..\..\data\tests\core\html\basic\");
            MultipleLanguagesHtmlTest(tokenizer, tokenizer.LanguageList.ToArray(), @"..\..\data\tests\examples\html\basic\");
        }

        public static void MultipleLanguagesTest(Tokenizer tokenizer, string[] langs, string testDir)
        {
            foreach (var l in langs)
            {
                Console.WriteLine(l);
                var test = new LanguageTest(tokenizer, l);
                test.Test(testDir + l + @"\");
            }
        }

        public static void MultipleLanguagesHtmlTest(Tokenizer tokenizer, string[] langs, string testDir)
        {
            foreach (var l in langs)
            {
                Console.WriteLine(l);
                var test = new LanguageTest(tokenizer, l);
                test.TestHtml(testDir + l + @"\");
            }
        }
    }
}