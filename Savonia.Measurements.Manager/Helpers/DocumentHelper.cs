using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Savonia.Measurements.Manager.Helpers
{
    public class DocumentHelper
    {
        /// <summary>
        /// Serializes an object and creates directory if not exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject">Object which will be serialized</param>
        /// <param name="filename">Filename-path where file will be created</param>
        public void SerializeObject<T>(T serializableObject, string path,string filename)
        {
            if (serializableObject == null) { return; }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, serializableObject);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save(path+filename);
                stream.Close();
            }
        }


        /// <summary>
        /// Deserializes an xml file to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            string xmlString = xmlDocument.OuterXml;

            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(T);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Close();
                }

                read.Close();
            }
            return objectOut;
        }

        /// <summary>
        /// Deserializes JSON-file to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public T DezerializeJSONFile<T>(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) { return default(T); }

            T objectOut = default(T);
            objectOut= JsonConvert.DeserializeObject<T>(File.ReadAllText(filepath), new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy HH:mm:ss.fff" });

            return objectOut;
        }

        /// <summary>
        /// Serializes object to json file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        public  void SerializeJSONFile<T>(T serializableObject, string path, string filename)
        {
            if (serializableObject == null) { return; }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(path+filename, JsonConvert.SerializeObject(serializableObject));
        }


    }
}