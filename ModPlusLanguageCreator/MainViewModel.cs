using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using ModPlusLanguageCreator.Helpers;
using ModPlusLanguageCreator.Models;

namespace ModPlusLanguageCreator
{
    public class MainViewModel : BaseNotify
    {
        private readonly string _curDir;
        private MainWindow _mainWindow;

        public MainViewModel()
        {
            
        }

        public MainViewModel(string curDir, MainWindow parentWindow)
        {
            _curDir = curDir;
            _mainWindow = parentWindow;
            // fill lang files
            LocalLanguageFiles = new List<string>();
            foreach (string file in Directory.GetFiles(_curDir, "*.xml", SearchOption.TopDirectoryOnly))
            {
                FileInfo fi = new FileInfo(file);
                LocalLanguageFiles.Add(fi.Name);
            }
            // load main language
            //var mainLangFile = Path.Combine(_curDir, "ru-RU.xml");
            //MainLanguage = LoadLanguageFromFile(mainLangFile, true);
        }
        public LanguageModel MainLanguage { get; set; }
        public LanguageModel WorkLanguage { get; set; }

        public List<string> LocalLanguageFiles { get; set; }

        private LanguageModel LoadLanguageFromFile(string file, bool isReadOnly)
        {
            LanguageModel language = new LanguageModel(file);
            XElement xel = XElement.Load(file);
            foreach (XElement nodeXel in xel.Elements())
            {
                NodeModel nodeModel = new NodeModel(nodeXel.Name.LocalName);
                // get attributes
                foreach (XAttribute attribute in nodeXel.Attributes())
                {
                    NodeAttributeModel nodeAttributeModel = new NodeAttributeModel(attribute.Name.LocalName, attribute.Value);
                    nodeAttributeModel.IsReadOnly = isReadOnly;
                    nodeModel.Attributes.Add(nodeAttributeModel);
                }
                // get items
                foreach (XElement itemXel in nodeXel.Elements())
                {
                    ItemModel item = new ItemModel();
                    item.Tag = itemXel.Name.LocalName;
                    item.Value = itemXel.Value;
                    item.IsReadOnly = isReadOnly;
                    nodeModel.Items.Add(item);
                }
                language.Nodes.Add(nodeModel);
            }
            return language;
        }
    }
}
