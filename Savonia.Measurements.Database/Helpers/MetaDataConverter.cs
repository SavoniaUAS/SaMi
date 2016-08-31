using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Database.Helpers
{
    public static class MetaDataConverter
    {
        public static byte[] ConvertToByteArray<T>(this T data)
        {
            Type t = typeof(T);
            switch (t.FullName)
            {
                case "System.String":
                    {
                        if (null == data)
                        {
                            return null;
                        }
                        return System.Text.UnicodeEncoding.Unicode.GetBytes(data as string);
                    }
                case "System.Boolean":
                    {
                        return BitConverter.GetBytes((bool)(object)data);
                    }
                case "System.Char":
                    {
                        return BitConverter.GetBytes((char)(object)data);
                    }
                case "System.Int16":
                    {
                        return BitConverter.GetBytes((short)(object)data);
                    }
                case "System.Int32":
                    {
                        return BitConverter.GetBytes((int)(object)data);
                    }
                case "System.Int64":
                    {
                        return BitConverter.GetBytes((long)(object)data);
                    }
                case "System.Single":
                    {
                        return BitConverter.GetBytes((float)(object)data);
                    }
                case "System.Double":
                    {
                        return BitConverter.GetBytes((double)(object)data);
                    }
                case "System.Byte[]":
                    {
                        return data as byte[];
                    }
                default:
                    return BinarySerialize(data);
            }
        }

        public static T ConvertTo<T>(this byte[] data)
        {
            Type t = typeof(T);
            
            switch (t.FullName)
            {
                case "System.String":
                    {
                        return (T)(object)data.ConvertToString();
                    }
                case "System.Boolean":
                    {
                        return (T)(object)data.ConvertToBoolean();
                    }
                case "System.Char":
                    {
                        return (T)(object)data.ConvertToChar();
                    }
                case "System.Int16":
                    {
                        return (T)(object)data.ConvertToInt16();
                    }
                case "System.Int32":
                    {
                        return (T)(object)data.ConvertToInt32();
                    }
                case "System.Int64":
                    {
                        return (T)(object)data.ConvertToInt64();
                    }
                case "System.Single":
                    {
                        return (T)(object)data.ConvertToSingle();
                    }
                case "System.Double":
                    {
                        return (T)(object)data.ConvertToDouble();
                    }
                case "System.Byte[]":
                    {
                        return (T)(object)data;
                    }
                default:
                    return BinaryDeserialize<T>(data);
            }
        }

        private static T BinaryDeserialize<T>(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object o = formatter.Deserialize(ms);
                return (T)o;
            }
        }

        private static byte[] BinarySerialize(object data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, data);
                return ms.ToArray();
            }
        }
    }
}
