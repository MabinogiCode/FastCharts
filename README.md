# FastCharts

[![Build](https://github.com/<your-username>/FastCharts/actions/workflows/build.yml/badge.svg)](https://github.com/<your-username>/FastCharts/actions/workflows/build.yml)

FastCharts is an **open-source**, high-performance charting library for .NET/WPF.  
It is designed to efficiently render large datasets in real time, with smooth visuals and MVVM-friendly APIs.

## ✨ Features
- 📈 High-performance rendering (optimized for thousands to millions of points).
- 🎨 Seamless **WPF integration** with MVVM support.
- ⚡ Built-in **downsampling** for real-time data scenarios.
- 🛠️ Extensible: custom styles, themes, and behaviors.
- 🔍 Multi-target support: **.NET Framework 4.8**, **.NET 6**, **.NET 8**.

## 📦 Installation
*(To be updated once published on NuGet)*  
```powershell
dotnet add package FastCharts
```

## 🚀 Quick Example
```xml
<Window ...
        xmlns:fc="clr-namespace:FastCharts.Wpf;assembly=FastCharts.Wpf">
    <fc:FastLineChart ItemsSource="{Binding Data}" />
</Window>
```

```csharp
public ObservableCollection<Point> Data { get; } = new()
{
    new Point(0, 0),
    new Point(1, 10),
    new Point(2, 5),
};
```

## 📂 Repository Structure
- **src/** → Main library source code  
- **demos/** → WPF demo applications  
- **tests/** → Unit and integration tests  
- **.github/** → CI/CD workflows  

## 🛡️ License
This project is licensed under the **MIT License** – free to use and modify.

## 🤝 Contributing
Contributions are welcome!  
- Open an *issue* to report bugs or request features.  
- Submit a *pull request* to improve the library.  
