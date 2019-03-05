using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ModPlusLanguageCreator.Models;

namespace ModPlusLanguageCreator
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TbAttribute_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox tb)) return;

            BindingExpression bindingExpression = tb.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression?.DataItem is NodeAttributeModel selectedAttribute)
            {
                // select attribute in listbox
                ListBoxItem atrLbi = FindParent<ListBoxItem>(tb);
                if (atrLbi != null) atrLbi.IsSelected = true;
                if (TvMainLanguage.ItemsSource is ObservableCollection<NodeModel> mainLangNodes)
                {
                    foreach (NodeModel mainLangNode in mainLangNodes)
                    {
                        if (mainLangNode.NodeName == selectedAttribute.OwnerNodeModel.NodeName)
                        {
                            mainLangNode.IsExpanded = true;

                            if (TvMainLanguage.ItemContainerGenerator.ContainerFromItem(mainLangNode) is TreeViewItem nodeTvi)
                            {
                                nodeTvi.UpdateLayout();
                                foreach (TextBlock textBlock in FindVisualChildren<TextBlock>(nodeTvi))
                                {
                                    if (textBlock.Text == selectedAttribute.Name)
                                    {
                                        var lbi = FindParent<ListBoxItem>(textBlock);
                                        if (lbi != null)
                                        {
                                            lbi.IsSelected = true;
                                            lbi.BringIntoView();
                                        }
                                        else textBlock.BringIntoView();
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void TbItemValue_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox tb)) return;
            BindingExpression bindingExpression = tb.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression?.DataItem is ItemModel selectedItem)
            {
                // select node
                if (TvWorkLanguage.ItemContainerGenerator.ContainerFromItem(selectedItem.OwnerNodeModel) is TreeViewItem selectedNodeTvi)
                {
                    if (selectedNodeTvi.ItemContainerGenerator.ContainerFromIndex(
                            selectedItem.OwnerNodeModel.Items.IndexOf(selectedItem))
                        is TreeViewItem selectedItemTvi)
                    {
                        selectedItemTvi.IsSelected = true;
                    }
                }
                if (TvMainLanguage.ItemsSource is ObservableCollection<NodeModel> mainLangNodes)
                {
                    var stopSearch = false;
                    foreach (NodeModel mainLangNode in mainLangNodes)
                    {
                        if (stopSearch) break;
                        if (mainLangNode.NodeName != selectedItem.OwnerNodeModel.NodeName) continue;
                        foreach (ItemModel itemModel in mainLangNode.Items)
                        {
                            if (itemModel.Tag == selectedItem.Tag)
                            {
                                // check uppercase
                                if (IsAllUpper(itemModel.Value))
                                    selectedItem.MustBeUppercase = true;

                                if (TvMainLanguage.ItemContainerGenerator.ContainerFromItem(mainLangNode) is
                                    TreeViewItem nodeTvi)
                                {
                                    nodeTvi.IsExpanded = true;
                                    nodeTvi.UpdateLayout();
                                    if (nodeTvi.ItemContainerGenerator.ContainerFromIndex(mainLangNode.Items.IndexOf(itemModel)) is TreeViewItem
                                        itemTvi)
                                    {
                                        itemTvi.BringIntoView();
                                        itemTvi.IsSelected = true;
                                    }
                                }

                                stopSearch = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
        bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void BtSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && viewModel.WorkLanguage != null)
            {
                viewModel.WorkLanguage.SaveToFile();
                MessageBox.Show("Saved!");
            }
        }

        private void BtGoToFirstMissingAttribute_OnClick(object sender, RoutedEventArgs e)
        {
            GoToFirstMissingAttribute();
        }

        private void GoToFirstMissingAttribute()
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                var missingValue = mainViewModel.MissingAttributes?.FirstOrDefault();
                if (missingValue != null && TvWorkLanguage.ItemsSource is ObservableCollection<NodeModel> workNodes)
                {
                    foreach (NodeModel nodeModel in workNodes)
                    {
                        if (nodeModel.NodeName == missingValue.NodeName)
                        {
                            if (TvWorkLanguage.ItemContainerGenerator.ContainerFromItem(nodeModel) is TreeViewItem nodeTvi)
                            {
                                nodeModel.IsExpanded = true;
                                TvWorkLanguage.UpdateLayout();
                                foreach (TextBlock textBlock in FindVisualChildren<TextBlock>(nodeTvi))
                                {
                                    if (textBlock.Text == missingValue.Name)
                                    {
                                        var lbi = FindParent<ListBoxItem>(textBlock);
                                        if (lbi != null)
                                        {
                                            FindVisualChildren<TextBox>(lbi).FirstOrDefault()?.Focus();
                                        }
                                        else textBlock.BringIntoView();
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void BtGoToFirstMissingItem_OnClick(object sender, RoutedEventArgs e)
        {
            GoToFirstMissingItem();
        }

        private void GoToFirstMissingItem()
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                var missingValue = mainViewModel.MissingItems?.FirstOrDefault();
                if (missingValue != null && TvWorkLanguage.ItemsSource is ObservableCollection<NodeModel> workNodes)
                {
                    foreach (NodeModel nodeModel in workNodes)
                    {
                        if (nodeModel.NodeName == missingValue.NodeName)
                        {
                            if (TvWorkLanguage.ItemContainerGenerator.ContainerFromItem(nodeModel) is TreeViewItem nodeTvi)
                            {
                                nodeTvi.IsExpanded = true;
                                TvWorkLanguage.UpdateLayout();
                                foreach (TextBlock textBlock in FindVisualChildren<TextBlock>(nodeTvi))
                                {
                                    if (textBlock.Text == missingValue.Name)
                                    {
                                        var lbi = FindParent<TreeViewItem>(textBlock);
                                        if (lbi != null)
                                        {
                                            FindVisualChildren<TextBox>(lbi).FirstOrDefault()?.Focus();
                                        }
                                        else textBlock.BringIntoView();
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void BtGoToFirstItemWithSpecialSymbol_OnClick(object sender, RoutedEventArgs e)
        {
            GoToFirstItemWithSpecialSymbol();
        }

        private void GoToFirstItemWithSpecialSymbol()
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                var missingValue = mainViewModel.MissingItemsWithSpecialSymbols?.FirstOrDefault();
                if (missingValue != null && TvWorkLanguage.ItemsSource is ObservableCollection<NodeModel> workNodes)
                {
                    foreach (NodeModel nodeModel in workNodes)
                    {
                        if (nodeModel.NodeName == missingValue.NodeName)
                        {
                            if (TvWorkLanguage.ItemContainerGenerator.ContainerFromItem(nodeModel) is TreeViewItem nodeTvi)
                            {
                                nodeTvi.IsExpanded = true;
                                TvWorkLanguage.UpdateLayout();
                                foreach (TextBlock textBlock in FindVisualChildren<TextBlock>(nodeTvi))
                                {
                                    if (textBlock.Text == missingValue.Name)
                                    {
                                        var lbi = FindParent<TreeViewItem>(textBlock);
                                        if (lbi != null)
                                        {
                                            FindVisualChildren<TextBox>(lbi).FirstOrDefault()?.Focus();
                                        }
                                        else textBlock.BringIntoView();
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://modplus.org/");
        }

        private void TbAttribute_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Enum.TryParse(Properties.Settings.Default.HotKeys_Translate, out Key k))
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == k && sender is TextBox tb)
            {
                var lbi = FindParent<ListBoxItem>(tb);
                if (lbi != null && lbi.Content is NodeAttributeModel nam)
                {
                    nam.Translate(true);
                }
            }
            else HotKeysOfWin(e);
        }

        private void TbItemValue_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(Enum.TryParse(Properties.Settings.Default.HotKeys_Translate, out Key k))
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == k && sender is TextBox tb)
            {
                var tvi = FindParent<TreeViewItem>(tb);
                if (tvi != null)
                {
                    var b = tvi.DataContext as ItemModel;
                    b?.Translate(true);
                }
            }
            else HotKeysOfWin(e);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            HotKeysOfWin(e);
        }

        private void HotKeysOfWin(KeyEventArgs e)
        {
            Enum.TryParse(Properties.Settings.Default.HotKeys_GoToAttribute, out Key attrKey);
            Enum.TryParse(Properties.Settings.Default.HotKeys_GoToItem, out Key itemKey);
            Enum.TryParse(Properties.Settings.Default.HotKeys_GoToSymbol, out Key symbKey);
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == attrKey)
            {
                GoToFirstMissingAttribute();
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == itemKey)
            {
                GoToFirstMissingItem();
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == symbKey)
            {
                GoToFirstItemWithSpecialSymbol();
            }
        }

        private void HotKeysComboBoxes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> hotKeys = new List<string>
            {
                Properties.Settings.Default.HotKeys_GoToItem,
                Properties.Settings.Default.HotKeys_Translate,
                Properties.Settings.Default.HotKeys_GoToAttribute,
                Properties.Settings.Default.HotKeys_GoToSymbol
            };
            if (hotKeys.GroupBy(n => n).Any(g => g.Count() > 1))
                MessageBox.Show("You specified the same hotkeys! Please specify other values");
        }

        private void TextBox_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox tb)
                tb.SelectAll();
        }

        private void TextBox_OnGotMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender is TextBox tb)
                tb.SelectAll();
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }
    }
}
