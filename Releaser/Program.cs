namespace Releaser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using ModPlusUpdate.Helpers;

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
            if (File.Exists(langsFile))
            {
                Console.WriteLine($"Write to {langsFile}");
                XElement xDoc = XElement.Load(langsFile);
                foreach (XElement xElement in xDoc.Elements("lang"))
                {
                    xElement.Attribute("Version")?.SetValue(version);
                }

                xDoc.Save(langsFile);
                Console.WriteLine("Done!");
            }
            else Console.WriteLine($"File is missing: {langsFile}");

            Console.WriteLine("Push to site? [Y/N]");
            var push = Console.ReadLine();
            if (push?.ToUpper() == "Y")
            {
                var ftpClient = new FtpClient
                {
                    UserName = "imperi20_updater",
                    Host = "imperi20.ftp.ukraine.com.ua",
                    Password = "ANvnn89v6HE9"
                };
                foreach (KeyValuePair<string, string> pair in langs)
                {
                    var file = Path.Combine(outputDir, pair.Value);
                    if (File.Exists(file))
                    {
                        Console.WriteLine($"Uploading file: {file}");
                        ftpClient.UploadFile("/Languages/", file);
                    }
                    else Console.WriteLine($"File is missing: {file}");
                }

                if (File.Exists(langsFile))
                {
                    ftpClient.UploadFile("/", langsFile);
                } else Console.WriteLine($"File is missing: {langsFile}");
            }

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
