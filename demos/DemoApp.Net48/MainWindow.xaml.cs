using System.Windows;

using DemoApp.Net48.ViewModels;

namespace DemoApp
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
          DataContext = new MainViewModel();
          ;
        }
    }
}
