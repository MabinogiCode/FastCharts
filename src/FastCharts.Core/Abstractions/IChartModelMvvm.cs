using System;
using System.ComponentModel;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Interface pour les objets supportant la notification de changement de propriété
    /// Permet de découpler le domaine des frameworks UI spécifiques
    /// </summary>
    public interface INotifyPropertyChanged : System.ComponentModel.INotifyPropertyChanged
    {
    }

    /// <summary>
    /// Interface pour les objets supportant les commandes
    /// </summary>
    public interface ICommandSupport
    {
        void ExecuteCommand(string commandName, object? parameter = null);
        bool CanExecuteCommand(string commandName, object? parameter = null);
    }

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