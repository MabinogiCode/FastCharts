using System.Windows;
using DemoApp.Net8.ViewModels;

namespace DemoApp
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            DataContext = new MainViewModel();
        }
    }
}
