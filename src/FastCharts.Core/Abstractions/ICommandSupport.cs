namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Interface for objects supporting commands
    /// </summary>
    public interface ICommandSupport
    {
        void ExecuteCommand(string commandName, object? parameter = null);
        bool CanExecuteCommand(string commandName, object? parameter = null);
    }
}