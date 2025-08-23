using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using TGBox.ViewModels;
using TGBox.Views;
using ReactiveUI;


namespace TGBox;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
            {
                try
                {
                    // 记录应用程序启动异常信息到控制台
                    // 这是应用程序无法正常初始化时的最后错误报告机制
                    Console.WriteLine("应用程序启动异常: " + ex.Message);
                    Console.WriteLine("异常堆栈: " + ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("内部异常: " + ex.InnerException.Message);
                        Console.WriteLine("内部异常堆栈: " + ex.InnerException.StackTrace);
                    }
                }
                catch { }
                
                throw;
            }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}