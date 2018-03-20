using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class ItemModel : BaseNotify
    {
        private string _value;

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged();}
        }

        public string Tag { get; set; }

        public bool IsReadOnly { get; set; }
    }
}
