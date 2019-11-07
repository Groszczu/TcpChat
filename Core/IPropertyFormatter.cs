using System;

namespace Core
{
    public interface IPropertyFormatter
    {
        byte[] Make(byte[] value);
        object Read(byte[] value, Type type);
    }
}