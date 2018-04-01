using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ModPlusLanguageCreator.Helpers;
using ModPlusLanguageCreator.Models;

namespace ModPlusLanguageCreator
{
    public class MainViewModel : BaseNotify
    {
        public YandexTranslator Translator;
        private readonly CultureInfo[] _cultures;
        private readonly string _curDir;
        private readonly MainWindow _mainWindow;
        private LangItem _currentMainLanguageFile;
        private LangItem _currentWorkLanguageFile;
        private LanguageModel _mainLanguage;
        private LanguageModel _workLanguage;
        private double _translationComplition;

        public MainViewModel()
        {

        }

        public MainViewModel(string curDir, MainWindow parentWindow)
        {
            Translator = new YandexTranslator();
            _cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            _curDir = curDir;
            _mainWindow = parentWindow;
            // fill lang files
            LanguagesForMain = new ObservableCollection<LangItem>();
            LanguagesForWork = new ObservableCollection<LangItem>();
            // fill main languages
            FillLanguageFiles(LanguagesForMain);
            // commands
            CreateNewLanguageFileCommand = new RelayCommand(CreateNewLanguageFile);
        }

        public bool IsLoadingLanguageProcess { get; set; }

        public LanguageModel MainLanguage
        {
            get => _mainLanguage;
            set { _mainLanguage = value; OnPropertyChanged(); }
        }

        public LanguageModel WorkLanguage
        {
            get => _workLanguage;
            set { _workLanguage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<LangItem> LanguagesForMain { get; set; }
        public ObservableCollection<LangItem> LanguagesForWork { get; set; }

        public LangItem CurrentMainLanguageFile
        {
            get => _currentMainLanguageFile;
            set
            {
                if (CurrentWorkLanguageFile == null)
                {
                    _currentMainLanguageFile = value;
                    FillLanguageFiles(LanguagesForWork, value);
                    NeedToFindMissing = false;
                    IsLoadingLanguageProcess = true;
                    MainLanguage = LoadLanguageFromFile(value, true);
                    IsLoadingLanguageProcess = false;
                    NeedToFindMissing = true;
                }
                else
                {
                    if (value.Name == CurrentWorkLanguageFile.Name &&
                        value.DisplayName == CurrentWorkLanguageFile.DisplayName)
                    {
                        _currentMainLanguageFile = null;
                        MainLanguage = null;
                        MissingAttributes.Clear();
                        MissingItems.Clear();
                        MissingItemsWithSpecialSymbols.Clear();
                    }
                    else
                    {
                        _currentMainLanguageFile = value;
                        NeedToFindMissing = false;
                        IsLoadingLanguageProcess = true;
                        MainLanguage = LoadLanguageFromFile(value, true);
                        CheckWorkLanguage();
                        FindMissingAttributes();
                        FindMissingItems();
                        FindMissingItemsWithSpecialSymbols();
                        IsLoadingLanguageProcess = false;
                        NeedToFindMissing = true;
                    }
                }
                OnPropertyChanged();
            }
        }

        public LangItem CurrentWorkLanguageFile
        {
            get => _currentWorkLanguageFile;
            set
            {
                _currentWorkLanguageFile = value;
                IsLoadingLanguageProcess = true;
                NeedToFindMissing = false;
                WorkLanguage = LoadLanguageFromFile(value, false);
                CheckWorkLanguage();
                FindMissingAttributes();
                FindMissingItems();
                FindMissingItemsWithSpecialSymbols();
                IsLoadingLanguageProcess = false;
                NeedToFindMissing = true;
                GetTranslationComplition();
                OnPropertyChanged();
            }
        }

        public double TranslationComplition
        {
            get => _translationComplition;
            set { _translationComplition = value; OnPropertyChanged(); }
        }

        public async void GetTranslationComplition()
        {
            if (IsLoadingLanguageProcess) return;

            Task<double> task = new Task<double>(() =>
            {
                List<bool> val = new List<bool>();
                for (var i = 0; i < MainLanguage.Nodes.Count; i++)
                {
                    NodeModel mainLanguageNode = MainLanguage.Nodes[i];
                    NodeModel workLanguageNode = WorkLanguage.Nodes[i];
                    for (var j = 0; j < mainLanguageNode.Attributes.Count; j++)
                    {
                        NodeAttributeModel mainAttributeModel = mainLanguageNode.Attributes[j];
                        NodeAttributeModel workAttributeModel = workLanguageNode.Attributes[j];
                        if (mainAttributeModel.Value == workAttributeModel.Value &&
                            mainAttributeModel.Value.ToLower() != "ok")
                            val.Add(false);
                        else if (string.IsNullOrEmpty(workAttributeModel.Value))
                            val.Add(false);
                        else val.Add(true);
                    }
                    for (var j = 0; j < mainLanguageNode.Items.Count; j++)
                    {
                        ItemModel mainItemModel = mainLanguageNode.Items[j];
                        ItemModel workItemModel = workLanguageNode.Items[j];
                        if (mainItemModel.Value == workItemModel.Value &&
                            mainItemModel.Value.ToLower() != "ok")
                            val.Add(false);
                        else if (string.IsNullOrEmpty(workItemModel.Value))
                            val.Add(false);
                        else val.Add(true);
                    }
                }
                var completeCount = val.Count(v => v);
                if (completeCount == val.Count)
                    return 100.00;
                else
                    return (double)completeCount / val.Count * 100.0;
            });
            task.Start();
            await task;

            TranslationComplition = task.Result;
        }
        /// <summary>Загрузка списка языков в указанную коллекцию</summary>
        /// <param name="languageFilesList"></param>
        /// <param name="exceptLang"></param>
        private void FillLanguageFiles(ObservableCollection<LangItem> languageFilesList, LangItem exceptLang = null)
        {
            languageFilesList.Clear();
            foreach (string file in Directory.GetFiles(_curDir, "*.xml", SearchOption.TopDirectoryOnly))
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var xDoc = XElement.Load(fileStream);
                    var langName = xDoc.Attribute("Name")?.Value;
                    if (!string.IsNullOrEmpty(langName))
                    {
                        if (exceptLang != null && exceptLang.Name == langName) continue;
                        var culture = _cultures.FirstOrDefault(x => x.Name.Equals(langName));
                        if (culture != null)
                        {
                            languageFilesList.Add(new LangItem(culture, file));
                        }
                    }
                }
            }
        }

        private LanguageModel LoadLanguageFromFile(LangItem langItem, bool isReadOnly)
        {
            try
            {
                var file = langItem.FileName;
                if (string.IsNullOrEmpty(file)) return null;
                if (!File.Exists(file)) return null;
                XElement xel = XElement.Load(file);
                LanguageModel language = new LanguageModel(file, xel.Attribute("Version")?.Value, xel.Attribute("Name")?.Value);

                foreach (XElement nodeXel in xel.Elements())
                {
                    NodeModel nodeModel = new NodeModel(nodeXel.Name.LocalName, language);
                    // get attributes
                    foreach (XAttribute attribute in nodeXel.Attributes())
                    {
                        NodeAttributeModel nodeAttributeModel =
                            new NodeAttributeModel(attribute.Name.LocalName, attribute.Value, nodeModel, this);
                        nodeAttributeModel.IsReadOnly = isReadOnly;
                        nodeModel.Attributes.Add(nodeAttributeModel);
                    }
                    // get items
                    foreach (XElement itemXel in nodeXel.Elements())
                    {
                        ItemModel item = new ItemModel(nodeModel, this);
                        item.Tag = itemXel.Name.LocalName;
                        item.Value = itemXel.Value;
                        item.IsReadOnly = isReadOnly;
                        nodeModel.Items.Add(item);
                    }
                    language.Nodes.Add(nodeModel);
                }
                return language;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.StackTrace);
                return null;
            }
        }
        /// <summary>Проверка рабочего языка на отсутствие нужных нодов</summary>
        private void CheckWorkLanguage()
        {
            try
            {
                for (var i = 0; i < MainLanguage.Nodes.Count; i++)
                {
                    NodeModel mainNode = MainLanguage.Nodes[i];
                    if (i <= WorkLanguage.Nodes.Count - 1)
                    {
                        NodeModel workNode = WorkLanguage.Nodes[i];
                        if (mainNode.NodeName != workNode.NodeName)
                        {
                            var newNode = mainNode.CreateEmptyCopy(WorkLanguage);
                            // create attributes and values
                            foreach (NodeAttributeModel mainAttr in mainNode.Attributes)
                                newNode.Attributes.Add(mainAttr.CreateEmptyCopy(newNode));
                            foreach (ItemModel mainItem in mainNode.Items)
                                newNode.Items.Add(mainItem.CreateEmptyCopy(newNode));
                            WorkLanguage.Nodes.Insert(i, newNode);
                        }
                        else
                        {
                            // check attributes
                            if (mainNode.Attributes.Count != workNode.Attributes.Count)
                                for (var j = 0; j < mainNode.Attributes.Count; j++)
                                {
                                    NodeAttributeModel mainAttributeModel = mainNode.Attributes[j];
                                    if (j <= workNode.Attributes.Count - 1)
                                    {
                                        if (mainAttributeModel.Name != workNode.Attributes[j].Name)
                                            workNode.Attributes.Insert(j, mainAttributeModel.CreateEmptyCopy(workNode));
                                    }
                                    else
                                        workNode.Attributes.Add(mainAttributeModel.CreateEmptyCopy(workNode));
                                }
                            // check items
                            if (mainNode.Items.Count != workNode.Items.Count)
                                for (var j = 0; j < mainNode.Items.Count; j++)
                                {
                                    ItemModel mainItemModel = mainNode.Items[j];
                                    if (j <= workNode.Items.Count - 1)
                                    {
                                        if (workNode.Items[j].Tag != mainItemModel.Tag)
                                            workNode.Items.Insert(j, mainItemModel.CreateEmptyCopy(workNode));
                                    }
                                    else
                                        workNode.Items.Add(mainItemModel.CreateEmptyCopy(workNode));
                                }
                        }
                    }
                    else
                    {
                        var newNode = mainNode.CreateEmptyCopy(WorkLanguage);
                        // create attributes and values
                        foreach (NodeAttributeModel mainAttr in mainNode.Attributes)
                            newNode.Attributes.Add(mainAttr.CreateEmptyCopy(newNode));
                        foreach (ItemModel mainItem in mainNode.Items)
                            newNode.Items.Add(mainItem.CreateEmptyCopy(newNode));
                        WorkLanguage.Nodes.Add(newNode);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        public ICommand CreateNewLanguageFileCommand { get; set; }
        private void CreateNewLanguageFile(object o)
        {
            if (!LanguagesForWork.Any()) return;
            NewLanguageFileSelector win = new NewLanguageFileSelector() { Owner = _mainWindow };
            win.LbLanguages.ItemsSource = _cultures;
            if (win.ShowDialog() == true)
            {
                if (win.LbLanguages.SelectedItem is CultureInfo selectedCulture)
                {
                    XElement newXDoc = new XElement("ModPlus");
                    newXDoc.SetAttributeValue("Version", "1.0.0.0");
                    newXDoc.SetAttributeValue("Name", selectedCulture.Name);
                    var newFile = Path.Combine(_curDir, selectedCulture.Name + ".xml");
                    newXDoc.Save(newFile);
                    if (File.Exists(newFile))
                    {
                        LanguagesForWork.Add(new LangItem(selectedCulture, newFile));
                        _mainWindow.CbWorkLanguages.SelectedIndex = _mainWindow.CbWorkLanguages.Items.Count - 1;
                    }
                }
            }
        }

        #region Missing values

        public bool NeedToFindMissing { get; set; }

        public ObservableCollection<MissingValue> MissingAttributes { get; set; }
        public ObservableCollection<MissingValue> MissingItems { get; set; }
        public ObservableCollection<MissingValue> MissingItemsWithSpecialSymbols { get; set; }

        public async void FindMissingAttributes()
        {
            Task<ObservableCollection<MissingValue>> task = new Task<ObservableCollection<MissingValue>>(() =>
            {
                try
                {
                    var collection = new ObservableCollection<MissingValue>();
                    for (var i = 0; i < MainLanguage.Nodes.Count; i++)
                    {
                        NodeModel mainLanguageNode = MainLanguage.Nodes[i];
                        NodeModel workLanguageNode = WorkLanguage.Nodes[i];
                        for (var j = 0; j < mainLanguageNode.Attributes.Count; j++)
                        {
                            NodeAttributeModel mainAttributeModel = mainLanguageNode.Attributes[j];
                            NodeAttributeModel workAttributeModel = workLanguageNode.Attributes[j];
                            if (mainAttributeModel.Value == workAttributeModel.Value &&
                                mainAttributeModel.Value.ToLower() != "ok")
                                collection.Add(new MissingValue(workLanguageNode.NodeName, workAttributeModel.Name));
                            else if (string.IsNullOrEmpty(workAttributeModel.Value))
                                collection.Add(new MissingValue(workLanguageNode.NodeName, workAttributeModel.Name));
                        }
                    }
                    return collection;
                }
                catch
                {
                    return null;
                }
            });
            task.Start();
            await task;
            if (task.Result != null)
            {
                MissingAttributes = task.Result;
                OnPropertyChanged(nameof(MissingAttributes));
            }
        }
        public async void FindMissingItems()
        {
            Task<ObservableCollection<MissingValue>> task = new Task<ObservableCollection<MissingValue>>(() =>
            {
                try
                {
                    var collection = new ObservableCollection<MissingValue>();
                    for (var i = 0; i < MainLanguage.Nodes.Count; i++)
                    {
                        NodeModel mainLanguageNode = MainLanguage.Nodes[i];
                        NodeModel workLanguageNode = WorkLanguage.Nodes[i];
                        for (var j = 0; j < mainLanguageNode.Items.Count; j++)
                        {
                            ItemModel mainItemModel = mainLanguageNode.Items[j];
                            ItemModel workItemModel = workLanguageNode.Items[j];
                            if (mainItemModel.Value == workItemModel.Value &&
                                mainItemModel.Value.ToLower() != "ok")
                                collection.Add(new MissingValue(workLanguageNode.NodeName, workItemModel.Tag));
                            else if (string.IsNullOrEmpty(workItemModel.Value))
                                collection.Add(new MissingValue(workLanguageNode.NodeName, workItemModel.Tag));
                        }
                    }
                    return collection;
                }
                catch
                {
                    return null;
                }
            });
            task.Start();
            await task;
            if (task.Result != null)
            {
                MissingItems = task.Result;
                OnPropertyChanged(nameof(MissingItems));
            }
        }
        public async void FindMissingItemsWithSpecialSymbols()
        {
            Task<ObservableCollection<MissingValue>> task = new Task<ObservableCollection<MissingValue>>(() =>
            {
                try
                {
                    var collection = new ObservableCollection<MissingValue>();
                    for (var i = 0; i < MainLanguage.Nodes.Count; i++)
                    {
                        NodeModel mainLanguageNode = MainLanguage.Nodes[i];
                        NodeModel workLanguageNode = WorkLanguage.Nodes[i];
                        for (var j = 0; j < mainLanguageNode.Items.Count; j++)
                        {
                            ItemModel mainItemModel = mainLanguageNode.Items[j];
                            ItemModel workItemModel = workLanguageNode.Items[j];
                            if (HasSymbol(mainItemModel.Value) && !HasSymbol(workItemModel.Value))
                                collection.Add(new MissingValue(workLanguageNode.NodeName, workItemModel.Tag));
                        }
                    }
                    return collection;
                }
                catch
                {
                    return null;
                }
            });
            task.Start();
            await task;
            if (task.Result != null)
            {
                MissingItemsWithSpecialSymbols = task.Result;
                OnPropertyChanged(nameof(MissingItemsWithSpecialSymbols));
            }
        }

        private static bool HasSymbol(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;
            var symbols = new List<string> { "\\n", "{0}", "{1}", "{2}", "{3}", "{4}", "{5}", "{6}", "{7}", "{8}" };
            foreach (string s in symbols)
            {
                if (str.Contains(s)) return true;
            }
            return false;
        }

        #endregion
    }
}
