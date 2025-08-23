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
    Red
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
    [Reactive] public Game SelectedGame { get; set; }
    
    /// <summary>
    /// 启动游戏命令
    /// </summary>
    public ReactiveCommand<Game, Unit> LaunchGameCommand { get; }
    
    /// <summary>
    /// 当前主题
    /// </summary>
    [Reactive] public ThemeType CurrentTheme { get; set; } = ThemeType.Light;
    
    /// <summary>
    /// 主题资源字典
    /// </summary>
    private ResourceDictionary _redThemeResources;
    private ResourceDictionary _darkThemeResources;
    
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
    [Reactive] public ThemeItem SelectedThemeItem { get; set; }
    
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
            ShowDebugWindowCommand = ReactiveCommand.Create(ShowDebugWindow);
            EditGameCommand = ReactiveCommand.Create<Game>(EditGame);
            DeleteGameCommand = ReactiveCommand.Create<Game>(DeleteGame);
            ToggleAdVisibilityCommand = ReactiveCommand.Create(ToggleAdVisibility);
            LaunchAdCommand = ReactiveCommand.Create(LaunchAd);
            
            // 初始化主题列表
            ThemeItems.Add(new ThemeItem { DisplayName = "浅色主题", ThemeType = ThemeType.Light });
            ThemeItems.Add(new ThemeItem { DisplayName = "深色主题", ThemeType = ThemeType.Dark });
            ThemeItems.Add(new ThemeItem { DisplayName = "红色主题", ThemeType = ThemeType.Red });
            
            // 初始化为Light主题
            CurrentTheme = ThemeType.Light;
            SelectedThemeItem = ThemeItems.FirstOrDefault(item => item.ThemeType == CurrentTheme);
            
            // 订阅SelectedThemeItem变化事件
            this.WhenAnyValue(x => x.SelectedThemeItem)
                .Where(item => item != null)
                .Subscribe(item => SetTheme(item.ThemeType));
            
            // 在UI线程上初始化主题
            Dispatcher.UIThread.Post(() =>
            {
                if (Application.Current != null)
                {
                    // 初始化红色主题资源
                    InitializeRedTheme();
                    // 初始化深色主题资源
                    InitializeDarkTheme();
                    
                    // 设置初始主题
                    ApplyTheme(ThemeType.Light);
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
            if (CurrentTheme != themeType)
            {
                // 确保在UI线程上执行主题切换操作
                if (Dispatcher.UIThread.CheckAccess())
                {
                    // 先移除所有自定义主题资源，防止资源冲突
                    if (_redThemeResources != null && Application.Current != null)
                    {
                        Application.Current.Resources.MergedDictionaries.Remove(_redThemeResources);
                    }
                    if (_darkThemeResources != null && Application.Current != null)
                    {
                        Application.Current.Resources.MergedDictionaries.Remove(_darkThemeResources);
                    }
                    
                    ApplyTheme(themeType);
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        // 先移除所有自定义主题资源，防止资源冲突
                        if (_redThemeResources != null && Application.Current != null)
                        {
                            Application.Current.Resources.MergedDictionaries.Remove(_redThemeResources);
                        }
                        if (_darkThemeResources != null && Application.Current != null)
                        {
                            Application.Current.Resources.MergedDictionaries.Remove(_darkThemeResources);
                        }
                        
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
    
    /// <summary>
    /// 初始化红色主题资源，参考 Constants.xaml 中的红色主题配色
    /// </summary>
        private void InitializeRedTheme()
        {
            if (Application.Current == null)
                return;

            // 如果已经存在资源字典，则先清理
            if (_redThemeResources != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(_redThemeResources);
            }
            
            _redThemeResources = new ResourceDictionary();
        
        try
        {
            // 创建红色主题资源字典
            _redThemeResources = new ResourceDictionary();
            
            // 参考 Constants.xaml 中的红色主题配色
            _redThemeResources.Add("MainColor", Color.Parse("#545B67"));
            _redThemeResources.Add("MainColorDark", Color.Parse("#700010"));
            _redThemeResources.Add("HoverColor", Color.Parse("#9a4545"));
            _redThemeResources.Add("GlyphColor", Color.Parse("#F4A460"));
            _redThemeResources.Add("HighlightGlyphColor", Color.Parse("#c08080"));
            _redThemeResources.Add("PopupBackgroundColor", Color.Parse("#171e26"));
            _redThemeResources.Add("PopupBorderColor", Color.Parse("#FFAF612E"));
            _redThemeResources.Add("BackgroundToneColor", Color.Parse("#2C3A67"));
            _redThemeResources.Add("GridItemBackgroundColor", Color.Parse("#609a4545"));
            _redThemeResources.Add("WindowPanelSeparatorColor", Color.Parse("#900030"));
            
            // 添加文本颜色资源
            _redThemeResources.Add("TextBrush", new SolidColorBrush(Color.Parse("#FFFFFF")));
            
            // 定义线性渐变刷
            var windowBackgroundBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.6, 0.8, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = {
                    new GradientStop(Color.Parse("#303030"), 0),
                    new GradientStop(Color.Parse("#800000"), 1.5)
                }
            };
            
            _redThemeResources.Add("WindowBackgourndBrush", windowBackgroundBrush);
            
            // 按钮背景色
            _redThemeResources.Add("ButtonBackgroundBrush", new SolidColorBrush(Color.Parse("#9a4545")));
            
            Console.WriteLine("红色主题资源初始化完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"红色主题资源初始化失败: {ex.Message}");
        }
    }
        
        /// <summary>
        /// 初始化深色主题资源
        /// </summary>
        private void InitializeDarkTheme()
        {
            if (Application.Current == null)
                return;

            // 如果已经存在资源字典，则先清理
            if (_darkThemeResources != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(_darkThemeResources);
            }
            
            _darkThemeResources = new ResourceDictionary();
            
            // 定义深色主题配色
            _darkThemeResources.Add("MainColor", Color.Parse("#333333"));
            _darkThemeResources.Add("MainColorDark", Color.Parse("#1A1A1A"));
            _darkThemeResources.Add("HoverColor", Color.Parse("#444444"));
            _darkThemeResources.Add("GlyphColor", Color.Parse("#CCCCCC"));
            _darkThemeResources.Add("HighlightGlyphColor", Color.Parse("#FFFFFF"));
            _darkThemeResources.Add("PopupBackgroundColor", Color.Parse("#2A2A2A"));
            _darkThemeResources.Add("PopupBorderColor", Color.Parse("#444444"));
            _darkThemeResources.Add("BackgroundToneColor", Color.Parse("#222222"));
            _darkThemeResources.Add("GridItemBackgroundColor", Color.Parse("#60444444"));
            _darkThemeResources.Add("WindowPanelSeparatorColor", Color.Parse("#444444"));
            
            // 添加文本颜色资源
            _darkThemeResources.Add("TextBrush", new SolidColorBrush(Color.Parse("#CCCCCC")));
            
            // 定义纯色背景刷（黑色）
            var windowBackgroundBrush = new SolidColorBrush(Color.Parse("#1A1A1A"));
            _darkThemeResources.Add("WindowBackgroundBrush", windowBackgroundBrush);
        }
        
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
            // 移除之前添加的红色主题资源（如果存在）
            if (_redThemeResources != null && Application.Current.Resources.MergedDictionaries.Contains(_redThemeResources))
            {
                Application.Current.Resources.MergedDictionaries.Remove(_redThemeResources);
            }
            
            switch (theme)
            {
                case ThemeType.Light:
                    Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                    break;
                case ThemeType.Dark:
                    Application.Current.RequestedThemeVariant = ThemeVariant.Light; // 使用Light作为基础，然后覆盖自定义深色资源
                    // 添加深色主题资源
                    if (_darkThemeResources != null)
                    {
                        Application.Current.Resources.MergedDictionaries.Add(_darkThemeResources);
                    }
                    break;
                case ThemeType.Red:
                    Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                    // 添加红色主题资源
                    if (_redThemeResources != null)
                    {
                        Application.Current.Resources.MergedDictionaries.Add(_redThemeResources);
                    }
                    break;
            }
            
            CurrentTheme = theme;
            Console.WriteLine($"主题已应用: {theme}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"应用主题失败: {ex.Message}");
        }
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
    private void ShowDebugWindow()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var debugWindow = new DebugWindow();
                debugWindow.Show();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"显示调试窗口失败: {ex.Message}");
        }
    }
    
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
                // 循环切换主题：Light -> Dark -> Red -> Light
                ThemeType nextTheme;
                switch (CurrentTheme)
                {
                    case ThemeType.Light:
                        nextTheme = ThemeType.Dark;
                        break;
                    case ThemeType.Dark:
                        nextTheme = ThemeType.Red;
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
                if (!string.IsNullOrEmpty(AdLink))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = AdLink,
                        UseShellExecute = true
                    });
                    Console.WriteLine($"已打开广告链接: {AdLink}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开广告链接失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 设置广告内容
        /// </summary>
        /// <param name="content">广告内容</param>
        /// <param name="link">广告链接</param>
        public void SetAdContent(string content, string link = null)
        {
            AdContent = content;
            if (!string.IsNullOrEmpty(link))
            {
                AdLink = link;
            }
        }
    

}
