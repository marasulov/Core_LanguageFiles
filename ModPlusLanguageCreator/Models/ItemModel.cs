using System.Windows.Input;
using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class ItemModel : BaseNotify
    {
        public ItemModel(NodeModel ownerNode, MainViewModel viewModel)
        {
            OwnerNodeModel = ownerNode;
            _mainViewModel = viewModel;
            TranslateCommand = new RelayCommand(Translate);
        }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                _value = MustBeUppercase ? value.ToUpper() : value;
                if(_mainViewModel != null && _mainViewModel.NeedToFindMissing)
                    _mainViewModel.FindMissingItems();
                OnPropertyChanged();
                _mainViewModel?.GetTranslationComplition();
            }
        }

        public string Tag { get; set; }

        public bool IsReadOnly { get; set; }

        public NodeModel OwnerNodeModel { get; }
        private readonly MainViewModel _mainViewModel;

        public bool MustBeUppercase { get; set; }

        public ItemModel CreateEmptyCopy(NodeModel ownerNodeModel)
        {
            return new ItemModel(ownerNodeModel, _mainViewModel) { Tag = Tag };
        }

        public ICommand TranslateCommand { get; set; }
        private void Translate(object o)
        {
            foreach (NodeModel mainLanguageNode in _mainViewModel.MainLanguage.Nodes)
            {
                if (mainLanguageNode.NodeName == OwnerNodeModel.NodeName)
                {
                    foreach (ItemModel mainItem in mainLanguageNode.Items)
                    {
                        if (mainItem.Tag == Tag)
                        {
                            var langFrom = _mainViewModel.CurrentMainLanguageFile.TwoLetterISOLanguageName;
                            var langTo = _mainViewModel.CurrentWorkLanguageFile.TwoLetterISOLanguageName;
                            Value = _mainViewModel.Translator.Translate(mainItem.Value.Replace("\\n"," ").Replace("{0}",""), langFrom + "-" + langTo);

                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}
