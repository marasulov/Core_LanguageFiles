using System.Windows.Input;
using ModPlusLanguageCreator.Helpers;

namespace ModPlusLanguageCreator.Models
{
    public class NodeAttributeModel : BaseNotify
    {
        private string _value;

        public NodeAttributeModel(string attrName, string attrValue, NodeModel ownerNode, MainViewModel viewModel)
        {
            Name = attrName;
            Value = attrValue;
            OwnerNodeModel = ownerNode;
            _mainViewModel = viewModel;
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
            return new NodeAttributeModel(Name, string.Empty, ownerNodeModel, _mainViewModel);
        }

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
