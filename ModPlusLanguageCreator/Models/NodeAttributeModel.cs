using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class NodeAttributeModel : BaseNotify
    {
        private string _value;

        public NodeAttributeModel(string attrName, string attrValue, NodeModel ownerNode, MainViewModel viewModel, List<string> sameLangs)
        {
            Name = attrName;
            Value = attrValue;
            OwnerNodeModel = ownerNode;
            _mainViewModel = viewModel;
            SameLanguageNames = new List<string>();
            if (sameLangs != null)
                foreach (var sameLang in sameLangs)
                {
                    SameLanguageNames.Add(sameLang);
                }
            TranslateCommand = new RelayCommand(Translate);
        }
        public string Name { get; }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                if(_mainViewModel != null && _mainViewModel.NeedToFindMissing)
                    _mainViewModel.FindMissingAttributes();
                OnPropertyChanged();
                _mainViewModel?.GetTranslationComplition();
            }
        }

        public bool IsReadOnly { get; set; }

        public NodeModel OwnerNodeModel { get; }
        private readonly MainViewModel _mainViewModel;

        public NodeAttributeModel CreateEmptyCopy(NodeModel ownerNodeModel)
        {
            return new NodeAttributeModel(Name, string.Empty, ownerNodeModel, _mainViewModel, null);
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

                    _mainViewModel.FindMissingAttributes();
                }
                OnPropertyChanged();
            }
        }
        public List<string> SameLanguageNames { get; set; }

        public ICommand TranslateCommand { get; set; }
        private void Translate(object o)
        {
            foreach (NodeModel mainLanguageNode in _mainViewModel.MainLanguage.Nodes)
            {
                if (mainLanguageNode.NodeName == OwnerNodeModel.NodeName)
                {
                    foreach (NodeAttributeModel mainNodeAttr in mainLanguageNode.Attributes)
                    {
                        if (mainNodeAttr.Name == Name)
                        {
                            var langFrom = _mainViewModel.CurrentMainLanguageFile.TwoLetterISOLanguageName;
                            var langTo = _mainViewModel.CurrentWorkLanguageFile.TwoLetterISOLanguageName;
                            Value = _mainViewModel.Translator.Translate(mainNodeAttr.Value, langFrom + "-" + langTo);

                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}
