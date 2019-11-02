using System.Xml.Schema;

namespace Core
{
    public interface IPropertyFormatter
    {
        byte[] Make(byte[] value);
        byte[] Read(byte[] value);
    }
}