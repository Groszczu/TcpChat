using System;
using System.Text.RegularExpressions;

namespace Core
{
    public class HeaderProperty : Attribute
    {
        public Type PropertyFormatterType { get; }
        public int MaximumLengthInBytes { get; }
        public string Key { get; }
        public int OrderNumber { get; }

        public HeaderProperty(int orderNumber, string key, int maximumLengthInBytes, Type propertyFormatterType)
        {
            PropertyFormatterType = propertyFormatterType;
            MaximumLengthInBytes = maximumLengthInBytes;
            OrderNumber = orderNumber;
            Key = key;
        }
    }
}