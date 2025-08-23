using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TGBox.Views.GameViews
{
    /// <summary>
    /// 卡片列表视图组件
    /// 用于以卡片形式展示游戏列表
    /// </summary>
    public partial class CardListView : UserControl
    {
        public CardListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}