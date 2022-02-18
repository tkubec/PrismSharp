using Orionsoft.PrismSharp.Tokenizing;

namespace Test
{
    internal static class Program
    {
        private static void Main()
        {
            Demo.Html();
            Demo.Rtf();

            var tokenizer = new Tokenizer();
            Tester.RunAllTests(tokenizer);
            ThemeTester.ThemesTests(tokenizer);
            HtmlTester.RenderHtmlExamples(tokenizer, tokenizer.LanguageList.ToArray(), "../../data/examples/");
        }
    }
}