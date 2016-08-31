using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace Savonia.Measurements.Providers.Models
{
    /// <summary>
    /// Helper class for XML serialization/deserialization.
    /// Uses System.Runtime.Serialization.DataContractSerializer.
    /// </summary>
    public static class SerializeHelper
    {
        /// <summary>
        /// Serializes object of given type to XML-formatted string. Uses <see cref="System.Runtime.Serialization.DataContractSerializer"/>.
        /// Use attributes [DataContract] on a class and [DataMember] on a properties you wish to serialize.
        /// </summary>
        /// <typeparam name="T">Type of given object</typeparam>
        /// <param name="objectToSerialize">Object to be serialized</param>
        /// <returns>Serialized XML-string.</returns>
        public static string SerializeToStringWithDCS<T>(T objectToSerialize)
        {
            string serializedString = null;
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (Stream writer = new MemoryStream())
            {
                serializer.WriteObject(writer, objectToSerialize);
                writer.Flush();
                writer.Position = 0;
                using (StreamReader reader = new StreamReader(writer))
                {
                    serializedString = reader.ReadToEnd();
                }
            }
            return serializedString;
        }
        /// <summary>
        /// Deserializes xml-string to desired object. Uses <see cref="System.Runtime.Serialization.DataContractSerializer"/>.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stringToDeserialize">Xml-string.</param>
        /// <returns>Deserialized object.</returns>
        public static T DeserializeWithDCS<T>(string stringToDeserialize)
        {
            T deserializedObject;
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (Stream reader = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(reader))
                {
                    writer.Write(stringToDeserialize);
                    writer.Flush();
                    reader.Position = 0;
                    deserializedObject = (T)serializer.ReadObject(reader);
                }
            }

            return deserializedObject;
        }

        /// <summary>
        /// Serializes object of given type to XML-formatted string. Uses <see cref="System.Xml.Serialization"/>.
        /// Use attributes [Serializable] on a class and [XmlElement] on a properties you wish to serialize.
        /// </summary>
        /// <typeparam name="T">Type of given object</typeparam>
        /// <param name="objectToSerialize">Object to be serialized</param>
        /// <returns>Serialized XML-string.</returns>
        public static string SerializeToString<T>(T objectToSerialize)
        {
            string serializedString = null;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, objectToSerialize);
                serializedString = writer.ToString();
            }
            return serializedString;
        }
        /// <summary>
        /// Deserializes xml-string to desired object. Uses <see cref="System.Xml.Serialization"/>.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stringToDeserialize">Xml-string.</param>
        /// <returns>Deserialized object.</returns>
        public static T Deserialize<T>(string stringToDeserialize)
        {
            T deserializedObject;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(stringToDeserialize))
            {
                deserializedObject = (T)serializer.Deserialize(reader);
            }

            return deserializedObject;
        }

    }
}
