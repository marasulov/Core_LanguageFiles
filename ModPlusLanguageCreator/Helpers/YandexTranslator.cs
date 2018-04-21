using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Runtime.Serialization.Json;

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
                        using (var streamFromLine = GenerateStreamFromString(line))
                        {
                            var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(Translation));
                            var translation = (Translation) dataContractJsonSerializer.ReadObject(streamFromLine);

                            s = "";

                            foreach (string str in translation.text)
                            {
                                s += str;
                            }
                        }
                    }
                }

                return s;
            }
            else
                return "";
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
    [DataContract]
    class Translation
    {
        [DataMember]
        public string code { get; set; }
        [DataMember]
        public string lang { get; set; }
        [DataMember]
        public string[] text { get; set; }
    }
}
