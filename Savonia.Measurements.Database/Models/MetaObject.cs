using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Text;
using Savonia.Measurements.Database.Helpers;

namespace Savonia.Measurements.Database.Models
{
    /// <summary>
    /// The MetaObject class.
    /// </summary>
    public class MetaObject
    {
        /// <summary>
        /// MetaObject context.
        /// </summary>
        [StringLength(100)]
        [Required]
        public string Context { get; set; }
        /// <summary>
        /// MetaObject object.
        /// </summary>
        [StringLength(150)]
        [Required]
        public string Object { get; set; }

        /// <summary>
        /// MetaObject properties.
        /// </summary>
        public List<MetaProperty> Properties { get; set; }

        private List<MetaProperty> readProperties;

        public MetaObject()
        {
            this.Properties = new List<MetaProperty>();
        }

        public MetaObject(string context, string @object, params MetaProperty[] properties)
        {
            this.Context = context;
            this.Object = @object;
            if (null != properties)
            {
                this.Properties = new List<MetaProperty>(properties.Count());
                this.Properties.AddRange(properties);
            }
            else
            {
                this.Properties = new List<MetaProperty>();
            }
        }

        /// <summary>
        /// MetaObject is valid when it has properties.
        /// </summary>
        public bool IsValid
        {
            get 
            {
                if (null == this.Properties || this.Properties.Count < 1)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Check whether the meta object has the given property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool HasProperty(string propertyName)
        {
            if (null == this.Properties)
            {
                return false;
            }
            return this.Properties.Any(p => p.Tag == propertyName);
        }

        /// <summary>
        /// Get all properties which value is not allready read.
        /// </summary>
        public IEnumerable<MetaProperty> UnreadProperties
        {
            get
            { 
                var read = readProperties.Select(p => p.PropertyId).Distinct();
                return this.Properties.Where(p => !read.Contains(p.PropertyId));
            }
        }

        private MetaProperty GetProperty(string tag, int version)
        {
            if (null == this.Properties)
            {
                return null;
            }
            MetaProperty property = null;
            if (version > MetaDataRepository.LatestVersion)
            {
                // get specific version
                property = this.Properties.SingleOrDefault(p => p.Tag == tag && p.Version == version);
            }
            else
            {
                // get latest version
                property = this.Properties.Where(p => p.Tag == tag).OrderBy(p => p.Version).LastOrDefault();
            }
            return property;
        }

        /// <summary>
        /// Get property value from MetaObject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="decryptor">When decryptor is not null the value is decrypted.</param>
        /// <returns></returns>
        public T GetPropertyValue<T>(MetaProperty property)
        {
            if (null == property)
            {
                return default(T);
            }
            if (null == readProperties)
            {
                readProperties = new List<MetaProperty>();
            }
            readProperties.Add(property);
            return this.ConvertTo<T>(property.Value);
        }
        /// <summary>
        /// Get property value from MetaObject. Returns <see cref="default(T)"/> if property is not present or it has no value. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <param name="decryptor">When decryptor is not null the value is decrypted.</param>
        /// <returns></returns>
        public T GetPropertyValue<T>(string tag, int version = MetaDataRepository.LatestVersion)
        {
            MetaProperty property = this.GetProperty(tag, version);
            return GetPropertyValue<T>(property);
        }

        private T ConvertTo<T>(byte[] data)
        {
            if (null == data)
            {
                return default(T);
            }
            return MetaDataConverter.ConvertTo<T>(data);
        }
        /// <summary>
        /// Add property to MetaObject. Adds property as new version to this MetaObject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <param name="isRequired"></param>
        /// <param name="encryptor">When encryptor is not null the value is encrypted.</param>
        public void AddProperty<T>(string tag, T value)
        {
            this.SetProperty<T>(tag, value, createNewVersion: true);
        }
        /// <summary>
        /// Set property to MetaObject. Modifies existing value or adds as a new value if existing tag is not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <param name="isRequired"></param>
        /// <param name="version"></param>
        /// <param name="createNewVersion"></param>
        /// <param name="encryptor">When encryptor is not null the value is encrypted.</param>
        public void SetProperty<T>(string tag, T value, int version = MetaDataRepository.LatestVersion, bool createNewVersion = false)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }
            MetaProperty property = null;
            if (createNewVersion)
            {
                property = this.GetProperty(tag, MetaDataRepository.LatestVersion);
            }
            else
            {
                property = this.GetProperty(tag, version);
            }
            if (null == property)
            {
                if (null == this.Properties)
                {
                    this.Properties = new List<MetaProperty>();
                }
                this.Properties.Add(new MetaProperty(tag, null != value ? MetaDataConverter.ConvertToByteArray<T>(value) : null));
            }
            else
            {
                if (createNewVersion)
                {
                    this.Properties.Add(new MetaProperty(tag, null != value ? MetaDataConverter.ConvertToByteArray<T>(value) : null, property.Version + 1));
                }
                else
                {
                    property.Value = null != value ? MetaDataConverter.ConvertToByteArray<T>(value) : null;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Context = {0}, Object = {1}", Context, Object));
            foreach (var p in Properties)
            {
                sb.AppendLine(string.Format("\t{0}", p));
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// The MetaProperty class.
    /// </summary>
    public class MetaProperty
    {
        /// <summary>
        /// MetaProperty tag. This is the attribute name.
        /// </summary>
        [StringLength(100)]
        [Required]
        public string Tag { get; set; }
        /// <summary>
        /// MetaProperty value. This is the attribute's value.
        /// </summary>
        public byte[] Value { get; set; }
        /// <summary>
        /// MetaProperty version. This is the attibute version.
        /// </summary>
        public int Version { get; set; }
        public MetaProperty()
        { }

        public MetaProperty(string tag, byte[] value, int version = 1)
        {
            this.Tag = tag;
            this.Value = value;
            this.Version = version;
        }

        /// <summary>
        /// Property id. Equals Tag-Version.
        /// </summary>
        public string PropertyId
        {
            get { return string.Format("{0}-{1}", Tag, Version); }

        }

        public override string ToString()
        {
            return string.Format("Tag = {0}, Version = {1}, Value = {2}", Tag, Version, Value.ConvertToString());
        }
    }
}