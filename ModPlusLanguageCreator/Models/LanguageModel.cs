using System.Collections.ObjectModel;
using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class LanguageModel : BaseNotify
    {
        public LanguageModel(string fileName)
        {
            Nodes = new ObservableCollection<NodeModel>();
            FileName = fileName;
        }

        public ObservableCollection<NodeModel> Nodes { get; set; }
        public string FileName { get; }
        public string Version { get; set; }
    }
}
