using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class HeaderPropertyFormatter : IPropertyFormatter
    {
        private readonly string _key;

        private readonly Dictionary<Type, Func<byte[], object>> _typesDictionary =
            new Dictionary<Type, Func<byte[], object>>
            {
                {typeof(Guid), bytes =>
                    {
                        var text = Encoding.UTF8.GetString(bytes);
                        return new Guid(text);
                    }
                },
                {typeof(int), bytes =>
                    {
                        var text = Encoding.UTF8.GetString(bytes);
                        return int.Parse(text);
                    }
                },
                {typeof(string), Encoding.UTF8.GetString}
            };

        public HeaderPropertyFormatter(string key)
        {
            _key = key;
        }

        public byte[] Make(byte[] value)
        {
            var prefix = Encoding.UTF8.GetBytes($"{_key}-)");
            var suffix = Encoding.UTF8.GetBytes("(|");

            return prefix.Union(value).Union(suffix).ToArray();
        }

        public object Read(byte[] value, Type type)
        {
            var span = new Span<byte>(value);
            var start = _key.Length + 2;
            var length = value.Length - start - 2;
            var result = span.Slice(start, length);
            var array = result.ToArray();
            
            if (type.IsEnum)
            {
                return Enum.ToObject(type, _typesDictionary[typeof(int)](array));
            }
            
            return _typesDictionary[type](array);
        }
    }
}