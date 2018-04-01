using System.Collections.ObjectModel;
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
            SameLanguageNames = new ObservableCollection<string>();
            TranslateCommand = new RelayCommand(Translate);
        }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                _value = MustBeUppercase ? value.ToUpper() : value;
                if (_mainViewModel != null && _mainViewModel.NeedToFindMissing)
                {
                    _mainViewModel.FindMissingItems();
                    _mainViewModel.FindMissingItemsWithSpecialSymbols();
                }
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

        private bool _isSameWithCurrentMainLanguage;
        /// <summary>
        /// Установка галочки "Это значение одинаково для текущего главного языка"
        /// </summary>
        public bool IsSameWithCurrentMainLanguage
        {
            get => _isSameWithCurrentMainLanguage;
            set
            {
                _isSameWithCurrentMainLanguage = value;
                if (_mainViewModel != null && _mainViewModel.NeedToFindMissing)
                {
                    if (!value)
                        if (SameLanguageNames.Contains(_mainViewModel.MainLanguage.Name))
                            SameLanguageNames.Remove(_mainViewModel.MainLanguage.Name);
                    if (value)
                        if (!SameLanguageNames.Contains(_mainViewModel.MainLanguage.Name))
                            SameLanguageNames.Add(_mainViewModel.MainLanguage.Name);

                    _mainViewModel.FindMissingItems();
                    _mainViewModel.FindMissingItemsWithSpecialSymbols();
                }
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> SameLanguageNames { get; set; }

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
                            Value = _mainViewModel.Translator.Translate(mainItem.Value.Replace("\\n", " ").Replace("{0}", ""), langFrom + "-" + langTo);

                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}
