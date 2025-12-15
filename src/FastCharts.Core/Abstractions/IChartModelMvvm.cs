namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Interface for chart model extended with MVVM support
    /// </summary>
    public interface IChartModelMvvm : IChartModel, INotifyPropertyChanged, ICommandSupport
    {
        /// <summary>
        /// Gets or sets the chart title
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the chart theme
        /// </summary>
        ITheme Theme { get; set; }

        /// <summary>
        /// Gets a value indicating whether the chart model has been initialized
        /// </summary>
        bool IsInitialized { get; }
    }
}