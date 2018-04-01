using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class LanguageModel : BaseNotify
    {
        public LanguageModel(string fileName, string version, string name)
        {
            Nodes = new ObservableCollection<NodeModel>();
            FileName = fileName;
            Version = version;
            Name = name;
        }

        public ObservableCollection<NodeModel> Nodes { get; set; }
        public string FileName { get; }
        public string Version { get; set; }
        public string Name { get; set; }

        public void SaveToFile()
        {
            using (FileStream fileStream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                XElement xDoc = new XElement("ModPlus");
                xDoc.SetAttributeValue("Version", Version);
                xDoc.SetAttributeValue("Name", Name);
                // add nodes
                foreach (NodeModel nodeModel in Nodes)
                {
                    XElement nodeXel = new XElement(nodeModel.NodeName);
                    foreach (NodeAttributeModel attributeModel in nodeModel.Attributes)
                    {
                        // same langs!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        nodeXel.SetAttributeValue(attributeModel.Name, attributeModel.Value);
                    }
                    foreach (ItemModel itemModel in nodeModel.Items)
                    {
                        XElement itemXel = new XElement(itemModel.Tag);
                        itemXel.SetValue(itemModel.Value ?? string.Empty);
                        if (itemModel.SameLanguageNames.Any())
                        {
                            var sameLangs = string.Empty;
                            foreach (string s in itemModel.SameLanguageNames)
                            {
                                sameLangs += s + ";";
                            }
                            itemXel.SetAttributeValue("SameLangs", sameLangs.TrimEnd(';'));
                        }
                        nodeXel.Add(itemXel);
                    }
                    xDoc.Add(nodeXel);
                }
                // save
                xDoc.Save(fileStream);
            }
        }
    }
}
