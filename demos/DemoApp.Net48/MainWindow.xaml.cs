using System.Windows;
using System.Windows.Threading;

using DemoApp.Net48.ViewModels;

namespace DemoApp.Net48
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
        }
    }
}

