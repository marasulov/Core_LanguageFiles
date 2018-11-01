namespace Releaser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    class Program
    {
        static void Main(string[] args)
        {
            var outputDir = @"E:\ModPlus\Languages";
            Dictionary<string, string> langs = new Dictionary<string, string>
            {
                { "Releaser.ru-RU.xml", "ru-RU.xml"},
                { "Releaser.en-US.xml", "en-US.xml"}
            };

            foreach (KeyValuePair<string, string> pair in langs)
            {
                Console.WriteLine($"Extract {pair.Key}");
                WriteResourceToFile(pair.Key, Path.Combine(outputDir, pair.Value));
            }

            Console.Read();
        }

        private static void WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }
    }
}
