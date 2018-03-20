using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class NodeModel : BaseNotify
    {
        public NodeModel(string nodeName)
        {
            Items = new ObservableCollection<ItemModel>();
            Attributes = new ObservableCollection<NodeAttributeModel>();
            NodeName = nodeName;
        }

        public ObservableCollection<NodeAttributeModel> Attributes { get; set; }

        public ObservableCollection<ItemModel> Items { get; set; }

        public string NodeName { get; }

        public Visibility AttributesVisibility
        {
            get
            {
                if (Attributes.Any()) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
    }
}
