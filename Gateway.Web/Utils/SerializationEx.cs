using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Gateway.Web.Utils
{
    public static class SerializationEx
    {
        private static readonly ConcurrentDictionary<Type, SerializerRegistration> Serializers =
            new ConcurrentDictionary<Type, SerializerRegistration>();

        public static string Serialize(this object obj, params Type[] additionalTypes)
        {
            return obj.Serialize(false, additionalTypes);
        }

        /// <summary>
        ///     Serializes object into string
        /// </summary>
        public static string Serialize(this object obj, bool isIndented, params Type[] additionalTypes)
        {
            var ser = GetSerializer(obj.GetType(), additionalTypes);

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = isIndented
                }))
                {
                    var ns = new XmlSerializerNamespaces();
                    //Add an empty namespace and empty value
                    ns.Add("", "");

                    // Got an exception on this line? If you've just added a new type of be serialised,
                    // make sure you add a ISeria1isedTypesProvider to provide it to the serialiser.
                    ser.Serialize(xmlWriter, obj, ns);

                    return stringWriter.ToString();
                }
            }
        }

        public static XElement SerializeToXElement(this object obj, params Type[] additionalTypes)
        {
            return XElement.Parse(obj.Serialize(additionalTypes));
        }

        public static T Deserialize<T>(this string str, params Type[] additionalTypes)
        {
            var xs = GetSerializer(typeof(T), additionalTypes);
            var obj = xs.Deserialize(new StringReader(str));
            return (T)obj;
        }

        public static T Deserialize<T>(this XElement element, params Type[] additionalTypes)
        {
            return Deserialize<T>(element.ToString(), additionalTypes);
        }

        private static XmlSerializer GetSerializer(Type type, params Type[] additionalTypes)
        {
            var result = Serializers.GetOrAdd(type, _ => new SerializerRegistration(type, additionalTypes));

            if (!additionalTypes.SequenceEqual(result.AdditionalTypes))
                throw new InvalidOperationException("XmlSerializer registered with different additional types");
            return result.Serializer;
        }

        /// <summary>
        ///     Needed for unit tests only
        /// </summary>
        public static void ClearCachedSerializers()
        {
            Serializers.Clear();
        }

        public static IEnumerable<SerializerRegistration> GetCachedSerializers()
        {
            return Serializers.Values;
        }

        public class SerializerRegistration
        {
            public SerializerRegistration(Type type, params Type[] additionalTypes)
            {
                RegisteredType = type;
                AdditionalTypes = additionalTypes;
                Serializer = new XmlSerializer(type, additionalTypes);
            }

            public Type RegisteredType { get; private set; }
            public XmlSerializer Serializer { get; private set; }
            public Type[] AdditionalTypes { get; private set; }

            public override string ToString()
            {
                if (AdditionalTypes == null || AdditionalTypes.Length == 0)
                    return string.Format("XmlSerializer<{0}>", RegisteredType.Name);
                return string.Format("XmlSerializer<{0}> ({1})", RegisteredType.Name,
                    string.Join(", ", AdditionalTypes.Select(type => type.Name)));
            }
        }
    }

}