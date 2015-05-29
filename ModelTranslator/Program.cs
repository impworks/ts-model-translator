namespace ModelTranslator
{
    using System;
    using System.IO;


    class Program
    {
        static void Main(string[] args)
        {
            var filePath = "TestTaskModel.cs";
            using (var sr = new StreamReader(filePath))
            {
                var translator = new Translator();
                var result = translator.Translate(sr.ReadToEnd());

                Console.WriteLine(result);
                Console.ReadLine();
            }
        }
    }
}
