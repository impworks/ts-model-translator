namespace ModelTranslator
{
    using System;
    using System.IO;


    class Program
    {
        static void Main(string[] args)
        {
            var currDir = Directory.GetCurrentDirectory();
            var inDir = Path.Combine(currDir, "in");
            var outDir = Path.Combine(currDir, "out");

            var count = 0;
            foreach (var inFilePath in Directory.EnumerateFiles(inDir, "*.cs", SearchOption.AllDirectories))
            {
                using (var sr = new StreamReader(inFilePath))
                {   
                    var translator = new Translator();
                    var result = translator.Translate(sr.ReadToEnd());

                    var outFilePath = Path.Combine(outDir, Path.ChangeExtension(inFilePath.Substring(inDir.Length + 1), "ts"));
                    Directory.CreateDirectory(Path.GetDirectoryName(outFilePath));
                    using(var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write))
                    using (var sw = new StreamWriter(fs))
                        sw.Write(result);
                }

                count++;
            }

            Console.WriteLine("Processed {0} file(s).", count);
            Console.ReadLine();
        }
    }
}
