using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
    }
}
