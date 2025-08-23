using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using TGBox.Models;

namespace TGBox.Views
{
    public partial class GameEditWindow : Window
    {
        public Game Game { get; set; }
        public bool IsNewGame { get; set; }

        // 无参构造函数，用于Avalonia运行时资源加载
        public GameEditWindow()
        {
            InitializeComponent();
            Game = new Game();
            IsNewGame = true;
        }

        public GameEditWindow(Game? game = null)
        {
            InitializeComponent();
            
            if (game == null)
            {
                Game = new Game();
                IsNewGame = true;
                Title = "添加新游戏";
            }
            else
            {
                Game = game;
                IsNewGame = false;
                Title = "编辑游戏信息";
            }

            // 初始化表单数据
            LoadGameData();
        }

        private void LoadGameData()
        {
            GameNameTextBox.Text = Game.Name;
            AlternativeNameTextBox.Text = Game.AlternativeName;
            GameTypeTextBox.Text = Game.Genre;
            ReleaseYearNumeric.Value = Game.ReleaseYear;
            DescriptionTextBox.Text = Game.Description;
            GamePathTextBox.Text = Game.Path;
            CoverPathTextBox.Text = Game.CoverPath;
            IconPathTextBox.Text = Game.IconPath;
            Screenshot1TextBox.Text = Game.Screenshot1;
            Screenshot2TextBox.Text = Game.Screenshot2;
            Screenshot3TextBox.Text = Game.Screenshot3;
            IsInstalledCheckBox.IsChecked = Game.IsInstalled;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 保存游戏数据
                Game.Name = GameNameTextBox.Text ?? string.Empty;
                Game.AlternativeName = AlternativeNameTextBox.Text ?? string.Empty;
                Game.Genre = GameTypeTextBox.Text ?? string.Empty;
                Game.ReleaseYear = (int)(ReleaseYearNumeric?.Value ?? 2024);
                Game.Description = DescriptionTextBox.Text ?? string.Empty;
                Game.Path = GamePathTextBox.Text ?? string.Empty;
                Game.CoverPath = CoverPathTextBox.Text ?? string.Empty;
                Game.IconPath = IconPathTextBox.Text ?? string.Empty;
                Game.Screenshot1 = Screenshot1TextBox.Text ?? string.Empty;
                Game.Screenshot2 = Screenshot2TextBox.Text ?? string.Empty;
                Game.Screenshot3 = Screenshot3TextBox.Text ?? string.Empty;
                Game.IsInstalled = IsInstalledCheckBox?.IsChecked ?? false;

                // TODO: 保存多媒体文件、存档、MOD等

                // 关闭窗口并返回成功
                Close(true);
            }
            catch (Exception ex)
            {
                // 显示错误消息
                var messageBox = new Window
                {
                    Title = "错误",
                    Width = 300,
                    Height = 150,
                    Content = new TextBlock
                    {
                        Text = $"保存失败: {ex.Message}",
                        Margin = new Thickness(10),
                        TextWrapping = TextWrapping.Wrap
                    }
                };
                messageBox.ShowDialog(this);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}