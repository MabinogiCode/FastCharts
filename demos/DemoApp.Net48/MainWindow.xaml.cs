using System.Windows;

using DemoApp.Net48.ViewModels;

namespace DemoApp.Net48
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}

