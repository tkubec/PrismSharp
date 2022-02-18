using Orionsoft.PrismSharp.Tokenizing;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Test
{    /// <summary>
     /// Testing class to verify token output compatibility with PrismJS
     /// </summary>
    public class SingleTest
    {
        private readonly Tokenizer tokenizer;
        private readonly string language;
        private string code;
        private string rawTemplateStream;
        private Token readToken;
        private string readTemplateStream;
        private Token parsedToken;
        private string parsedStream;

        public SingleTest(Tokenizer tokenizer, string language)
        {
            this.tokenizer = tokenizer;
            this.language = language;
        }

        public bool Test(string test)
        {
            var split = Regex.Split(test, "^-{20,}\\r?$", RegexOptions.Multiline);
            if (split.Length < 2) throw new ArgumentException("Invalid test");

            code = split[0].TrimEnd();
            rawTemplateStream = split[1].TrimEnd();

            var ssp = new SimpleStreamParser();
            readToken = ssp.Parse(rawTemplateStream);
            readTemplateStream = readToken.ToSimpleStream(pretty: true).ToString();

            parsedToken = tokenizer.Tokenize(split[0].TrimEnd(), language);

            var eq = parsedToken.IsPrettyEqual(readToken);
            parsedStream = parsedToken.ToSimpleStream(pretty: true).ToString();

            var success = eq;
            if (!success && parsedToken.GetFirstDiff(readToken) == "no difference") success = true;

            if (!success) Console.WriteLine(eq);
            return success;
        }

        public bool TestFile(string path)
        {
            return Test(File.ReadAllText(path));
        }

        public bool IsStreamReadWell()
        {
            return SimpleStreamParser.IsPrettyEqual(rawTemplateStream, readTemplateStream);
        }

        public void Dump()
        {
            Console.WriteLine(readToken.GetFirstDiff(parsedToken));
            Console.WriteLine(code);
            Console.WriteLine("----------------------------------------------");

            Console.WriteLine("Read Template:");
            Console.WriteLine(readTemplateStream);

            Console.WriteLine("\r\nResult:");
            Console.WriteLine(parsedStream);
        }
    }
}