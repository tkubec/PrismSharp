namespace Importer
{
    public static class Program
    {
        private static void Main()
        {
            ExtractExamples();
            ImportPrismJSThemes();
            ExtensionMapper.PrintCLike();
        }

        private static void ImportPrismJSThemes()
        {
            var te = new ThemeImporter();
            te.ImportDir(@"../../../prism-master/themes/", "../../../PrismSharp/data/themes");
            te.ImportDir(@"../../../prism-themes-master/themes/", "../../../PrismSharp/data/themes");
        }

        private static void ExtractExamples()
        {
            var examples = new ExampleImporter();
            examples.ExtractAllSnippets(@"../../../prism-master/examples", "../../../Test/data/examples", false);
            examples.ExtractAllSnippets(@"../../../prism-master/examples", "../../../Test/data/tests/examples/exampleTestBase", true);
        }
    }
}