using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TGBox.Views.GameViews
{
    /// <summary>
    /// 网格视图组件
    /// 用于以网格形式展示游戏列表
    /// </summary>
    public partial class GridListView : UserControl
    {
        public GridListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}