using System.Windows;

namespace ModPlusLanguageCreator.Helpers
{
    public partial class NewLanguageFileSelector 
    {
        public NewLanguageFileSelector()
        {
            InitializeComponent();
        }

        private void Accept_OnClick(object sender, RoutedEventArgs e)
        {
            if (LbLanguages.SelectedIndex == -1)
            {
                MessageBox.Show("You did not select anything!");
                return;
            }
            DialogResult = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
