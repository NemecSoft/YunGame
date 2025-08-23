using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TGBox.Converters
{
    /// <summary>
    /// 将枚举值转换为其Description特性中定义的中文名称
    /// </summary>
    public class EnumToDescriptionConverter : IValueConverter
    {
        /// <summary>
        /// 将枚举值转换为Description特性的中文名称
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">转换参数</param>
        /// <param name="culture">文化信息</param>
        /// <returns>转换后的字符串</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var enumType = value.GetType();
            if (!enumType.IsEnum)
                return value.ToString() ?? string.Empty;

            var fieldInfo = enumType.GetField(value.ToString() ?? string.Empty);
            if (fieldInfo == null)
                return value.ToString() ?? string.Empty;

            var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                var descriptionAttribute = (DescriptionAttribute)attributes[0];
                return descriptionAttribute.Description;
            }

            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 将Description特性的中文名称转换回枚举值
        /// 暂不实现反向转换
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}