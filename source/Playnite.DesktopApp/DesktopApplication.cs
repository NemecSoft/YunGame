using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using Playnite.API;
using Playnite.Common;
using Playnite.Controllers;
using Playnite.Controls;
using Playnite.Database;
using Playnite.DesktopApp.API;
using Playnite.DesktopApp.Controls;
using Playnite.DesktopApp.Markup;
using Playnite.DesktopApp.ViewModels;
using Playnite.DesktopApp.Windows;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.Settings;
using Playnite.ViewModels;
using Playnite.WebView;
using Playnite.Windows;

namespace Playnite.DesktopApp
{
    public class DesktopApplication : PlayniteApplication
    {
        private ILogger logger = LogManager.GetLogger();
        private TaskbarIcon trayIcon;
        private SplashScreen splashScreen;

        private DesktopAppViewModel mainModel;
        public DesktopAppViewModel MainModel
        {
            get => mainModel;
            set
            {
                mainModel = value;
                MainModelBase = value;
            }
        }

        public static new DesktopApplication Current
        {
            get =>
                PlayniteApplication.Current == null
                    ? null
                    : (DesktopApplication)PlayniteApplication.Current;
        }

        // 命令行参数属性，预留供将来自定义功能使用
        public CmdLineOptions CmdLine { get; private set; }

        public DesktopApplication(
            Func<Application> appInitializer,
            SplashScreen splashScreen,
            CmdLineOptions cmdLineOptions
        )
            : base(appInitializer, ApplicationMode.Desktop, cmdLineOptions)
        {
            this.splashScreen = splashScreen;
            this.CmdLine = cmdLineOptions; // 存储参数但不做处理
        }

        public override void ConfigureViews()
        {
            ProgressWindowFactory.SetWindowType<ProgressWindow>();
            CrashHandlerWindowFactory.SetWindowType<CrashHandlerWindow>();
            ExtensionCrashHandlerWindowFactory.SetWindowType<ExtensionCrashHandlerWindow>();
            UpdateWindowFactory.SetWindowType<UpdateWindow>();
            LicenseAgreementWindowFactory.SetWindowType<LicenseAgreementWindow>();
            SingleItemSelectionWindowFactory.SetWindowType<SingleItemSelectionWindow>();
            MultiItemSelectionWindowFactory.SetWindowType<MultiItemSelectionWindow>();
            Dialogs = new DesktopDialogs();
            Playnite.Dialogs.SetHandler(Dialogs);
        }

        public override bool Startup()
        {
            if (!ConfigureApplication())
            {
                return false;
            }

            InstantiateApp();
            AppUriHandler = MainModel.ProcessUriRequest;
            var isFirstStart = ProcessStartupWizard();
            MigrateDatabase();
            OpenMainViewAsync(isFirstStart);
            LoadTrayIcon();
            // 已禁用启动时检查更新
            //#pragma warning disable CS4014
            //StartUpdateCheckerAsync();
            //#pragma warning restore CS4014
            splashScreen?.Close(new TimeSpan(0));
            return true;
        }

        public override void InitializeNative()
        {
            ((App)CurrentNative).InitializeComponent();
        }

        public override void Restore()
        {
            MainModel?.RestoreWindow();
        }

        public override void Minimize()
        {
            MainModel.WindowState = WindowState.Minimized;
        }

        public override void ReleaseResources(bool releaseCefSharp = true)
        {
            trayIcon?.Dispose();
            MainModel?.UnregisterSystemSearchHotkey();
            base.ReleaseResources(releaseCefSharp);
        }

        public override void Restart(bool saveSettings)
        {
            // 预留参数支持：将来可使用CmdLine属性中的参数
            QuitAndStart(
                PlaynitePaths.DesktopExecutablePath,
                string.Empty,
                saveSettings: saveSettings
            );
        }

        public override void Restart(CmdLineOptions options, bool saveSettings)
        {
            // 存储新参数但不做处理，预留供将来自定义功能使用
            this.CmdLine = options;
            Restart(saveSettings);
        }

        public override void InstantiateApp()
        {
            Database = new GameDatabase();
            Database.SetAsSingletonInstance();
            Controllers = new GameControllerFactory(Database);
            Extensions = new ExtensionFactory(Database, Controllers, GetApiInstance);
            GamesEditor = new DesktopGamesEditor(
                Database,
                Controllers,
                AppSettings,
                Dialogs,
                Extensions,
                this,
                new DesktopActionSelector()
            );
            Game.DatabaseReference = Database;
            ImageSourceManager.SetDatabase(Database);
            MainModel = new DesktopAppViewModel(
                Database,
                new MainWindowFactory(),
                Dialogs,
                new ResourceProvider(),
                AppSettings,
                (DesktopGamesEditor)GamesEditor,
                Extensions,
                this
            );
            PlayniteApiGlobal = GetApiInstance();
            SDK.API.Instance = PlayniteApiGlobal;
        }

        private void LoadTrayIcon()
        {
            if (AppSettings.EnableTray)
            {
                try
                {
                    trayIcon = new TaskbarIcon
                    {
                        MenuActivation = PopupActivationMode.LeftOrRightClick,
                        DoubleClickCommand = MainModel.ShowWindowCommand,
                        Icon = GetTrayIcon(),
                        Visibility = Visibility.Visible,
                        ContextMenu = new TrayContextMenu(MainModel),
                    };
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to initialize tray icon.");
                }
            }
        }

        private async void OpenMainViewAsync(bool isFirstStart)
        {
            if (!isFirstStart)
            {
                Extensions.LoadPlugins(
                    AppSettings.DisabledPlugins,
                    false, // 不再使用安全启动模式
                    AppSettings
                        .DevelExtenions.Where(a => a.Selected == true)
                        .Select(a => a.Item)
                        .ToList()
                );

                // 检查是否加载了YunGameGeneric插件（包括任何包含该名称的变体）
                // var yunGamePlugin = Extensions.Plugins.Values.FirstOrDefault(p =>
                //     (p.Description.Type == ExtensionType.GenericPlugin)
                //     && (
                //         p.Description.Name.Contains(
                //             "YunGameGeneric",
                //             StringComparison.OrdinalIgnoreCase
                //         )
                //         || p.Description.Id.Contains(
                //             "YunGameGeneric",
                //             StringComparison.OrdinalIgnoreCase
                //         )
                //     )
                // );
                // if (yunGamePlugin == null)
                // {
                //     MessageBox.Show(
                //         // "未检测到YunGameGeneric插件，请安装该插件后再启动应用。",
                //         // "插件缺失",
                //         "核心文件缺失，应用即将关闭！请确保没有修改核心文件，联系管理员。",

                //         "错误",
                //         MessageBoxButton.OK,
                //         MessageBoxImage.Error
                //     );
                //     Environment.Exit(1);
                // }
            }

            Extensions.LoadScripts(
                AppSettings.DisabledPlugins,
                false, // 不再使用安全启动模式
                AppSettings
                    .DevelExtenions.Where(a => a.Selected == true)
                    .Select(a => a.Item)
                    .ToList()
            );
            OnExtensionsLoaded();

            try
            {
                MainModel.ThirdPartyTools = ThirdPartyToolsList.GetTools(Extensions.LibraryPlugins);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to load third party tools.");
            }

            MainModel.OpenView();
            CurrentNative.MainWindow = MainModel.Window.Window;

            if (isFirstStart)
            {
                await MainModel.UpdateLibrary(false, true, false);
                await MainModel.DownloadMetadata(AppSettings.MetadataSettings);
            }
            else
            {
                await MainModel.ProcessStartupLibUpdate();
            }

            // This is most likely safe place to consider application to be started properly
            FileSystem.DeleteFile(PlaynitePaths.SafeStartupFlagFile);
        }

        private bool ProcessStartupWizard()
        {
            // 始终跳过首次启动检测和窗口，直接设置为非首次启动
            // 预留参数支持：将来可使用CmdLine属性中的UserDataDir
            var defaultDbDir = GameDatabase.GetDefaultPath(
                PlayniteSettings.IsPortable,
                null // 当前不使用命令行参数中的UserDataDir
            );

            // 确保数据库路径已设置
            if (AppSettings.DatabasePath.IsNullOrEmpty())
            {
                AppSettings.DatabasePath = defaultDbDir;
                AppSettings.SaveSettings();
            }

            // 设置数据库路径
            Database.SetDatabasePath(AppSettings.DatabasePath);

            // 确保数据库已打开
            if (!Database.IsOpen)
            {
                Database.OpenDatabase();
            }

            // 设置首次启动向导完成标志
            if (!AppSettings.FirstTimeWizardComplete)
            {
                AppSettings.FirstTimeWizardComplete = true;
                AppSettings.AutoBackupEnabled = true;
                AppSettings.LastAutoBackup = DateTime.Now.AddDays(1); // 延迟首次备份
                AppSettings.RotatingBackups = 3;
                AppSettings.AutoBackupDir = Path.Combine(PlaynitePaths.ConfigRootPath, "Backup");
                AppSettings.AutoBackupFrequency = AutoBackupFrequency.OnceADay;
                AppSettings.AutoBackupIncludeExtensions = false;
                AppSettings.AutoBackupIncludeExtensionsData = false;
                AppSettings.AutoBackupIncludeLibFiles = false;
                AppSettings.AutoBackupIncludeThemes = false;
                AppSettings.SaveSettings();
            }

            // 始终返回false，表示不是首次启动
            return false;
        }

        public override void ShowWindowsNotification(string title, string body, Action action)
        {
            var icon = GetTrayIcon();
            if (AppSettings.EnableTray)
            {
                trayIcon.ShowBalloonTip(title, body, icon, true);
            }
            else
            {
                WindowsNotifyIconManager.Notify(icon, title, body, action);
            }
        }

        private Icon GetTrayIcon()
        {
            var trayIconImage =
                ResourceProvider.GetResource(AppSettings.TrayIcon.GetDescription()) as BitmapImage
                ?? ResourceProvider.GetResource("TrayIcon") as BitmapImage;
            return new Icon(trayIconImage.UriSource.LocalPath);
        }

        public override void SwitchAppMode(ApplicationMode mode)
        {
            if (mode == ApplicationMode.Fullscreen)
            {
                MainModel.SwitchToFullscreenMode();
            }
            else
            {
                Restore();
            }
        }

        public override PlayniteAPI GetApiInstance(ExtensionManifest pluginOwner)
        {
            return new PlayniteAPI
            {
                Addons = new AddonsAPI(Extensions, AppSettings),
                ApplicationInfo = new PlayniteInfoAPI(),
                ApplicationSettings = new PlayniteSettingsAPI(AppSettings, Database),
                Database = new DatabaseAPI(Database),
                Dialogs = Dialogs,
                Emulation = new Emulators.Emulation(),
                MainView = new MainViewAPI(MainModel),
                Notifications = Notifications,
                Paths = new PlaynitePathsAPI(),
                Resources = new ResourceProvider(),
                RootApi = new PlayniteApiRoot(GamesEditor, Extensions, Database),
                UriHandler = UriHandler,
                WebViews = new WebViewFactory(AppSettings),
            };
        }

        public override PlayniteAPI GetApiInstance()
        {
            return new PlayniteAPI
            {
                Addons = new AddonsAPI(Extensions, AppSettings),
                ApplicationInfo = new PlayniteInfoAPI(),
                ApplicationSettings = new PlayniteSettingsAPI(AppSettings, Database),
                Database = new DatabaseAPI(Database),
                Dialogs = Dialogs,
                Emulation = new Emulators.Emulation(),
                MainView = new MainViewAPI(MainModel),
                Notifications = Notifications,
                Paths = new PlaynitePathsAPI(),
                Resources = new ResourceProvider(),
                RootApi = new PlayniteApiRoot(GamesEditor, Extensions, Database),
                UriHandler = UriHandler,
                WebViews = new WebViewFactory(AppSettings),
            };
        }
    }
}
