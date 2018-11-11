namespace Releaser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

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

            XElement langDoc = XElement.Load(Path.Combine(outputDir, langs.First().Value));
            var version = langDoc.Attribute("Version")?.Value;

            var langsFile = @"E:\ModPlus Updates\Langs.xml";
            Console.WriteLine($"Write to {langsFile}");
            XElement xDoc = XElement.Load(langsFile);
            foreach (XElement xElement in xDoc.Elements("lang"))
            {
                xElement.Attribute("Version")?.SetValue(version);
            }
            xDoc.Save(langsFile);
            Console.WriteLine("Done!");
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
