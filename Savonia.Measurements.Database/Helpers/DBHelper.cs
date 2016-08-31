using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Savonia.Measurements.Models;
using System.Data.Entity.Spatial;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Configuration;
using System.Globalization;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Savonia.Measurements.Database.Helpers
{
    public static class DBHelper
    {
        public const int TagMaxLength = 50;
        public const int ObjectMaxLength = 50;
        public const int NameMaxLength = 250;
        public const int KeySize = 256;
        public const int BlockSize = 128;

        /// <summary>
        /// Returns maximum of maxLength characters from original string.
        /// </summary>
        /// <param name="?"></param>
        /// <param name="maxLenght"></param>
        /// <returns></returns>
        public static string SafeGet(this string original, int maxLenght)
        {
            if (null != original && original.Length > maxLenght)
            {
                return original.Substring(0, maxLenght);
            }
            return original;
        }

        public static Measurement ToMeasurement(this MeasurementModel model, int providerID, short? keyId)
        {
            DbGeography location = model.Location.ToGeography();
            Measurement measurement = null;
            if (model.TimestampISO8601 != null)
            {
                DateTimeOffset dto;
                if (DateTimeOffset.TryParse(model.TimestampISO8601,out dto))
                {
                    model.Timestamp = dto;
                }   
            }
            
            measurement = new Measurement()
            {
                Object = model.Object.SafeGet(ObjectMaxLength),
                Note = model.Note,
                Tag = model.Tag.SafeGet(TagMaxLength),
                Timestamp = model.Timestamp,
                RowCreatedTimestamp = DateTimeOffset.Now,
                ProviderID = providerID,
                Location = location,
                KeyId = keyId
            };            
            return measurement;
        }

        public static DbGeography ToGeography(this Location location)
        {
            if (null == location || !location.IsValid)
            {
                return null;
            }
            DbGeography dbg = null;
            dbg = DbGeography.FromText(location.ToString(), DbGeography.DefaultCoordinateSystemId);
            //if (!string.IsNullOrEmpty(location.WellKnownTextWGS84))
            //{
            //    // TODO: this fails if text is not valid!!!
            //    dbg = DbGeography.FromText(location.WellKnownTextWGS84, DbGeography.DefaultCoordinateSystemId);
            //}
            //else
            //{
            //    dbg = DbGeography.PointFromText(location.ToString(), DbGeography.DefaultCoordinateSystemId);
            //}

            return dbg;
        }

        public static Location FromGeography(this DbGeography geography)
        {
            if (null == geography)
            {
                return null;
            }
            Location location = new Location()
            {
                Latitude = geography.Latitude,
                Longitude = geography.Longitude,
                WellKnownTextWGS84 = geography.AsText()
            };

            return location;
        }

        public static MeasurementModel ToMeasurementModel(this Measurement measurement)
        {
            return ToMeasurementModel(measurement, null, null);
        }

        public static MeasurementModel ToMeasurementModel(this Measurement measurement, ICollection<Datum> data, List<string> sensorTags)
        {
            if (null == measurement)
            {
                return null;
            }
            var model = new MeasurementModel()
            {
                Key = measurement.ID.ToString("X"),
                ID = measurement.ID,
                ProviderID = measurement.ProviderID,
                Object = measurement.Object,
                Tag = measurement.Tag,
                Timestamp = measurement.Timestamp,
                TimestampISO8601 = measurement.Timestamp.ToString("o"),
                Note = measurement.Note,
                Data = new List<DataModel>()
            };
            if (null != measurement.Location)
            {
                model.Location = new Location()
                {
                    WellKnownTextWGS84 = measurement.Location.AsText(),
                    Latitude = measurement.Location.Latitude,
                    Longitude = measurement.Location.Longitude
                };
            }

            if (null != data && data.Count > 0)
            {
                var dbData = data;
                if (null != sensorTags && sensorTags.Count > 0)
                {
                    dbData = dbData.Where(d => sensorTags.Contains(d.Tag)).ToList();
                }
                foreach (var d in dbData)
                {
                    model.Data.Add(d.ToDataModel());
                }
            }
            return model;
        }

        public static Datum ToDatum(this DataModel model, Measurement measurement)
        {
            return new Datum()
            {
                Tag = model.Tag.SafeGet(TagMaxLength),
                Value = model.Value,
                LongValue = model.LongValue,
                TextValue = string.IsNullOrWhiteSpace(model.TextValue) ? null : model.TextValue,
                BinaryValue = model.BinaryValue,
                XmlValue = string.IsNullOrWhiteSpace(model.XmlValue) ? null : model.XmlValue,
                Measurement = measurement
            };
        }

        public static DataModel ToDataModel(this Datum datum)
        {
            if (null == datum)
            {
                return null;
            }

            return new DataModel()
            {
                MeasurementID = datum.MeasurementID,
                Tag = datum.Tag,
                BinaryValue = datum.BinaryValue,
                TextValue = datum.TextValue,
                Value = datum.Value,
                LongValue = datum.LongValue,
                XmlValue = datum.XmlValue
            };
        }

        public static ProviderModel ToProviderModel(this Provider provider)
        {
            if (null == provider)
            {
                return null;
            }
            var model = new ProviderModel()
            {
                ID = provider.ID,
                Created = provider.Created,
                Info = provider.Info,
                IsPublic = provider.IsPublicDomain,
                Key = provider.Key.Decrypt(),
                Name = provider.Name,
                Owner = provider.Name,
                Tag = provider.Tag,
                ActiveFrom = provider.ActiveFrom,
                ActiveTo = provider.ActiveTo,
                ContactEmail = provider.ContactEmail,
                CreatedBy = provider.CreatedBy,
                DataStorageUntil = provider.DataStorageUntil,
                Location = provider.Location.FromGeography()
            };
            if (null != provider.AccessKeys)
            {
                model.Keys = new List<AccessKeyModel>();
                foreach (var k in provider.AccessKeys)
                {
                    model.Keys.Add(k.ToAccessKeyModel(true));
                }
            }
            return model;
        }

        public static Provider ToProvider(this ProviderModel model)
        {
            return new Provider()
            {
                ID = model.ID,
                Created = model.Created,
                Info = model.Info,
                IsPublicDomain = model.IsPublic,
                Key = model.Key.Encrypt(),
                Name = model.Name.SafeGet(NameMaxLength),
                Owner = model.Owner,
                Tag = model.Tag,
                Location = model.Location.ToGeography(),
                ActiveFrom = model.ActiveFrom,
                ActiveTo = model.ActiveTo,
                ContactEmail = model.ContactEmail,
                CreatedBy = model.CreatedBy,
                DataStorageUntil = model.DataStorageUntil
            };
        }

        public static AccessKeyModel ToAccessKeyModel(this AccessKey key, bool decryptKey = false)
        {
            AccessKeyModel akm =  new AccessKeyModel()
            {
                AccessControl = (AccessControl)key.AccessControl,
                ProviderID = key.ProviderID,
                ValidFrom = key.ValidFrom,
                ValidTo = key.ValidTo,
                KeyId = key.KeyId,
                Info = key.Info
            };
            if (decryptKey)
            {
                akm.Key = key.KeyEncrypt.Decrypt(key.ProviderID.ToString());
            }

            return akm;
        }

        public static AccessKey ToAccessKey(this AccessKeyModel model)
        {
            return new AccessKey()
            {
                AccessControl = (int)model.AccessControl,
                KeyEncrypt = model.Key.Encrypt(model.ProviderID.ToString()),
                Key = model.Key.Hash(model.ProviderID.ToString()),
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo,
                ProviderID = model.ProviderID,
                KeyId = model.KeyId,
                Info = model.Info
            };
        }

        public static Query ToQuery(this MeasurementQueryModel model, AccessKeyModel access)
        {
            return new Query()
            {
                Key = access.Key.Hash(access.ProviderID.ToString()),
                ProviderID = access.ProviderID,
                Object = model.Obj,
                Tag = model.Tag,
                Take = model.Take,
                From = model.From,
                To = model.To,
                Sensors = model.Sensors,
                Name=model.Name,
                ID = model.ID

            };

        }

        public static MeasurementQueryModel ToMeasurementQueryModel(this Query model)
        {
            return new MeasurementQueryModel()
            {
                Key = "",
                Obj = model.Object,
                Tag = model.Tag,
                Take = model.Take,
                From = model.From,
                To = model.To,
                Sensors = model.Sensors,
                Name = model.Name,
                ID = model.ID
            };

        }

        public static string Hash(this string text, string salt = "")
        {
            string base64String;
#if DEBUG
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif            
            using (var mySHA256 = SHA256Managed.Create())
            {
               // var bytes = Encoding.Unicode.GetBytes(ConfigurationManager.AppSettings[DBHelper.Salt] + salt + text);
                var bytes = Encoding.Unicode.GetBytes(Common.GlobalSettings.GetSalt + salt + text);
                var hashValue = mySHA256.ComputeHash(bytes);
                base64String = System.Convert.ToBase64String(hashValue, 0, hashValue.Length);
            }
#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Hash took {0} ms.", sw.Elapsed.TotalMilliseconds);
#endif
            return base64String;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string Encrypt(this string text, string salt = "")
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            string cipherText;
            byte[] textBytes = Encoding.Unicode.GetBytes(text);
          
             using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Common.GlobalSettings.GetEncryptionKey, Encoding.Unicode.GetBytes(Common.GlobalSettings.GetSalt + salt)))
            {
                using (RijndaelManaged RMCrypto = new RijndaelManaged())
                {

                    RMCrypto.Key = pdb.GetBytes(DBHelper.KeySize / 8);
                    RMCrypto.IV = pdb.GetBytes(DBHelper.BlockSize / 8);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        ICryptoTransform encryptor = RMCrypto.CreateEncryptor(RMCrypto.Key, RMCrypto.IV);
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(textBytes, 0, textBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            byte[] cipherXmlTextBytes = memoryStream.ToArray();
                            cipherText = Convert.ToBase64String(cipherXmlTextBytes);
                        }
                    }
                }
            }
#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Encrypt took {0} ms.", sw.ElapsedMilliseconds);
#endif
            return cipherText;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string Decrypt(this string cryptedText, string salt = "")
        {
            try
            {
                byte[] cipherTextBytes = Convert.FromBase64String(cryptedText);
               
                using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Common.GlobalSettings.GetEncryptionKey, Encoding.Unicode.GetBytes(Common.GlobalSettings.GetSalt + salt)))
                {
                    using (RijndaelManaged RMCrypto = new RijndaelManaged())
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            RMCrypto.Key = pdb.GetBytes(DBHelper.KeySize / 8);
                            RMCrypto.IV = pdb.GetBytes(DBHelper.BlockSize / 8);
                            ICryptoTransform decryptor = RMCrypto.CreateDecryptor(RMCrypto.Key, RMCrypto.IV);
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] xmlPlainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(xmlPlainTextBytes, 0, xmlPlainTextBytes.Length);
                                string plainText = Encoding.Unicode.GetString(xmlPlainTextBytes, 0, decryptedByteCount);
                                return plainText;
                            }
                        }
                    }
                }
            }
            catch (CryptographicException cex)
            {
                // cryptedText was not crypted, return as is
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(cex, null);
            }
            catch (FormatException fex)
            {
                // cryptedText was not proper base64, return as is
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(fex, null);
            }
            catch (ArgumentNullException anex)
            {
                // cryptedText was not proper base64, return as is
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(anex, null);
            }
            return cryptedText;
        }

        public static SensorModel ToSensorModel(this Sensor sensor)
        {
            return new SensorModel()
            {
                ProviderID = sensor.ProviderID,
                Description = sensor.Description,
                Name = sensor.Name,
                Tag = sensor.Tag,
                Unit = sensor.Unit,
                ValueDecimalCount = sensor.Rounding,
                Location = sensor.Location.FromGeography()
            };
        }

        public static Sensor ToSensor(this SensorModel model, int providerID)
        {
            return new Sensor()
            {
                ProviderID = providerID,
                Description = model.Description,
                Name = model.Name.SafeGet(NameMaxLength),
                Rounding = model.ValueDecimalCount,
                Tag = model.Tag.SafeGet(TagMaxLength),
                Unit = model.Unit.SafeGet(TagMaxLength),
                Location = model.Location.ToGeography(),
            };
        }

        public static string GenerateKey(string prefix = "", string postfix = "")
        {
            int maxLength = 50;
            long ticks = DateTime.Now.Ticks;
            Random rnd = new Random();
            int r = rnd.Next(101, 1000);
            ticks = ticks / r;
            string newKey = string.Format("{0}{1}{2}", prefix, ticks.ToString("X"), postfix);
            if (newKey.Length > maxLength)
            {
                return newKey.Substring(0, maxLength);
            }
            return newKey;
        }

        /// <summary>
        /// Checks if entity already exists and adds it if not.
        /// </summary>
        /// <typeparam name="T">entity type</typeparam>
        /// <param name="dbSet">entity context</param>
        /// <param name="entity">entity that will be put to added state if conditions meet</param>
        /// <param name="predicate">Expression function</param>
        /// <returns></returns>
        public static T AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }
    }
}