using System.ComponentModel;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Interface for objects supporting property change notification
    /// Allows decoupling domain from specific UI frameworks
    /// </summary>
    public interface INotifyPropertyChanged : System.ComponentModel.INotifyPropertyChanged
    {
    }
}