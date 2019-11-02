using System;
using System.Xml.Schema;

namespace Core
{
    public interface IPropertyFormatter
    {
        byte[] Make(byte[] value);
        object Read(byte[] value, Type type);
    }
}