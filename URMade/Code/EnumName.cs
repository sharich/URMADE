using System;

namespace URMade
{
    public class EnumNameAttribute : Attribute
    {
        public string Name { get; protected set; }
        public EnumNameAttribute(string value)
        { this.Name = value; }
    }

    /// <summary>
    /// Define the Extension Methods
    /// </summary>
    public static class GetEnumAttributes
    {
        public static string GetName(this Enum value)
        {
            if (value == null) return null;
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            if (fieldInfo == null) return "";
            var attributes = fieldInfo.GetCustomAttributes(typeof(EnumNameAttribute), false) as EnumNameAttribute[];
            return attributes != null && attributes.Length > 0 ? attributes[0].Name : value.ToString();
        }
    }
}