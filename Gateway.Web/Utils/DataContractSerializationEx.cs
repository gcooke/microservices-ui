using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace Gateway.Web.Utils
{
    public static class DataContractSerializationEx
    {
        public static T DeserializeUsingDataContract<T>(this string xml)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                return (T)serializer.ReadObject(ms);
            }
        }

        public static T DeserializeUsingDataContract<T>(this XElement xml)
        {
            return DeserializeUsingDataContract<T>(xml.ToString());
        }
    }
}