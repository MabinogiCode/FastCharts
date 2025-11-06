using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Threading;
using ReactiveUI;

namespace DemoApp.Net8;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // ? CRITICAL: Initialize ReactiveUI with proper WPF scheduler
        // This ensures RxApp.MainThreadScheduler uses the WPF Dispatcher
        RxApp.MainThreadScheduler = new DispatcherScheduler(Dispatcher.CurrentDispatcher);

        base.OnStartup(e);
    }
}
