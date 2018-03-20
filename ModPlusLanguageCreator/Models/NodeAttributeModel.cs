using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class NodeAttributeModel : BaseNotify
    {
        private string _value;

        public NodeAttributeModel(string attrName)
        {
            Name = attrName;
        }
        public NodeAttributeModel(string attrName, string attrValue)
        {
            Name = attrName;
            Value = attrValue;
        }
        public string Name { get; }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged();}
        }

        public bool IsReadOnly { get; set; }
    }
}
