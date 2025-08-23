using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TGBox.Converters
{
    /// <summary>
    /// 将枚举值转换为布尔值的转换器
    /// 当枚举值等于ConverterParameter时返回true，否则返回false
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            // 检查value和parameter是否为null
            if (value == null || parameter == null)
                return false;

            // 检查value是否为枚举类型
            if (value.GetType().IsEnum)
            {
                // 安全地获取value和parameter的字符串表示
                string? valueStr = value.ToString();
                string? parameterStr = parameter.ToString();
                
                // 检查字符串是否为空或null
                if (string.IsNullOrEmpty(valueStr) || string.IsNullOrEmpty(parameterStr))
                    return false;
                
                // 直接比较枚举值的字符串表示
                return valueStr.Equals(parameterStr, StringComparison.OrdinalIgnoreCase);
            }
            
            // 对于非枚举类型，保持原有逻辑
            string? enumValue = value.ToString();
            string? targetValue = parameter.ToString();
            
            // 检查字符串是否为空或null
            if (string.IsNullOrEmpty(enumValue) || string.IsNullOrEmpty(targetValue))
                return false;
                
            return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            // 检查value、parameter和targetType是否为null
            if (value == null || parameter == null || targetType == null)
                return null;

            if (value is not bool boolValue)
                return null;
                
            string? targetValue = parameter.ToString();
            // 检查字符串是否为空或null
            if (string.IsNullOrEmpty(targetValue))
                return null;
            
            if (boolValue)
            {
                try
                {
                    // 检查targetType是否为枚举类型
                    if (targetType.IsEnum)
                    {
                        return Enum.Parse(targetType, targetValue, true);
                    }
                    return targetValue;
                }
                catch
                {
                    return null;
                }
            }
            
            return null;
        }
    }
}