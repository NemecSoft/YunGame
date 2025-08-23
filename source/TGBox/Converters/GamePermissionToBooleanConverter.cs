using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TGBox.Models;

namespace TGBox.Converters
{
    /// <summary>
    /// 游戏权限转换器
    /// 用于检查用户是否有权限玩特定游戏
    /// </summary>
    public class GamePermissionToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            // 确保value是Game对象
            if (value is not Game game)
                return false;
            
            // 如果没有提供用户级别，默认返回true
            if (parameter is not UserLevel userLevel)
                return true;
            
            // 检查用户是否有权限玩此游戏
            return GamePermissionManager.CanPlayGame(userLevel, game.GameLevel);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            // 不支持反向转换
            throw new NotImplementedException();
        }
    }
}