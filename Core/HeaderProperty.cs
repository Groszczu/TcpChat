using System;
using System.Text.RegularExpressions;

namespace Core
{
    public class HeaderProperty : Attribute
    {
        public Type PropertyFormatterType { get; }
        public int LengthInBytes { get; }
        public string Key { get; }
        public int OrderNumber { get; }

        public HeaderProperty(int orderNumber, string key, int lengthInBytes, Type propertyFormatterType)
        {
            PropertyFormatterType = propertyFormatterType;
            LengthInBytes = lengthInBytes;
            OrderNumber = orderNumber;
            Key = key;
        }
    }
}