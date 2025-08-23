using Avalonia;
using System;
using System.Text;
using Avalonia.ReactiveUI;

namespace TGBox;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // 设置控制台编码为UTF-8以正确显示中文
        Console.OutputEncoding = Encoding.UTF8;
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(); // 添加ReactiveUI支持，确保线程调度正确
}
