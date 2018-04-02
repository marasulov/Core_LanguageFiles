using System.IO;
using System.Net;
using System.Windows;
using Newtonsoft.Json;

namespace ModPlusLanguageCreator.Helpers
{
    public class YandexTranslator
    {
        public string Translate(string s, string lang)
        {
            if (s.Length > 0)
            {
                WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/translate?"
                                                       + "key=trnsl.1.1.20180321T085924Z.e668b43f5a1b040b.d77a5c74d8cf9fe5b2b25f31ac2cde85bb40d97c"
                                                       + "&text=" + s
                                                       + "&lang=" + lang);
                WebResponse response = null;
                try
                {
                    response = request.GetResponse();
                }
                catch
                {
                    MessageBox.Show("Cannot translate " + lang);
                }
                if(response != null)
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    string line;

                    if ((line = stream.ReadLine()) != null)
                    {
                        Translation translation = JsonConvert.DeserializeObject<Translation>(line);

                        s = "";

                        foreach (string str in translation.text)
                        {
                            s += str;
                        }
                    }
                }

                return s;
            }
            else
                return "";
        }
    }

    class Translation
    {
        public string code { get; set; }
        public string lang { get; set; }
        public string[] text { get; set; }
    }
}
