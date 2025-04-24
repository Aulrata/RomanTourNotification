using System.ComponentModel;
using System.Reflection;

namespace RomanTourNotification.Application.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
        var attribute = (DescriptionAttribute?)fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
        return attribute is null ? value.ToString() : attribute.Description;
    }
}