using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using ReactiveUI;
using TGBox.Models;

namespace TGBox.Views
{
    public partial class VideoGuideWindow : Window
    {
        public static readonly StyledProperty<string> GameNameProperty =
            AvaloniaProperty.Register<VideoGuideWindow, string>(nameof(GameName));

        public string GameName
        {
            get => GetValue(GameNameProperty);
            set => SetValue(GameNameProperty, value);
        }

        public ObservableCollection<VideoFile> Videos { get; } = new ObservableCollection<VideoFile>();
        
        public ICommand RefreshCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand OpenFolderCommand { get; }

        public VideoGuideWindow()
        {
            InitializeComponent();
            
            RefreshCommand = ReactiveCommand.Create(RefreshVideos);
            PlayCommand = ReactiveCommand.Create<VideoFile>(PlayVideo);
            OpenFolderCommand = ReactiveCommand.Create<VideoFile>(OpenVideoFolder);
            
            this.DataContext = this;
            this.AttachedToVisualTree += OnAttachedToVisualTree;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            RefreshVideos();
        }

        private void RefreshVideos()
        {
            Videos.Clear();
            
            if (string.IsNullOrEmpty(GameName))
                return;

            try
            {
                // 获取游戏视频攻略目录
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TGBox",
                    "VideoGuides",
                    GameName);

                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                    return;
                }

                var videoExtensions = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v" };
                
                var videoFiles = Directory.GetFiles(appDataPath)
                    .Where(file => videoExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .OrderByDescending(file => new FileInfo(file).CreationTime);

                foreach (var videoFile in videoFiles)
                {
                    var fileInfo = new FileInfo(videoFile);
                    var video = new VideoFile
                    {
                        FilePath = videoFile,
                        Title = Path.GetFileNameWithoutExtension(videoFile),
                        Description = $"创建于 {fileInfo.CreationTime:yyyy-MM-dd HH:mm}",
                        Duration = GetVideoDuration(videoFile),
                        FileSize = FormatFileSize(fileInfo.Length),
                        ThumbnailPath = GetVideoThumbnail(videoFile)
                    };
                    
                    Videos.Add(video);
                }
            }
            catch (Exception ex)
            {
                // 忽略加载错误
            }
        }

        private string GetVideoDuration(string videoPath)
        {
            // 这里可以实现实际的视频时长获取逻辑
            // 暂时返回模拟数据
            var random = new Random();
            var minutes = random.Next(1, 60);
            var seconds = random.Next(0, 60);
            return $"{minutes:00}:{seconds:00}";
        }

        private string GetVideoThumbnail(string videoPath)
        {
            // 这里可以实现实际的缩略图生成逻辑
            // 暂时返回默认图标
            return "/Assets/avalonia-logo.ico";
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void PlayVideo(VideoFile video)
        {
            if (File.Exists(video.FilePath))
            {
                try
                {
                    // 使用系统默认播放器打开视频
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = video.FilePath;
                    process.StartInfo.UseShellExecute = true;
                    process.Start();
                }
                catch (Exception ex)
                {
                    // 忽略播放错误
                }
            }
        }

        private void OpenVideoFolder(VideoFile video)
        {
            if (File.Exists(video.FilePath))
            {
                try
                {
                    // 打开视频所在文件夹
                    var folderPath = Path.GetDirectoryName(video.FilePath);
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        var process = new System.Diagnostics.Process();
                        process.StartInfo.FileName = folderPath;
                        process.StartInfo.UseShellExecute = true;
                        process.Start();
                    }
                }
                catch (Exception ex)
                {
                    // 忽略打开文件夹错误
                }
            }
        }
    }


}