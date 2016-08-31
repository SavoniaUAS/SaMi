using Savonia.Measurements.Manager.Models;
using Savonia.Measurements.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CsvHelper;
using System.Net.Mime;

namespace Savonia.Measurements.Manager.Helpers
{
    /// <summary>
    /// This class is used to create and export csv-files
    /// </summary>
    public class CsvExportHelper
    {
        /// <summary>
        /// Data which is used to write on csv-file
        /// </summary>
        private List<MeasurementModel> _measurements = new List<MeasurementModel>();
        /// <summary>
        /// index of measurement on list
        /// </summary>
        private int mIndex = 0;
        /// <summary>
        /// index of data on list 
        /// </summary>
        private int dIndex = 0;

        /// <summary>
        /// Creates CSV-file to given path using Exportmodel as configurations and measurementModels as data.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="options"></param>
        /// <param name="measurements"></param>
        public void CreateCsvFile(string filename, string directoryPath, ExportModel options, List<MeasurementModel> measurements)
        {
            _measurements = measurements;
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (TextWriter tw = new StreamWriter(directoryPath + filename))
                {

                    using (var writer = new CsvHelper.CsvWriter(tw))
                    {
                        if (!string.IsNullOrEmpty(options.ColumnDelimeter))
                        {
                            writer.Configuration.Delimiter = options.ColumnDelimeter;
                        }
                        writer.Configuration.HasHeaderRecord = options.ShowHeaders;
                        var selected = CreateSelectedValueList(options);
                        if (options.ShowHeaders)
                        {
                            var headers = CreateHeaders(measurements, selected);
                            foreach (var h in headers)
                            {
                                writer.WriteField(h);
                            }
                            writer.NextRecord();
                        }
                        var functions = CreateDataValueFunctions(selected);

                        for (mIndex = 0; mIndex < measurements.Count; mIndex++)
                        {
                            writer.WriteField(measurements[mIndex].Object);
                            writer.WriteField(measurements[mIndex].Tag);
                            writer.WriteField(measurements[mIndex].Timestamp.ToString(options.Datetimeformat));
                            writer.WriteField(measurements[mIndex] != null ? measurements[mIndex].Location?.ToString() : "");
                            writer.WriteField(measurements[mIndex].Note);

                            for (dIndex = 0; dIndex < measurements[mIndex].Data.Count; dIndex++)
                            {
                                foreach (var f in functions)
                                {
                                    var result = f.Invoke();
                                    writer.WriteField((object)result ?? (object)string.Empty);
                                }
                            }
                            writer.NextRecord();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates list of actions based on list values.
        /// </summary>
        /// <returns></returns>
        private List<Func<object>> CreateDataValueFunctions(List<string> selected)
        {
            List<Func<object>> functions = new List<Func<object>>();

            foreach (var item in selected)
            {
                switch (item)
                {
                    case "Value":
                        functions.Add(new Func<object>(() => GetValue()));
                        break;
                    case "LongValue":
                        functions.Add(new Func<object>(() => GetLongValue()));
                        break;
                    case "TextValue":
                        functions.Add(new Func<object>(() => GetTextValue()));
                        break;
                    case "BinaryValue":
                        functions.Add(new Func<object>(() => GetBinaryValue()));
                        break;
                    case "XmlValue":
                        functions.Add(new Func<object>(() => GetXmlValue()));
                        break;
                    default:
                        break;
                }

            }
            return functions;
        }

        private object GetValue()
        {
            return _measurements[mIndex].Data[dIndex].Value;
        }

        private object GetLongValue()
        {
            return _measurements[mIndex].Data[dIndex].LongValue;
        }

        private object GetTextValue()
        {
            return _measurements[mIndex].Data[dIndex].TextValue;
        }
        private object GetBinaryValue()
        {
            return _measurements[mIndex].Data[dIndex].BinaryValue;
        }

        private object GetXmlValue()
        {
            return _measurements[mIndex].Data[dIndex].XmlValue;
        }
        /// <summary>
        /// Creates list of data values which will be used on csv file.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private List<string> CreateSelectedValueList(ExportModel options)
        {
            List<string> selected = new List<string>();
            foreach (var item in options.DataValues.GetType().GetProperties())
            {
                var value = item.GetValue(options.DataValues);
                if (value != null && value.GetType() == typeof(bool) && (bool)value == true)
                {
                    selected.Add(item.Name);
                }
            }
            return selected;
        }

        /// <summary>
        /// Creates list of headers.
        /// </summary>
        /// <param name="measurements"></param>
        /// <param name="selectedDataValues"></param>
        /// <returns></returns>
        public List<string> CreateHeaders(List<MeasurementModel> measurements, List<string> selectedDataValues)
        {
            List<string> headers = new List<string>();

            foreach (var m in measurements)
            {
                foreach (var d in m.Data)
                {
                    foreach (var v in selectedDataValues)
                    {
                        headers.Add(d.Tag + " (" + v + ")");
                    }
                }
                break;
            }
            headers.InsertRange(0, new List<string> { "Object", "Tag", "Timestamp", "Location", "Note" });

            return headers;
        }

        //http://stackoverflow.com/questions/5826649/returning-a-file-to-view-download-in-asp-net-mvc
        //http://www.rfc-editor.org/rfc/rfc6266.txt  look example to add optional filename*-parameter to encode non-ISO-8859-1 characters
        /// <summary>
        /// Gets file size of file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Nullable<long> GetFileSize(string path, string filename)
        {
            Nullable<long> size = null;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fiArr = di.GetFiles();
            var infoFile = fiArr.SingleOrDefault(i => i.Name == filename);
            if (infoFile != null)
            {
                var fileLenght = infoFile?.Length;
                if (fileLenght != null)
                {
                    size = fileLenght;
                }
            }
            return size;
        }

        /// <summary>
        /// Creates MIME-protocols Content-disposition header.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="inline"></param>
        /// <returns></returns>
        public ContentDisposition CreateContentDisposition(string filename, bool inline = false)
        {
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = inline
            };
            return cd;
        }

        /// <summary>
        /// Creates MIME-protocols Content-disposition header.
        /// </summary>
        /// <param name="filename">Name which will be added to header</param>
        /// <param name="file">physical file name</param>
        /// <param name="path">path to physical file</param>
        /// <param name="inline">inline or not</param>
        /// <returns></returns>
        public ContentDisposition CreateContentDisposition(string filename, string file, string path, bool inline = false)
        {
            var size = GetFileSize(path, file);
            if (!size.HasValue)
            {
                return CreateContentDisposition(filename, inline);
            }
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = inline,
                Size = size.Value
            };
            return cd;
        }
    }
}
