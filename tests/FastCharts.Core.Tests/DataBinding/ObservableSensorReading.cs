using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FastCharts.Core.Tests.DataBinding
{
    /// <summary>
    /// Observable test data class for sensor readings
    /// </summary>
    public class ObservableSensorReading : INotifyPropertyChanged
    {
        private DateTime _time;
        private double _temperature;

        public DateTime Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }

        public double Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}