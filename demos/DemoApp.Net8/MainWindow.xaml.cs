using System.Windows;

using DemoApp.Net8.ViewModels;

namespace DemoApp.Net8;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainViewModel();
    }
}

