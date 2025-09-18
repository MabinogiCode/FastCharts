
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DynamicData;
using FastCharts.Wpf.Contracts;

namespace DemoApp
{
    public partial class MainWindow : Window
    {
        public System.Collections.ObjectModel.ReadOnlyObservableCollection<LineSeries> Series { get; }
        private readonly SourceList<LineSeries> _source = new();

        public MainWindow()
        {
            InitializeComponent();
            _source.Connect().Bind(out var series).Subscribe();
            Series = series;
            int n=100_000; var rand=new Random();
            var xs=Enumerable.Range(0,n).Select(i=>(double)i/100).ToArray();
            var sine=xs.Select(Math.Sin).ToArray();
            var noise=xs.Select(_=>rand.NextDouble()*2-1).ToArray();
            _source.Add(new LineSeries(xs,sine,"Sine"){ Stroke=Brushes.SteelBlue, StrokeThickness=1.0, DrawLines=true, ShowPoints=false });
            _source.Add(new LineSeries(xs,noise,"Noise"){ Stroke=Brushes.OrangeRed, StrokeThickness=1.0, DrawLines=true, ShowPoints=false });
            DataContext=this;
        }
    }
}
