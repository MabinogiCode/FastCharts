using System.Windows;
using ReactiveUI;

namespace DemoApp
{
    public partial class App : Application
    {
        public App()
        {
            // Restrict ReactiveUI platform probing to WPF: prevents Assembly.Load attempts
            // for ReactiveUI.XamForms & co at startup. ReactiveUI.WPF (referenced through
            // FastCharts.Wpf) wires RxApp.MainThreadScheduler to the WPF Dispatcher.
            PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Wpf);
        }
    }
}
