using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using Savonia.Measurements.Database.Models;

namespace Savonia.Measurements.Database.Helpers
{
    public static class MetaDataHelper
    {
       
        
        #region "Convert byte[] to a primitive type"
        public static string ConvertToString(this byte[] data)
        {
            if (null == data)
            {
                return null;
            }
            return System.Text.UnicodeEncoding.Unicode.GetString(data);
        }

        public static bool ConvertToBoolean(this byte[] data)
        {
            if (null == data)
            {
                return false;
            }
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(data);

            return BitConverter.ToBoolean(data, 0);
        }

        public static char ConvertToChar(this byte[] data)
        {
            if (null == data)
            {
                return char.MinValue;
            }
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(data);

            return BitConverter.ToChar(data, 0);
        }

        public static short ConvertToInt16(this byte[] data)
        {
            if (null == data)
            {
                return 0;
            }
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public static int ConvertToInt32(this byte[] data)
        {
            if (null == data)
            {
                return 0;
            }
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public static long ConvertToInt64(this byte[] data)
        {
            if (null == data)
            {
                return 0;
            }
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public static float ConvertToSingle(this byte[] data)
        {
            if (null == data)
            {
                return float.NaN;
            }
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }

        public static double ConvertToDouble(this byte[] data)
        {
            if (null == data)
            {
                return double.NaN;
            }
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }
        #endregion

    

        /// <summary>
        /// Compare two object contents. Uses <see cref="StructuralComparisons.StructuralEqualityComparer.Equals"/> to make the comparison.
        /// Returns true when the two objects contains the same data.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool ContentEquals(this object x, object y)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
        }

        public static IQueryable<Meta> GetObject(this IQueryable<Meta> meta, string @object)
        {
            if (null == meta)
            {
                return null;
            }
            return meta.Where(m => m.Object == @object);
        }

        public static IQueryable<Meta> GetObjectByTag(this IQueryable<Meta> meta, string tag, byte[] value)
        {
            if (null == meta)
            {
                return null;
            }
            var obj = meta.FirstOrDefault(m => m.Tag == tag && m.Data == value);
            if (null != obj)
            {
                return meta.GetObject(obj.Object);
            }
            return null;
        }


        public static byte[] GetData(this IQueryable<Meta> @object, string tag = null)
        {
            if (null == @object)
            {
                return null;
            }

            byte[] value = null;
            Meta mc;
            if (string.IsNullOrEmpty(tag))
            {
                mc = @object.FirstOrDefault();
            }
            else
            {
                mc = @object.FirstOrDefault(m => m.Tag == tag);
            }

            if (null != mc)
            {
                value = mc.Data;
            }

            return value;
        }

    }
}