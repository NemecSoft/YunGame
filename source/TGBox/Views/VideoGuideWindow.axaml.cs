using System;
using System.Collections.Generic;
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
using LibVLCSharp.Shared;
using LibVLCSharp.Avalonia;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

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
        public ICommand PlayPauseCommand { get; }
        public ICommand ToggleMuteCommand { get; }
        
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private VideoFile _currentVideo;
        private Slider _positionSlider;
        private TextBlock _timeDisplay;
        private Button _playPauseButton;
        private Slider _volumeSlider;

        public VideoGuideWindow()
        {
            InitializeComponent();
            
            // 初始化LibVLC
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            
            // 初始化命令
            RefreshCommand = ReactiveCommand.Create(RefreshVideos);
            PlayCommand = ReactiveCommand.Create<VideoFile>(PlayVideo);
            OpenFolderCommand = ReactiveCommand.Create<VideoFile>(OpenVideoFolder);
            PlayPauseCommand = ReactiveCommand.Create(TogglePlayPause);
            ToggleMuteCommand = ReactiveCommand.Create(ToggleMute);
            
            this.DataContext = this;
            this.AttachedToVisualTree += OnAttachedToVisualTree;
            this.Closing += OnWindowClosing;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // 获取UI元素引用
            _positionSlider = this.FindControl<Slider>("PositionSlider");
            _timeDisplay = this.FindControl<TextBlock>("TimeDisplay");
            _playPauseButton = this.FindControl<Button>("PlayPauseButton");
            _volumeSlider = this.FindControl<Slider>("VolumeSlider");
            
            // 设置视频视图
            if (this.FindControl<VideoView>("VideoView") is VideoView videoView)
            {
                videoView.MediaPlayer = _mediaPlayer;
            }
            
            // 设置滑块事件
            if (_positionSlider != null)
            {
                _positionSlider.PointerPressed += (s, ev) => _mediaPlayer.SetPause(true);
                _positionSlider.PointerReleased += (s, ev) =>
                {
                    if (_mediaPlayer.IsSeekable)
                    {
                        _mediaPlayer.Position = (float)_positionSlider.Value;
                    }
                    _mediaPlayer.Play();
                };
            }
            
            if (_volumeSlider != null)
            {
                _volumeSlider.ValueChanged += (s, ev) =>
                {
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.Volume = (int)ev.NewValue;
                    }
                };
            }
            
            // 监听媒体播放器事件
            _mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            _mediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            _mediaPlayer.EndReached += MediaPlayer_EndReached;
            
            RefreshVideos();
        }
        
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            // 清理资源
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
            }
            
            if (_libVLC != null)
            {
                _libVLC.Dispose();
            }
        }

        private void RefreshVideos()
        {
            Debug.WriteLine($"[视频窗口] 刷新视频列表，游戏名称: '{GameName}'");
            Videos.Clear();
            
            if (string.IsNullOrEmpty(GameName))
            {
                Debug.WriteLine("[视频窗口] 游戏名称为空，无法刷新视频列表");
                return;
            }

            try
            {
                // 获取游戏视频攻略目录（检查两种可能路径）
                var appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Debug.WriteLine($"[视频窗口] APPDATA路径: {appDataRoot}");
                var pathsToCheck = new List<string>{
                    // 用户直接放在TGBox下的路径
                    Path.Combine(appDataRoot, "TGBox", GameName),
                    // 系统预期的VideoGuides子目录路径
                    Path.Combine(appDataRoot, "TGBox", "VideoGuides", GameName)
                };

                var videoExtensions = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v" };
                List<string> allVideoFiles = new List<string>();
                
                // 检查所有可能的路径
                foreach (var path in pathsToCheck)
                {
                    Debug.WriteLine($"[视频窗口] 检查路径: {path}");
                    if (Directory.Exists(path))
                    {
                        Debug.WriteLine($"[视频窗口] 路径存在: {path}");
                        var videoFiles = Directory.GetFiles(path)
                            .Where(file => videoExtensions.Contains(Path.GetExtension(file).ToLower()))
                            .ToList();
                        
                        Debug.WriteLine($"[视频窗口] 在 {path} 中找到 {videoFiles.Count} 个视频文件");
                        // 输出找到的文件列表
                        foreach (var file in videoFiles)
                        {
                            Debug.WriteLine($"[视频窗口] 找到文件: {file}");
                        }
                        allVideoFiles.AddRange(videoFiles);
                    }
                    else
                    {
                        Debug.WriteLine($"[视频窗口] 路径不存在: {path}");
                        if (path == pathsToCheck[1]) // 只在系统预期路径不存在时创建
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine($"[视频窗口] 创建目录: {path}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[视频窗口] 创建目录失败: {ex.Message}");
                            }
                        }
                    }
                }
                
                Debug.WriteLine($"[视频窗口] 总共找到 {allVideoFiles.Count} 个视频文件");
                
                // 排序所有找到的视频文件
                var sortedVideoFiles = allVideoFiles
                    .OrderByDescending(file => new FileInfo(file).CreationTime);

                foreach (var videoFile in sortedVideoFiles)
                {
                    var fileInfo = new FileInfo(videoFile);
                    Debug.WriteLine($"[视频窗口] 添加视频文件: {videoFile}");
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
                
                Debug.WriteLine($"[视频窗口] 视频列表刷新完成，共 {Videos.Count} 个视频");
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
                    _currentVideo = video;
                    
                    // 创建媒体并播放
                    var media = new Media(_libVLC, new Uri(video.FilePath));
                    _mediaPlayer.Media = media;
                    _mediaPlayer.Play();
                    
                    // 更新播放按钮状态
                    if (_playPauseButton != null)
                    {
                        _playPauseButton.Content = "暂停";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("播放视频失败: " + ex.Message);
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
        
        /// <summary>
        /// 切换播放/暂停状态
        /// </summary>
        private void TogglePlayPause()
        {
            if (_mediaPlayer != null)
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Pause();
                    if (_playPauseButton != null)
                    {
                        _playPauseButton.Content = "播放";
                    }
                }
                else
                {
                    _mediaPlayer.Play();
                    if (_playPauseButton != null)
                    {
                        _playPauseButton.Content = "暂停";
                    }
                }
            }
        }
        
        /// <summary>
        /// 切换静音状态
        /// </summary>
        private void ToggleMute()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Mute = !_mediaPlayer.Mute;
            }
        }
        
        /// <summary>
        /// 媒体播放时间变化事件处理
        /// </summary>
        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            if (_timeDisplay != null && _mediaPlayer.Media != null)
            {
                var currentTime = TimeSpan.FromMilliseconds(e.Time);
                var totalTime = TimeSpan.FromMilliseconds(_mediaPlayer.Media.Duration);
                
                _timeDisplay.Text = $"{currentTime:mm':'ss} / {totalTime:mm':'ss}";
            }
        }
        
        /// <summary>
        /// 媒体播放位置变化事件处理
        /// </summary>
        private void MediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            if (_positionSlider != null && !_positionSlider.IsPointerOver)
            {
                _positionSlider.Value = e.Position;
            }
        }
        
        /// <summary>
        /// 媒体播放结束事件处理
        /// </summary>
        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            if (_playPauseButton != null)
            {
                _playPauseButton.Content = "播放";
            }
            
            // 可以选择自动播放下一个视频或重置到开始
            _mediaPlayer.Position = 0;
        }
    }


}