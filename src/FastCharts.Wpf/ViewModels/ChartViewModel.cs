using System;
using System.ComponentModel;
using System.Windows.Input;
using ReactiveUI;
using FastCharts.Core;
using FastCharts.Core.Services;

namespace FastCharts.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel pour le contrôle FastChart, séparant la logique métier de la présentation
    /// </summary>
    public class ChartViewModel : ReactiveObject
    {
        private readonly IBehaviorManager _behaviorManager;
        private ChartModel _model;
        private bool _isInitialized;

        public ChartViewModel(ChartModel model) : this(model, new BehaviorManager())
        {
        }

        public ChartViewModel(ChartModel model, IBehaviorManager behaviorManager)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _behaviorManager = behaviorManager ?? throw new ArgumentNullException(nameof(behaviorManager));
            InitializeCommands();
        }

        public ChartModel Model
        {
            get => _model;
            set => this.RaiseAndSetIfChanged(ref _model, value);
        }

        public bool IsInitialized
        {
            get => _isInitialized;
            private set => this.RaiseAndSetIfChanged(ref _isInitialized, value);
        }

        public ICommand InitializeCommand { get; private set; } = null!;
        public ICommand AutoFitCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            InitializeCommand = ReactiveCommand.Create(Initialize);
            AutoFitCommand = ReactiveCommand.Create(() => Model.AutoFitDataRange());
        }

        private void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            // Configuration des comportements par défaut via le service
            _behaviorManager.ConfigureDefaultBehaviors(Model.Behaviors);

            Model.AutoFitDataRange();
            IsInitialized = true;
        }
    }
}