using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestClients.MeasurementsService;

namespace TestClients
{
    public static class TestHelper
    {
        public const string TestKey = "Your test key";
        public const string TestMeasurementObject = "TestMeasurement";
        public const string TestDataTag = "test";

        
        // json uri for service in Savonia.Measurements.Services project
       public const string JSONSaveMeasurementPackageUri = "http://localhost:50240/MeasurementsService.svc/json/measurementpackage/save";

        // json uri for service in sami.savonia.fi/Service
       // public const string JSONSaveMeasurementPackageUri = "YOURDOMAIN/Service/1.0/MeasurementsService.svc/json/measurementpackage/save";

        public static MeasurementPackage GetMeasurementPackage(this DateTimeOffset measurementTime, string measurementTag)
        {
            MeasurementPackage package = new MeasurementPackage();
            package.Key = TestHelper.TestKey;

            List<MeasurementModel> measurements = new List<MeasurementModel>();
            MeasurementModel m = new MeasurementModel()
            {
                Object = TestHelper.TestMeasurementObject,
                Tag = measurementTag,
                Timestamp = measurementTime,
                Location = new Location()
                {
                    Latitude = 62.8989,
                    Longitude = 27.6630
                }
            };
            DataModel d = new DataModel()
            {
                Tag = TestHelper.TestDataTag,
                Value = 3.14
            };
            m.Data = new DataModel[1];
            m.Data[0] = d;
            measurements.Add(m);

            package.Measurements = measurements.ToArray();

            return package;
        }

        public static MeasurementPackage GetMeasurementPackageWithISO(this DateTime measurementTime, string measurementTag)
        {
            MeasurementPackage package = new MeasurementPackage();
            package.Key = TestHelper.TestKey;

            List<MeasurementModel> measurements = new List<MeasurementModel>();
            // string ISO = measurementTime.ToString();
            string ISO = "11/02/03 15:03:06";
            MeasurementModel m = new MeasurementModel()
            {
                Object = TestHelper.TestMeasurementObject,
                Tag = measurementTag,
                TimestampISO8601 = ISO,
                Location = new Location()
                {
                    Latitude = 62.8989,
                    Longitude = 27.6630
                }
            };
           
            DataModel d = new DataModel()
            {
                Tag = TestHelper.TestDataTag,
                Value = 3.14
            };
            m.Data = new DataModel[1];
            m.Data[0] = d;
            measurements.Add(m);
            measurements.Add(m);

            package.Measurements = measurements.ToArray();

            return package;
        }

        public static MeasurementPackage GetMeasurementPackageWithXMeasurementsAndYData(this DateTimeOffset measurementTime, string measurementTag, int measurementsCount, int dataCount, bool useLocation)
        {
            MeasurementPackage package = new MeasurementPackage();
            package.Key = TestHelper.TestKey;

            if (dataCount < 1)
            {
                dataCount = 1;
            }

            List<MeasurementModel> measurements = new List<MeasurementModel>();
            MeasurementModel m;
            for (int i = 0; i < measurementsCount; i++)
            {

                m = new MeasurementModel()
                {
                    Object = TestHelper.TestMeasurementObject + i.ToString(),
                    Tag = measurementTag + i.ToString(),
                    Timestamp = measurementTime.AddSeconds(i)
                };
                if (useLocation)
                {
                    m.Location = new Location()
                    {
                        Latitude = 62.8989,
                        Longitude = 27.6630
                    };
                }
                m.Data = new DataModel[dataCount];
                for (int j = 0; j < dataCount; j++)
                {
                    DataModel d = new DataModel()
                    {
                        Tag = TestHelper.TestDataTag + j.ToString(),
                        Value = 3.14
                    };
                    m.Data[j] = d;
                }
                measurements.Add(m);
            }
            package.Measurements = measurements.ToArray();

            return package;
        }

        public static MeasurementPackage GetMeasurementPackageWithXMeasurementsAndYDataWithTextValue(this DateTimeOffset measurementTime, string measurementTag, int measurementsCount, int dataCount, bool useLocation)
        {
            MeasurementPackage package = new MeasurementPackage();
            package.Key = TestHelper.TestKey;

            if (dataCount < 1)
            {
                dataCount = 1;
            }

            List<MeasurementModel> measurements = new List<MeasurementModel>();
            MeasurementModel m;
            for (int i = 0; i < measurementsCount; i++)
            {

                m = new MeasurementModel()
                {
                    Object = TestHelper.TestMeasurementObject,
                    Tag = measurementTag,
                    Timestamp = measurementTime.AddSeconds(i)
                };
                if (useLocation)
                {
                    m.Location = new Location()
                    {
                        Latitude = 62.8989,
                        Longitude = 27.6630
                    };
                }
                m.Data = new DataModel[dataCount];
                //var xml = Savonia.Measurements.Providers.Models.SerializeHelper.SerializeToString<MeasurementModel>(m);
                for (int j = 0; j < dataCount; j++)
                {
                    DataModel d = new DataModel()
                    {
                        Tag = TestHelper.TestDataTag + j.ToString(),
                        Value = 3.14,
                        TextValue = "text_" + i.ToString()
                    };
                    m.Data[j] = d;
                }
                measurements.Add(m);
            }
            package.Measurements = measurements.ToArray();

            return package;
        }


        public static List<Savonia.Measurements.Models.MeasurementModel> GetMeasurementMeasurements(this DateTimeOffset measurementTime, string measurementTag, int measurementsCount, int dataCount, bool useLocation)
        {
            

            List<Savonia.Measurements.Models.MeasurementModel> measurements = new List<Savonia.Measurements.Models.MeasurementModel>();
            Savonia.Measurements.Models.MeasurementModel m;
            for (int i = 0; i < measurementsCount; i++)
            {

                m = new Savonia.Measurements.Models.MeasurementModel()
                {
                    Object = TestHelper.TestMeasurementObject + i.ToString(),
                    Tag = measurementTag + i.ToString(),
                    Timestamp = measurementTime.AddSeconds(i)
                };
                if (useLocation)
                {
                    m.Location = new Savonia.Measurements.Models.Location()
                    {
                        Latitude = 62.8989,
                        Longitude = 27.6630
                    };
                }
                m.Data =new List< Savonia.Measurements.Models.DataModel>();
                for (int j = 0; j < dataCount; j++)
                {
                    Savonia.Measurements.Models.DataModel d = new Savonia.Measurements.Models.DataModel()
                    {
                        Tag = TestHelper.TestDataTag + j.ToString(),
                        Value = 3.14
                    };
                    m.Data.Add(d);
                }
                measurements.Add(m);
            }
            

            return measurements;
        }

    }
}
