using System;
using System.ComponentModel;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Interface pour le modèle de chart étendu avec support MVVM
    /// </summary>
    public interface IChartModelMvvm : IChartModel, INotifyPropertyChanged, ICommandSupport
    {
        string Title { get; set; }
        ITheme Theme { get; set; }
        bool IsInitialized { get; }
    }
}