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
using Avalonia.Threading;

namespace TGBox.Views;

public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();
        // 不再自动创建ViewModel，而是从外部传入
    }
    
    // 带参数的构造函数，接受外部传入的ViewModel
    public DebugWindow(DebugViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

public class DebugViewModel : ReactiveObject
{
    private readonly ViewModels.MainWindowViewModel _mainViewModel;
    
    // 存储调试日志的集合
    private ObservableCollection<string> _debugLogs = new ObservableCollection<string>();
    public ObservableCollection<string> DebugLogs => _debugLogs;

    public int GamesCount => _mainViewModel.Games.Count;
    
    public List<string> GamesList {
        get {
            return _mainViewModel.Games.Select(g => $"{g.Name} ({g.AlternativeName}) - {g.Genre}").ToList();
        }
    }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearLogsCommand { get; }

    public DebugViewModel()
    {
        // 获取应用程序的MainWindowViewModel实例
        _mainViewModel = GetMainWindowViewModel();
        
        // 移除可能导致循环引用的代码
        // 不再将当前ViewModel实例存储到MainWindowViewModel中
        
        CloseCommand = ReactiveCommand.Create(() =>
        {
            // 关闭窗口的逻辑
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.OfType<DebugWindow>().FirstOrDefault();
                window?.Close();
            }
        });
        
        ClearLogsCommand = ReactiveCommand.Create(() =>
        {
            ClearDebugLogs();
        });
    }
    
    // 添加调试日志的方法
    public void AddDebugLog(string message)
    {
        // 确保在UI线程上执行
        Dispatcher.UIThread.Post(() =>
        {
            _debugLogs.Add($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            // 如果日志过多，保留最新的200条
            while (_debugLogs.Count > 200)
            {
                _debugLogs.RemoveAt(0);
            }
        });
    }
    
    // 清除所有调试日志
    public void ClearDebugLogs()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _debugLogs.Clear();
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