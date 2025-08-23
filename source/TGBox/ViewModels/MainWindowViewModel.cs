using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Reactive;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using TGBox.Models;
using System.Diagnostics;
using System.ComponentModel;
using System.Reactive.Linq;
using System.IO;
using System.Reflection;
// 注释：Material.Avalonia包不使用Material.Styles.Themes命名空间
using TGBox.Views;

namespace TGBox.ViewModels;

/// <summary>
/// 游戏显示模式枚举
/// </summary>
public enum GameDisplayMode
{
    /// <summary>
    /// 卡片列表模式
    /// </summary>
    CardList,
    /// <summary>
    /// 网格模式
    /// </summary>
    Grid
}

/// <summary>
/// 主题类型枚举
/// </summary>
public enum ThemeType
{
    Light,
    Dark,
    Red,
    Blue
}

/// <summary>
/// 主题项类，用于下拉列表显示
/// </summary>
public class ThemeItem
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    /// 主题类型
    /// </summary>
    public ThemeType ThemeType { get; set; }
    
    /// <summary>
    /// 用于显示在UI中的文本
    /// </summary>
    public override string ToString()
    {
        return DisplayName;
    }
}

public class MainWindowViewModel : ViewModelBase
{
    private readonly GameDatabase _gameDatabase;
    
    /// <summary>
    /// 游戏列表
    /// </summary>
    public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();
    
    /// <summary>
    /// 当前选中的游戏
    /// </summary>
    [Reactive] public Game? SelectedGame { get; set; }
    
    /// <summary>
    /// 启动游戏命令
    /// </summary>
    public ReactiveCommand<Game, Unit> LaunchGameCommand { get; }
    
    /// <summary>
    /// 当前主题
    /// </summary>
    [Reactive] public ThemeType CurrentTheme { get; set; } = ThemeType.Light;
    

    
    /// <summary>
    /// 切换主题命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleThemeCommand { get; }
    
    /// <summary>
    /// 可用的主题列表，用于下拉选择
    /// </summary>
    public List<ThemeItem> ThemeItems { get; } = new List<ThemeItem>();
    
    /// <summary>
    /// 当前选中的主题项
    /// </summary>
    [Reactive] public ThemeItem? SelectedThemeItem { get; set; }
    
    /// <summary>
    /// 当前游戏显示模式
    /// </summary>
    [Reactive] public GameDisplayMode CurrentDisplayMode { get; set; } = GameDisplayMode.CardList;
    
    /// <summary>
    /// 当前应用模式
    /// </summary>
    [Reactive] public AppMode CurrentAppMode { get; set; } = AppMode.PlayMode;
    
    /// <summary>
    /// 管理模式可见性
    /// </summary>
    [Reactive] public bool IsManageModeVisible { get; set; } = false;
    
    /// <summary>
    /// 显示模式选项
    /// </summary>
    public List<GameDisplayMode> DisplayModeOptions { get; } = Enum.GetValues<GameDisplayMode>().ToList();
    
    /// <summary>
    /// 切换应用模式命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleAppModeCommand { get; }
    
    /// <summary>
    /// 添加游戏命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddGameCommand { get; }
    
    /// <summary>
    /// 导入游戏命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ImportGamesCommand { get; }
    
    /// <summary>
    /// 加载游戏命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadGamesCommand { get; }
    
    /// <summary>
    /// 显示调试窗口命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ShowDebugWindowCommand { get; }
    
    /// <summary>
    /// 调试视图模型，用于显示调试信息
    /// </summary>
    //public Views.DebugViewModel? DebugViewModel { get; set; }
    
    /// <summary>
    /// 编辑游戏命令
    /// </summary>
    public ReactiveCommand<Game, Unit> EditGameCommand { get; }
    
    /// <summary>
    /// 删除游戏命令
    /// </summary>
    public ReactiveCommand<Game, Unit> DeleteGameCommand { get; }
    
    /// <summary>
    /// 广告显示状态
    /// </summary>
    [Reactive] public bool IsAdVisible { get; set; } = true;
    
    /// <summary>
    /// 广告内容
    /// </summary>
    [Reactive] public string AdContent { get; set; } = "太极盒TGBox - 游戏管理利器";
    
    /// <summary>
    /// 广告链接
    /// </summary>
    [Reactive] public string AdLink { get; set; } = "https://example.com";
    
    /// <summary>
    /// 切换广告显示状态命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleAdVisibilityCommand { get; }
    
    /// <summary>
    /// 启动广告链接命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> LaunchAdCommand { get; }
    
    /// <summary>
    /// 显示游戏视频攻略命令
    /// </summary>
    public ReactiveCommand<Game, Unit> ShowVideoGuideCommand { get; }

    public MainWindowViewModel()
    {
        _gameDatabase = new GameDatabase();
        
        // 初始化命令
            LaunchGameCommand = ReactiveCommand.Create<Game>(LaunchGame);
            ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);
            ToggleAppModeCommand = ReactiveCommand.Create(ToggleAppMode);
            AddGameCommand = ReactiveCommand.Create(AddGame);
            ImportGamesCommand = ReactiveCommand.Create(ImportGames);
            LoadGamesCommand = ReactiveCommand.Create(LoadGames);
         //   ShowDebugWindowCommand = ReactiveCommand.Create(ShowDebugWindow);
            EditGameCommand = ReactiveCommand.Create<Game>(EditGame);
            DeleteGameCommand = ReactiveCommand.Create<Game>(DeleteGame);
            ToggleAdVisibilityCommand = ReactiveCommand.Create(ToggleAdVisibility);
            LaunchAdCommand = ReactiveCommand.Create(LaunchAd);
            ShowVideoGuideCommand = ReactiveCommand.Create<Game>(ShowVideoGuide);
            
            // 初始化主题列表
            ThemeItems.Add(new ThemeItem { DisplayName = "浅色主题", ThemeType = ThemeType.Light });
            ThemeItems.Add(new ThemeItem { DisplayName = "深色主题", ThemeType = ThemeType.Dark });
            ThemeItems.Add(new ThemeItem { DisplayName = "红色主题", ThemeType = ThemeType.Red });
            ThemeItems.Add(new ThemeItem { DisplayName = "蓝色主题", ThemeType = ThemeType.Blue });
            
            // 设置初始主题为深色
            CurrentTheme = ThemeType.Red;
            SelectedThemeItem = ThemeItems.FirstOrDefault(item => item.ThemeType == CurrentTheme);
            
            // 在UI线程上初始化主题资源
            Dispatcher.UIThread.Post(() =>
            {
                if (Application.Current != null)
                {
                    Console.WriteLine("开始初始化主题资源...");
                    // 不再需要单独初始化主题资源，ThemeVariant系统会自动处理
                    Console.WriteLine($"准备应用初始主题: {CurrentTheme}");
                    // 设置初始主题
                    ApplyTheme(CurrentTheme);
                    Console.WriteLine($"主题应用完成，当前主题: {CurrentTheme}");
                     
                    // 主题资源初始化完成后，再设置主题变化订阅，避免多次触发
                    this.WhenAnyValue(x => x.SelectedThemeItem)
                        .Where(item => item != null)
                        .Subscribe(item =>
                        {
                            if (item != null) // 额外的null检查，确保安全访问
                            {
                                SetTheme(item.ThemeType);
                            }
                        });
                }
                else
                {
                    Console.WriteLine("警告: Application.Current为空，无法初始化主题");
                }
            });
        
        LoadGames();
        AddSampleGames();
    }
    
    /// <summary>
    /// 根据选择的主题类型设置主题
    /// </summary>
    /// <param name="themeType">要设置的主题类型</param>
    private void SetTheme(ThemeType themeType)
    {
        try
        {
            if (CurrentTheme != themeType && Application.Current != null)
            {
                Console.WriteLine($"开始切换主题: {CurrentTheme} -> {themeType}");
                // 确保在UI线程上执行主题切换操作
                if (Dispatcher.UIThread.CheckAccess())
                {
                    // 使用ThemeVariant系统，不再需要手动移除资源字典
                    Console.WriteLine("准备应用新主题");
                    
                    ApplyTheme(themeType);
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        // 先移除所有自定义主题资源，防止资源冲突
                        // 使用ThemeVariant系统，不再需要手动移除资源字典
                        Console.WriteLine("准备应用新主题");
                        
                        ApplyTheme(themeType);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"设置主题失败: {ex.Message}");
        }
    }
    
        // 主题初始化方法已被删除，现在通过Avalonia的ThemeVariant系统统一管理主题资源
        // 主题资源定义位于/Themes/ThemeResources.axaml文件中
        
        /// <summary>
        /// 应用指定的主题
        /// </summary>
        /// <param name="theme">要应用的主题类型</param>
        private void ApplyTheme(ThemeType theme)
        {
            if (Application.Current == null)
                return;
            
            try
            {
                // 获取调用堆栈信息，以追踪谁调用了ApplyTheme
                var stackTrace = new System.Diagnostics.StackTrace();
                string callerMethod = stackTrace.FrameCount > 1 ? stackTrace.GetFrame(1).GetMethod().Name : "Unknown";
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 进入ApplyTheme方法，调用者: {callerMethod}，请求应用主题: {theme}");
                
                // 统一使用Avalonia的ThemeVariant机制
                switch (theme)
                {
                    case ThemeType.Light:
                        Console.WriteLine("应用浅色主题");
                        Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                        break;
                    case ThemeType.Dark:
                        Console.WriteLine("应用深色主题");
                        Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                        break;
                    case ThemeType.Red:
                        Console.WriteLine("应用红色主题");
                        Application.Current.RequestedThemeVariant = new ThemeVariant("Red", null);
                        break;
                    case ThemeType.Blue:
                        Console.WriteLine("应用蓝色主题");
                        Application.Current.RequestedThemeVariant = new ThemeVariant("Blue", null);
                        break;
                }
                
                Console.WriteLine($"主题应用成功: {theme}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"应用主题时发生错误: {ex.Message}");
            }
            
            CurrentTheme = theme;
            Console.WriteLine($"主题已应用: {theme}，当前Application主题: {Application.Current.RequestedThemeVariant}");
        }
    

    
    /// <summary>
    /// 加载游戏列表
    /// </summary>
    private void LoadGames()
    {
        var games = _gameDatabase.GetAllGames();
        Games.Clear();
        foreach (var game in games)
        {
            Games.Add(game);
        }
    }
    
    /// <summary>
    /// 添加示例游戏数据
    /// </summary>
    private void AddSampleGames()
    {
        if (Games.Count == 0)
        {
            var sampleGames = new List<Game>
            {
                new Game
                {
                    Name = "赛博朋克 2077",
                    Description = "一款开放世界角色扮演游戏，背景设定在未来科技高度发达的城市。",
                    Genre = "角色扮演",
                    ReleaseYear = 2020,
                    IsInstalled = true,
                    Platform = "PC"
                },
                new Game
                {
                    Name = "巫师 3: 狂猎",
                    Description = "一款获奖无数的开放世界角色扮演游戏，讲述猎魔人杰洛特的冒险故事。",
                    Genre = "角色扮演",
                    ReleaseYear = 2015,
                    PlayTime = 1200,
                    LastPlayed = DateTime.Now.AddDays(-5),
                    IsInstalled = true,
                    Platform = "PC"
                },
                new Game
                {
                    Name = "塞尔达传说: 旷野之息",
                    Description = "一款革命性的开放世界动作冒险游戏，探索广阔的海拉鲁大陆。",
                    Genre = "动作冒险",
                    ReleaseYear = 2017,
                    IsInstalled = false,
                    Platform = "Switch"
                },
                new Game
                {
                    Name = "只狼: 影逝二度",
                    Description = "一款以日本战国时代为背景的动作冒险游戏，以高难度和独特的战斗系统著称。",
                    Genre = "动作冒险",
                    ReleaseYear = 2019,
                    PlayTime = 800,
                    LastPlayed = DateTime.Now.AddDays(-2),
                    IsInstalled = true,
                    Platform = "PC"
                }
            };
            
            foreach (var game in sampleGames)
            {
                _gameDatabase.AddGame(game);
                Games.Add(game);
            }
        }
    }
    
    /// <summary>
    /// 启动游戏方法
    /// </summary>
    private void LaunchGame(Game game)
    {
        if (game != null && game.IsInstalled && !string.IsNullOrEmpty(game.Path))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = game.Path,
                    UseShellExecute = true
                });
                
                // 更新最后游玩时间
                game.LastPlayed = DateTime.Now;
                _gameDatabase.UpdateGame(game);
            }
            catch (Exception ex)
            {
                // 在实际应用中应该添加适当的错误处理和日志记录
                Console.WriteLine($"启动游戏失败: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 切换应用模式（Normal->Manage->Normal循环）
    /// </summary>
    private void ToggleAppMode()
    {
        CurrentAppMode = CurrentAppMode == AppMode.PlayMode ? AppMode.ManageMode : AppMode.PlayMode;
        IsManageModeVisible = CurrentAppMode == AppMode.ManageMode;
    }
    
    /// <summary>
    /// 添加游戏
    /// </summary>
    private void AddGame()
    {
        // 在实际应用中，这里应该打开添加游戏的窗口
        Console.WriteLine("添加游戏功能待实现");
    }
    
    /// <summary>
    /// 导入游戏
    /// </summary>
    private void ImportGames()
    {
        // 在实际应用中，这里应该打开文件选择器让用户选择游戏目录
        Console.WriteLine("导入游戏功能待实现");
    }
    
    /// <summary>
    /// 显示调试窗口
    /// </summary>
    //private void ShowDebugWindow()
    //{
    //    try
    //    {
    //        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    //        {
    //            // 确保DebugViewModel已初始化
    //            if (DebugViewModel == null)
    //            {
    //                DebugViewModel = new Views.DebugViewModel();
    //            }
                
    //            // 使用带参数的构造函数创建DebugWindow
    //            var debugWindow = new Views.DebugWindow(DebugViewModel);
    //            debugWindow.Show();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"显示调试窗口失败: {ex.Message}");
    //    }
    //}
    
    /// <summary>
    /// 编辑游戏
    /// </summary>
    /// <param name="game">要编辑的游戏</param>
    private void EditGame(Game game)
    {
        if (game == null)
            return;
            
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var editWindow = new GameEditWindow();
                editWindow.DataContext = game;
                editWindow.ShowDialog(desktop.MainWindow);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"编辑游戏失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 删除游戏
    /// </summary>
    /// <param name="game">要删除的游戏</param>
    private void DeleteGame(Game game)
    {
        if (game == null)
            return;
            
        try
        {
            _gameDatabase.DeleteGame(game.GameId);
            Games.Remove(game);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除游戏失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 切换应用程序主题（Light->Dark->Red->Light循环）
    /// </summary>
    private void ToggleTheme()
    {
        try
        {
            // 确保在UI线程上执行主题切换操作
            if (Dispatcher.UIThread.CheckAccess())
            {
                // 已经在UI线程上，直接执行
                ToggleThemeInternal();
            }
            else
            {
                // 不在UI线程上，使用Post方法异步在UI线程上执行
                Dispatcher.UIThread.Post(ToggleThemeInternal);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"主题切换异常: {ex.Message}");
            Console.WriteLine($"异常堆栈: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// 实际执行主题切换的内部方法，必须在UI线程上调用
    /// </summary>
    private void ToggleThemeInternal()
    {
        try
        {
            if (Application.Current != null)
            {
                // 循环切换主题：Light -> Dark -> Red -> Blue -> Light
                ThemeType nextTheme;
                switch (CurrentTheme)
                {
                    case ThemeType.Light:
                        nextTheme = ThemeType.Dark;
                        break;
                    case ThemeType.Dark:
                        nextTheme = ThemeType.Red;
                        break;
                    case ThemeType.Red:
                        nextTheme = ThemeType.Blue;
                        break;
                    default:
                        nextTheme = ThemeType.Light;
                        break;
                }
                
                // 应用下一个主题
                ApplyTheme(nextTheme);
                
                Console.WriteLine($"主题已切换为: {nextTheme}");
            }
            else
            {
                Console.WriteLine("警告: Application.Current为空，无法切换主题");
            }
        } catch (Exception ex)
            {
                Console.WriteLine($"主题切换内部方法异常: {ex.Message}");
                Console.WriteLine($"异常堆栈: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 切换广告显示状态
        /// </summary>
        private void ToggleAdVisibility()
        {
            IsAdVisible = !IsAdVisible;
            Console.WriteLine($"广告显示状态已切换为: {IsAdVisible}");
        }
        
        /// <summary>
        /// 启动广告链接
        /// </summary>
        private void LaunchAd()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.bilibili.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("启动广告链接失败: " + ex.Message);
            }
        }
        
        /// <summary>
        /// 显示游戏视频攻略
        /// </summary>
        /// <param name="game">游戏对象</param>
        private void ShowVideoGuide(Game game)
        {
            try
            {
                // 添加调试日志
                Debug.WriteLine($"[视频攻略] 尝试显示游戏 '{game.Name}' 的视频攻略");
                
                // 检查游戏名是否为空
                if (string.IsNullOrEmpty(game.Name))
                {
                    Debug.WriteLine("[视频攻略] 游戏名称为空，导航到B站搜索");
                    NavigateToBilibiliSearch(game.Name);
                    return;
                }
                
                // 获取游戏视频攻略目录（两种可能路径）
                var appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string logMessage = $"[视频攻略] APPDATA路径: {appDataRoot}";
                Debug.WriteLine(logMessage);
                //DebugViewModel?.AddDebugLog(logMessage);
                
                var pathsToCheck = new List<string>{
                    // 用户直接放在TGBox下的路径
                    Path.Combine(appDataRoot, "TGBox", game.Name),
                    // 系统预期的VideoGuides子目录路径
                    Path.Combine(appDataRoot, "TGBox", "VideoGuides", game.Name)
                };
                
                string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".wmv", ".mov", ".flv", ".webm", ".m4v" };
                List<string> allVideoFiles = new List<string>();
                
                // 检查所有可能的路径
                foreach (var path in pathsToCheck)
                {
                    string pathCheckMessage = $"[视频攻略] 检查路径: {path}";
                    Debug.WriteLine(pathCheckMessage);
                    //DebugViewModel?.AddDebugLog(pathCheckMessage);
                    if (System.IO.Directory.Exists(path))
                    {
                        string pathExistsMessage = $"[视频攻略] 路径存在: {path}";
                        Debug.WriteLine(pathExistsMessage);
                       // DebugViewModel?.AddDebugLog(pathExistsMessage);
                        var videoFiles = System.IO.Directory.GetFiles(path)
                            .Where(file => videoExtensions.Contains(System.IO.Path.GetExtension(file).ToLower()))
                            .ToList();
                        
                        string filesFoundMessage = $"[视频攻略] 在 {path} 中找到 {videoFiles.Count} 个视频文件";                        
                        Debug.WriteLine(filesFoundMessage);
                        //DebugViewModel?.AddDebugLog(filesFoundMessage);
                        
                        // 显示找到的每个视频文件的名称
                        if (videoFiles.Count > 0)
                        {
                            //DebugViewModel?.AddDebugLog("[视频攻略] 找到的文件列表:");
                            foreach (var file in videoFiles)
                            {
                             //   DebugViewModel?.AddDebugLog($"  - {System.IO.Path.GetFileName(file)}");
                            }
                        }
                        allVideoFiles.AddRange(videoFiles);
                    }
                    else
                    {
                        string pathNotExistsMessage = $"[视频攻略] 路径不存在: {path}";
                        Debug.WriteLine(pathNotExistsMessage);
                        //DebugViewModel?.AddDebugLog(pathNotExistsMessage);
                    }
                }
                
                // 如果任何路径下有视频文件
                if (allVideoFiles.Any())
                {
                    string totalFilesMessage = $"[视频攻略] 总共找到 {allVideoFiles.Count} 个视频文件，打开视频窗口";                    
                    Debug.WriteLine(totalFilesMessage);
                    //DebugViewModel?.AddDebugLog(totalFilesMessage);
                    // 打开视频攻略窗口显示本地视频
                    var videoGuideWindow = new VideoGuideWindow();
                    videoGuideWindow.GameName = game.Name;
                    //DebugViewModel?.AddDebugLog("[视频攻略] 打开视频攻略窗口");
                    videoGuideWindow.Show();
                    return;
                }
                else
                {
                    string noFilesMessage = "[视频攻略] 未找到任何视频文件，导航到B站搜索";
                    Debug.WriteLine(noFilesMessage);
                    //DebugViewModel?.AddDebugLog(noFilesMessage);
                }
                
                // 如果本地没有视频文件，则导航到bilibili进行搜索
                NavigateToBilibiliSearch(game.Name);
            }
            catch (Exception ex)
            {
                string errorMessage = "显示视频攻略失败: " + ex.Message;
                    Debug.WriteLine(errorMessage);
                    //DebugViewModel?.AddDebugLog(errorMessage);
                // 如果出错，也导航到bilibili
                NavigateToBilibiliSearch(game.Name);
            }
        }
        
        /// <summary>
        /// 导航到bilibili搜索页面
        /// </summary>
        /// <param name="gameName">游戏名称</param>
        private void NavigateToBilibiliSearch(string gameName)
        {
            try
            {
                // 构建搜索URL
                string searchUrl = $"https://search.bilibili.com/all?keyword={Uri.EscapeDataString(gameName + " 攻略")}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("导航到bilibili搜索失败: " + ex.Message);
            }
        }
        
        /// <summary>
        /// 设置广告内容
        /// </summary>
        /// <param name="content">广告内容</param>
        /// <param name="link">广告链接</param>
        public void SetAdContent(string content, string link = "")
        {
            AdContent = content;
            if (!string.IsNullOrEmpty(link))
            {
                AdLink = link;
            }
        }
    

}
