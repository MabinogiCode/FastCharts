using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Threading;
using ReactiveUI;

namespace DemoApp
{
    public partial class App : Application
    {
        public App()
        {
            // ? CRITICAL: Initialize ReactiveUI with proper WPF scheduler
            // This ensures RxApp.MainThreadScheduler uses the WPF Dispatcher
            RxApp.MainThreadScheduler = new DispatcherScheduler(Dispatcher.CurrentDispatcher);
        }
    }
}
