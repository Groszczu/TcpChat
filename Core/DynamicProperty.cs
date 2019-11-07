using System;

namespace Core
{
    public class DynamicProperty: Attribute
    {
        public string Key { get; }
        public int FormatLength { get; }
        public string LengthField { get; }
        public Type PropertyFormatter { get; }

        public DynamicProperty(string key, int formatLength, string lengthField, Type propertyFormatter)
        {
            Key = key;
            FormatLength = formatLength;
            LengthField = lengthField;
            PropertyFormatter = propertyFormatter;
        }
    }
}