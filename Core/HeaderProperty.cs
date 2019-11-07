using System;
using System.Text.RegularExpressions;

namespace Core
{
    public class HeaderProperty<T> : IHeaderProperty
    {
        public object ObjectValue => Value;

        public T Value { get; }
        public string Key { get; }
        public bool IsSet { get; }

        public HeaderProperty(T value, string key, bool isSet)
        {
            Value = value;
            Key = key;
            IsSet = isSet;
        }

        public HeaderProperty()
        {
            Value = default;
            Key = default;
            IsSet = false;
        }
    }
}