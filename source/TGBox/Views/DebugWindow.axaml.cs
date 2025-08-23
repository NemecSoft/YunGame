using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive;
using System.Linq;
using System.Collections.ObjectModel;
using TGBox.Models;
using Avalonia.Controls.ApplicationLifetimes;
using System;

namespace TGBox.Views;

public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();
        DataContext = new DebugViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

public class DebugViewModel : ReactiveObject
{
    private readonly ViewModels.MainWindowViewModel _mainViewModel;

    public int GamesCount => _mainViewModel.Games.Count;
    
    public List<string> GamesList {
        get {
            return _mainViewModel.Games.Select(g => $"{g.Name} ({g.AlternativeName}) - {g.Genre}").ToList();
        }
    }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public DebugViewModel()
    {
        // 获取应用程序的MainWindowViewModel实例
        _mainViewModel = GetMainWindowViewModel();
        
        CloseCommand = ReactiveCommand.Create(() =>
        {
            // 关闭窗口的逻辑
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.OfType<DebugWindow>().FirstOrDefault();
                window?.Close();
            }
        });
        

    }

    private ViewModels.MainWindowViewModel GetMainWindowViewModel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && 
            desktop.MainWindow is MainWindow mainWindow && 
            mainWindow.DataContext is ViewModels.MainWindowViewModel viewModel)
        {
            return viewModel;
        }
        

        // 返回一个空的ViewModel作为备用
        return new ViewModels.MainWindowViewModel();
    }
}