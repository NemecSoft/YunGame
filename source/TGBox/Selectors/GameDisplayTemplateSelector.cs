using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using TGBox.ViewModels;
using TGBox.Models;
using System;

namespace TGBox.Selectors
{
    public class GameDisplayTemplateSelector : IDataTemplate
    {
        public IDataTemplate? CardListTemplate { get; set; }
        public IDataTemplate? GridTemplate { get; set; }
        public IDataTemplate? FallbackTemplate { get; set; }

        public bool SupportsRecycling => false;

        public Control? Build(object? data)
        {
            if (data == null)
                return null;

            // 获取当前数据项
            var game = data as Game;
            if (game == null)
                return FallbackTemplate?.Build(data);

            // 尝试获取MainWindowViewModel
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var viewModel = desktop.MainWindow.DataContext as MainWindowViewModel;
                if (viewModel != null)
                {
                    // 根据CurrentDisplayMode选择模板
                    var template = viewModel.CurrentDisplayMode == ViewModels.GameDisplayMode.Grid ? GridTemplate : CardListTemplate;
                    return template?.Build(data);
                }
            }

            // 如果无法获取ViewModel，默认使用卡片列表模板
            return CardListTemplate?.Build(data);
        }

        public bool Match(object? data)
        {
            // 只匹配单个Game对象
            return data is Game;
        }
    }
}