using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class NodeModel : BaseNotify
    {
        private bool _isExpanded;

        public NodeModel(string nodeName, LanguageModel ownerLanguage)
        {
            Items = new ObservableCollection<ItemModel>();
            Attributes = new ObservableCollection<NodeAttributeModel>();
            NodeName = nodeName;
            OwnerLanguage = ownerLanguage;
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

        public LanguageModel OwnerLanguage { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged();}
        }

        public NodeModel CreateEmptyCopy(LanguageModel ownerLang)
        {
            return new NodeModel(NodeName, ownerLang);
        }
    }
}
