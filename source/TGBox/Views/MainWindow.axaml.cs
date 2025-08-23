using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TGBox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new ViewModels.MainWindowViewModel();
    }
    
    // 最小化窗口
    private void MinimizeClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    // 最大化/还原窗口
    private void MaximizeClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
    
    // 关闭窗口
    private void CloseClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}