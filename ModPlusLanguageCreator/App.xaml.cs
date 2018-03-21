using System.IO;
using System.Reflection;
using System.Windows;

namespace ModPlusLanguageCreator
{
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var curDir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            if (curDir != null)
            {
                var mainLangFile = Path.Combine(curDir, "ru-RU.xml");
                if (!File.Exists(mainLangFile))
                {
                    MessageBox.Show("The application must be located in the folder /ModPlus/Languages/ where the ru-RU file should be located");
                    Application.Current.Shutdown();
                }

                MainWindow mainWindow = new MainWindow();
                MainViewModel viewModel = new MainViewModel(curDir, mainWindow);
                mainWindow.DataContext = viewModel;
                mainWindow.Show();
            }
            else Current.Shutdown();
        }
    }
}
