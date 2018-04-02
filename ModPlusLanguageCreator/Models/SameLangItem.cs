using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class SameLangItem : BaseNotify
    {
        public SameLangItem()
        {
            SameLangs = new List<string>();
        }

        public SameLangItem(string tag, string workLangName, string nodeName, List<string> sameLangs)
        {
            Tag = tag;
            WorkLangName = workLangName;
            NodeName = nodeName;
            SameLangs = sameLangs;
        }
        public string Tag { get; set; }
        public string WorkLangName { get; set; }
        public string NodeName { get; set; }
        public List<string> SameLangs { get; set; }

        #region Methods

        public static List<SameLangItem> LoadFromFile()
        {
            var curDir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            if (curDir != null)
            {
                var fileName = Path.Combine(curDir, "SameLangs.xml");
                if (File.Exists(fileName))
                {
                    List<SameLangItem> sameLangItems = new List<SameLangItem>();
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XElement xDoc = XElement.Load(fileStream);
                        foreach (XElement itemXel in xDoc.Elements("item"))
                        {
                            List<string> sameLangs = new List<string>();
                            foreach (string s in itemXel.Attribute("SameLangs")?.Value?.Split(';'))
                            {
                                sameLangs.Add(s);
                            }
                            sameLangItems.Add(new SameLangItem(
                                itemXel.Attribute("Tag")?.Value,
                                itemXel.Attribute("WorkLangName")?.Value,
                                itemXel.Attribute("NodeName")?.Value,
                                sameLangs
                                ));
                        }
                    }
                    return sameLangItems;
                }
            }
            return null;
        }

        private static bool HasItem(List<SameLangItem> sameLangItems, SameLangItem sameLangItem)
        {
            foreach (SameLangItem langItem in sameLangItems)
            {
                if (langItem.Tag == sameLangItem.Tag &&
                    langItem.WorkLangName == sameLangItem.WorkLangName &&
                    langItem.NodeName == sameLangItem.NodeName)
                    return true;
            }
            return false;
        }

        public static void SaveToFile(List<SameLangItem> sameLangItems)
        {
            var curDir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            if (curDir != null)
            {
                var fileName = Path.Combine(curDir, "SameLangs.xml");
                if (File.Exists(fileName))
                {
                    var exist = LoadFromFile();
                    if (exist != null)
                    {
                        List<SameLangItem> itemsToAdd = new List<SameLangItem>();
                        foreach (SameLangItem sameLangItem in exist)
                        {
                            if(!HasItem(sameLangItems, sameLangItem))
                                itemsToAdd.Add(sameLangItem);
                        }
                        sameLangItems.AddRange(itemsToAdd);
                    }
                }
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    XElement xDoc = new XElement("SameLanguages");
                    foreach (SameLangItem sameLangItem in sameLangItems)
                    {
                        XElement item = new XElement("item");
                        item.SetAttributeValue(nameof(sameLangItem.WorkLangName), sameLangItem.WorkLangName);
                        item.SetAttributeValue(nameof(sameLangItem.Tag), sameLangItem.Tag);
                        item.SetAttributeValue(nameof(sameLangItem.NodeName), sameLangItem.NodeName);
                        var langs = string.Empty;
                        foreach (string lang in sameLangItem.SameLangs)
                        {
                            langs += lang + ";";
                        }
                        item.SetAttributeValue(nameof(sameLangItem.SameLangs), langs.TrimEnd(';'));
                        xDoc.Add(item);
                    }
                    xDoc.Save(fileStream);
                }
            }
        }

        #endregion
    }
}
