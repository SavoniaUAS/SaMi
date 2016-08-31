using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Savonia.Measurements.Database.Helpers;
using Savonia.Measurements.Models;
using Savonia.Measurements.Models.Helpers;
using Savonia.Measurements.Database.Models;
using Savonia.Measurements.Common;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading;

namespace Savonia.Measurements.Database
{
    /// <summary>
    /// Database repository contains logic to interact with the measurements database.
    /// </summary>
    public class Repository : IDisposable
    {
        private SavoniaMeasurementsV2Entities _db;
        private int? forcedThreadsCount = null;
        /// <summary>
        /// Tested threshold value
        /// </summary>
        private int parallelizationThreshold = 100;

        public Repository() { }

        public Repository(bool loadConfig)
        {
            if (loadConfig)
            {
                this.forcedThreadsCount = GlobalSettings.ParallelizationThreadsCount;
                this.parallelizationThreshold = GlobalSettings.ParallelizationThreshold;
            }
        }

        /// <summary>
        /// Saves measurements async
        /// </summary>
        /// <param name="key"></param>
        /// <param name="measurements"></param>
        /// <returns></returns>
        public async Task<SaveResult> SaveMeasurementsAsync(string key, params MeasurementModel[] measurements)
        {
            if (null == measurements)
            {
                throw new ArgumentNullException("Invalid measurements. Measurements is null and it contains no measurements. Data was not saved.");
            }

            var access = await GetProviderAccessAsync(key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data was not saved.");
            }
            if (!access.AccessAllowed(AccessControl.Write))
            {
                throw new UnauthorizedAccessException("Saving data with specified key is not allowed. Data was not saved.");
            }
            // TODO: should propably check if provider is valid???
            var result = await SaveMeasurementsAsyncOrParallel(access.ProviderID, access.KeyId, measurements).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Saves measurements async or parallel depending on parallezationThreshold
        /// </summary>
        /// <param name="providerID"></param>
        /// <param name="keyId"></param>
        /// <param name="measurements"></param>
        /// <returns></returns>
        private async Task<SaveResult> SaveMeasurementsAsyncOrParallel(int providerID, short? keyId, params MeasurementModel[] measurements)
        {
            SaveResult result = null;
            if (measurements.Length < this.parallelizationThreshold)
            {
                result = await this.PersistMeasurementsAsync(providerID, keyId, measurements).ConfigureAwait(false);
            }
            else
            {
                result = this.PersistMeasurementsParallel(providerID, keyId, measurements);
            }
            // speedup EF bulk saves: http://weblog.west-wind.com/posts/2013/Dec/22/Entity-Framework-and-slow-bulk-INSERTs
            //(taskin wait)
            result.PopulateOverallSuccessStatus();
            // TODO: make error reporting better!
            //if (!result.Success && null != result.Failures)
            //{
            //    Task.Run(() =>
            //    {
            //        var exceptions = result.Failures.Where(f => null != f.Exception).Select(f => f.Exception);
            //        Parallel.ForEach(exceptions, (ex) =>
            //        {
            //            Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, null);
            //        });
            //    });
            //}

            return result;
        }

        /// <summary>
        /// Adds measurements async to database
        /// </summary>
        /// <param name="providerID"></param>
        /// <param name="keyId"></param>
        /// <param name="measurements"></param>
        /// <returns></returns>
        private async Task<SaveResult> PersistMeasurementsAsync(int providerID, short? keyId, params MeasurementModel[] measurements)
        {
            SaveResult result = new SaveResult();
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                Measurement meas;
                int index = 0;
                foreach (var m in measurements)
                {
                    meas = m.ToMeasurement(providerID, keyId);
                    //jos on uusia tageja, objecteja jne niin lisätään meta tauluun
                    db.Measurements.Add(meas);
                    if (null != m.Data)
                    {
                        foreach (var d in m.Data)
                        {
                            db.Data.Add(d.ToDatum(meas));
                        }
                    }

                    try
                    {
                        int changesCount = await db.SaveChangesAsync().ConfigureAwait(false);
                        // changesCount should have one (1) for the measurement + one for each data in that measurement.
                        if (changesCount == 1 + m.Data.Count)
                        {
                            // save was successfull
                        }
                        else
                        {
                            // save failed --> add failure
                            result.AddFailure(new Failure()
                            {
                                Index = index,
                                Reason = string.Format("Save count was {0} when it should have been {1}.", changesCount, 1 + m.Data.Count)
                            });
                        }
                    }
                    catch (DbUpdateException dbex)
                    {
                        result.AddFailure(new Failure()
                        {
                            Index = index,
                            Code = dbex.HResult,
                            Reason = "DB update failed. Check your data for duplicate Data tag values. " + dbex.FriendlyException(),
                            Exception = dbex
                        });
                    }
                    catch (Exception ex)
                    {
                        result.AddFailure(new Failure()
                        {
                            Index = index,
                            Code = ex.HResult,
                            Reason = ex.FriendlyException(),
                            Exception = ex
                        });

                    }
                    index++;
                }
            }
            return result;
        }

        /// <summary>
        /// Adds measurements parallel to database
        /// </summary>
        /// <param name="providerID"></param>
        /// <param name="keyId"></param>
        /// <param name="measurements"></param>
        /// <returns></returns>
        private SaveResult PersistMeasurementsParallel(int providerID, short? keyId, params MeasurementModel[] measurements)
        {
            BlockingCollection<Failure> failures = new BlockingCollection<Failure>(measurements.Length);

            Parallel.ForEach(measurements, (m, state, i) =>
            {
                using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
                {
                    db.Configuration.AutoDetectChangesEnabled = false;

                    Measurement meas;
                    meas = m.ToMeasurement(providerID, keyId);

                    db.Measurements.Add(meas);
                    if (null != m.Data)
                    {
                        foreach (var d in m.Data)
                        {
                            db.Data.Add(d.ToDatum(meas));
                        }
                    }

                    try
                    {
                        int changesCount = db.SaveChanges();
                        // changesCount should have one (1) for the measurement + one for each data in that measurement.
                        if (changesCount == 1 + m.Data.Count)
                        {
                            // save was successfull
                        }
                        else
                        {
                            // save failed --> add failure
                            failures.Add(new Failure()
                            {
                                Index = (int)i,
                                Reason = string.Format("Save count was {0} when it should have been {1}.", changesCount, 1 + m.Data.Count)
                            });
                        }
                    }
                    catch (DbUpdateException dbex)
                    {
                        failures.Add(new Failure()
                        {
                            Index = (int)i,
                            Code = dbex.HResult,
                            Reason = "DB update failed. Check your data for duplicate Data tag values. " + dbex.FriendlyException(),
                            Exception = dbex
                        });
                    }
                    catch (Exception ex)
                    {
                        failures.Add(new Failure()
                        {
                            Index = (int)i,
                            Code = ex.HResult,
                            Reason = ex.FriendlyException(),
                            Exception = ex
                        });
                    }
                }
            });
            SaveResult result = new SaveResult();
            if (failures.Count > 0)
            {
                result.Failures = failures.ToList();
            }
            return result;
        }


        public async Task<IQueryable<MeasurementModel>> GetMeasurementsAsync(string key)
        {
            var access = await GetProviderAccessAsync(key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be read.");
            }
            if (!access.AccessAllowed(AccessControl.Read))
            {
                throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
            }

            if (null == _db)
            {
                _db = new SavoniaMeasurementsV2Entities();
            }

            var measurements = new List<MeasurementModel>();
            var dbMeas = await _db.Measurements.Where(m => m.ProviderID == access.ProviderID).ToListAsync().ConfigureAwait(false);
            foreach (var m in dbMeas)
            {
                measurements.Add(m.ToMeasurementModel(m.Data, null));
            }

            return measurements.AsQueryable();
        }

        public async Task<List<MeasurementModel>> GetMeasurementsAsync(MeasurementQueryModel query, bool populateData = false)
        {
            if (null == query)
            {
                throw new ArgumentNullException("query");
            }
            var access = await GetProviderAccessAsync(query.Key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be read.");
            }
            if (!access.AccessAllowed(AccessControl.Read))
            {
                throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
            }
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;

                IQueryable<Measurement> measurements;
                measurements = db.Measurements.Where(m => m.ProviderID == access.ProviderID);
                if (!string.IsNullOrEmpty(query.Obj))
                {
                    measurements = measurements.Where(m => m.Object == query.Obj);
                }
                if (!string.IsNullOrEmpty(query.Tag))
                {
                    measurements = measurements.Where(m => m.Tag == query.Tag);
                }
                if (query.From.HasValue)
                {
                    if (query.InclusiveFrom)
                    {
                        measurements = measurements.Where(m => m.Timestamp >= query.From.Value);
                    }
                    else
                    {
                        measurements = measurements.Where(m => m.Timestamp > query.From.Value);
                    }
                }
                if (query.To.HasValue)
                {
                    if (query.InclusiveTo)
                    {
                        measurements = measurements.Where(m => m.Timestamp <= query.To.Value);
                    }
                    else
                    {
                        measurements = measurements.Where(m => m.Timestamp < query.To.Value);
                    }
                }
#if DEBUG
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif
                measurements = measurements.OrderByDescending(m => m.Timestamp);
                if (query.Take.HasValue)
                {
                    measurements = measurements.Take(query.Take.Value);
                }
                // get measurements
                var dbMeas = await measurements.ToListAsync().ConfigureAwait(false);
                ConcurrentDictionary<long, MeasurementModel> measurementsDictionary = new ConcurrentDictionary<long, MeasurementModel>();
                var dataTags = query.DataTags();

                if (populateData)
                {
                    // data was also requested. Read data with data reader in chuncks to speed up data reading and modeling
                    OrderablePartitioner<Measurement> partitioner = Partitioner.Create(dbMeas);
                    var measurementsCount = dbMeas.Count;
                    int partitionsCount = (int)Math.Ceiling((double)measurementsCount / 10000);
                    partitionsCount = Math.Max(partitionsCount, 1); // minimum of 1 partition is needed

                    //var partitions = partitioner.GetPartitions(Environment.ProcessorCount);
                    var partitions = partitioner.GetPartitions(partitionsCount);
                    Task[] tasks = partitions.Select(p => Task.Run(() =>
                    {
                        using (p)
                        {
                            List<long> ids = new List<long>();
                            while (p.MoveNext())
                            {
                                var m = p.Current;
                                measurementsDictionary.TryAdd(m.ID, m.ToMeasurementModel(null, null));
                                ids.Add(m.ID);
                            }

                            if (ids.Count > 0)
                            {
                                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                                {
                                    string sql;
                                    // compose sql query to read all data values for given measurements.
                                    SqlCommand command = new SqlCommand();
                                    sql = string.Format("Select MeasurementID, Tag, Value, LongValue, TextValue, BinaryValue, XmlValue from Data where MeasurementID IN ({0})", string.Join(",", ids));

                                    //Data tags are parametrisized here
                                    if (dataTags?.Count > 0)
                                    {
                                        string stringIDs = string.Join(",", ids);
                                        var parameters = CreateSqlParameters(dataTags);
                                        sql = AppendSqlStringUsingParameters(sql, parameters, "Tag");
                                        command.Parameters.AddRange(parameters);
                                    }

                                    command.CommandText = sql;
                                    command.Connection = connection;
                                    connection.Open();

                                    SqlDataReader reader = command.ExecuteReader();
                                    DataModel d;
                                    if (reader.HasRows)
                                    {
                                        string tag;
                                        long mid;
                                        if (null != dataTags && dataTags.Count > 0)
                                        {
                                            //tässä mahdollinen sort 
                                            while (reader.Read())
                                            {
                                                tag = reader.GetString(1);
                                                if (dataTags.Contains(tag))
                                                {
                                                    mid = reader.GetInt64(0);
                                                    d = new DataModel();
                                                    d.MeasurementID = mid;
                                                    d.Tag = tag;
                                                    d.Value = reader.IsDBNull(2) ? default(Nullable<double>) : reader.GetDouble(2);
                                                    d.LongValue = reader.IsDBNull(3) ? default(Nullable<long>) : reader.GetInt64(3);
                                                    d.TextValue = reader.IsDBNull(4) ? null : reader.GetString(4);
                                                    d.BinaryValue = reader.IsDBNull(5) ? null : (byte[])reader[5];
                                                    d.XmlValue = reader.IsDBNull(6) ? null : reader.GetString(6);
                                                    measurementsDictionary[mid].Data.Add(d);
                                                }
                                                //Tähän mahdollinen sort data tagsien syöttöjärjestyksessä
                                            }
                                        }
                                        else
                                        {
                                            while (reader.Read())
                                            {
                                                mid = reader.GetInt64(0);
                                                tag = reader.GetString(1);
                                                d = new DataModel();
                                                d.MeasurementID = mid;
                                                d.Tag = tag;
                                                d.Value = reader.IsDBNull(2) ? default(Nullable<double>) : reader.GetDouble(2);
                                                d.LongValue = reader.IsDBNull(3) ? default(Nullable<long>) : reader.GetInt64(3);
                                                d.TextValue = reader.IsDBNull(4) ? null : reader.GetString(4);
                                                d.BinaryValue = reader.IsDBNull(5) ? null : (byte[])reader[5];
                                                d.XmlValue = reader.IsDBNull(6) ? null : reader.GetString(6);
                                                measurementsDictionary[mid].Data.Add(d);
                                            }
                                        }
                                    }
                                    reader.Close();
                                }
                            }
                        }
                    })).ToArray();
                    // wait for all partition tasks to finish
                    Task.WaitAll(tasks);
                }
                else//Asordered?
                {

                    // data was not requested, populate only measurement values
                    Parallel.ForEach(dbMeas, m =>
                    {
                        measurementsDictionary.TryAdd(m.ID, m.ToMeasurementModel(null, null));
                    });
                }
#if DEBUG
                sw.Stop();
                System.Diagnostics.Debug.WriteLine("It took {0} ms to get the measurements", sw.ElapsedMilliseconds);
#endif
                // results are sorted here to descending order by measurement time
                return measurementsDictionary.Values.OrderByDescending(m => m.Timestamp).ToList();
            }
        }

        /// <summary>
        /// Gets measurements async 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancel"></param>
        /// <param name="populateData"></param>
        /// <returns></returns>
        public async Task<List<MeasurementModel>> GetMeasurementsAsync(MeasurementQueryModel query, CancellationToken cancel, bool populateData = false)
        {
            if (null == query)
            {
                throw new ArgumentNullException("query");
            }
            var access = await GetProviderAccessAsync(query.Key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be read.");
            }
            if (!access.AccessAllowed(AccessControl.Read))
            {
                throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
            }
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;

                IQueryable<Measurement> measurements;
                measurements = db.Measurements.Where(m => m.ProviderID == access.ProviderID);
                if (!string.IsNullOrEmpty(query.Obj))
                {
                    measurements = measurements.Where(m => m.Object == query.Obj);
                }
                if (!string.IsNullOrEmpty(query.Tag))
                {
                    measurements = measurements.Where(m => m.Tag == query.Tag);
                }
                if (query.From.HasValue)
                {
                    if (query.InclusiveFrom)
                    {
                        measurements = measurements.Where(m => m.Timestamp >= query.From.Value);
                    }
                    else
                    {
                        measurements = measurements.Where(m => m.Timestamp > query.From.Value);
                    }
                }
                if (query.To.HasValue)
                {
                    if (query.InclusiveTo)
                    {
                        measurements = measurements.Where(m => m.Timestamp <= query.To.Value);
                    }
                    else
                    {
                        measurements = measurements.Where(m => m.Timestamp < query.To.Value);
                    }
                }
#if DEBUG
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif
                measurements = measurements.OrderByDescending(m => m.Timestamp);
                if (query.Take.HasValue)
                {
                    measurements = measurements.Take(query.Take.Value);
                }
                // get measurements
                var dbMeas = await measurements.ToListAsync(cancel).ConfigureAwait(false);
                ConcurrentDictionary<long, MeasurementModel> measurementsDictionary = new ConcurrentDictionary<long, MeasurementModel>();
                var dataTags = query.DataTags();

                if (populateData)
                {
                    // data was also requested. Read data with data reader in chuncks to speed up data reading and modeling
                    OrderablePartitioner<Measurement> partitioner = Partitioner.Create(dbMeas);
                    var measurementsCount = dbMeas.Count;
                    int partitionsCount = (int)Math.Ceiling((double)measurementsCount / 10000);
                    partitionsCount = Math.Max(partitionsCount, 1); // minimum of 1 partition is needed

                    //var partitions = partitioner.GetPartitions(Environment.ProcessorCount);
                    var partitions = partitioner.GetPartitions(partitionsCount);
                    Task[] tasks = partitions.Select(p => Task.Run(() =>
                    {
                        using (p)
                        {
                            List<long> ids = new List<long>();
                            while (p.MoveNext())
                            {
                                var m = p.Current;
                                measurementsDictionary.TryAdd(m.ID, m.ToMeasurementModel(null, null));
                                ids.Add(m.ID);
                            }

                            if (ids.Count > 0)
                            {
                                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                                {
                                    string sql;
                                    // compose sql query to read all data values for given measurements.
                                    // TODO: could we read only those tags that the user specifies without sql injection risk? MeasurementID + Tag is the primary key in Data table.
                                    SqlCommand command = new SqlCommand();
                                    sql = string.Format("Select MeasurementID, Tag, Value, LongValue, TextValue, BinaryValue, XmlValue from Data where MeasurementID IN ({0})", string.Join(",", ids));

                                    //Data tags are parametrisized here
                                    if (dataTags?.Count > 0)
                                    {
                                        string stringIDs = string.Join(",", ids);
                                        var parameters = CreateSqlParameters(dataTags);
                                        sql = AppendSqlStringUsingParameters(sql, parameters, "Tag");
                                        command.Parameters.AddRange(parameters);
                                    }

                                    command.CommandText = sql;
                                    command.Connection = connection;
                                    connection.Open();

                                    SqlDataReader reader = command.ExecuteReader();
                                    DataModel d;
                                    if (reader.HasRows)
                                    {
                                        string tag;
                                        long mid;
                                        if (null != dataTags && dataTags.Count > 0)
                                        {
                                            //TODO: Possible asc sort here
                                            while (reader.Read())
                                            {
                                                tag = reader.GetString(1);
                                                if (dataTags.Contains(tag))
                                                {
                                                    mid = reader.GetInt64(0);
                                                    d = new DataModel();
                                                    d.MeasurementID = mid;
                                                    d.Tag = tag;
                                                    d.Value = reader.IsDBNull(2) ? default(Nullable<double>) : reader.GetDouble(2);
                                                    d.LongValue = reader.IsDBNull(3) ? default(Nullable<long>) : reader.GetInt64(3);
                                                    d.TextValue = reader.IsDBNull(4) ? null : reader.GetString(4);
                                                    d.BinaryValue = reader.IsDBNull(5) ? null : (byte[])reader[5];
                                                    d.XmlValue = reader.IsDBNull(6) ? null : reader.GetString(6);
                                                    measurementsDictionary[mid].Data.Add(d);
                                                }
                                                //TODO: Possible asc sort here
                                            }
                                        }
                                        else
                                        {
                                            while (reader.Read())
                                            {
                                                mid = reader.GetInt64(0);
                                                tag = reader.GetString(1);
                                                d = new DataModel();
                                                d.MeasurementID = mid;
                                                d.Tag = tag;
                                                d.Value = reader.IsDBNull(2) ? default(Nullable<double>) : reader.GetDouble(2);
                                                d.LongValue = reader.IsDBNull(3) ? default(Nullable<long>) : reader.GetInt64(3);
                                                d.TextValue = reader.IsDBNull(4) ? null : reader.GetString(4);
                                                d.BinaryValue = reader.IsDBNull(5) ? null : (byte[])reader[5];
                                                d.XmlValue = reader.IsDBNull(6) ? null : reader.GetString(6);
                                                measurementsDictionary[mid].Data.Add(d);
                                            }
                                        }
                                    }
                                    reader.Close();
                                }
                            }
                        }
                    }, cancel)).ToArray();
                    // wait for all partition tasks to finish
                    Task.WaitAll(tasks);
                }
                else//Asordered?
                {
                    // data was not requested, populate only measurement values
                    Parallel.ForEach(dbMeas, m =>
                    {
                        measurementsDictionary.TryAdd(m.ID, m.ToMeasurementModel(null, null));
                    });
                }
#if DEBUG
                sw.Stop();
                System.Diagnostics.Debug.WriteLine("It took {0} ms to get the measurements", sw.ElapsedMilliseconds);
#endif
                // results are sorted here to descending order by measurement time
                return measurementsDictionary.Values.OrderByDescending(m => m.Timestamp).ToList();
            }
        }

        /// <summary>
        /// Saves new query async if model is valid (Must be AccessKey [CANNOT be ProviderKey]). Model needs name and valid key. Returns ID of inserted query (or -1 if saving was tried with masterkey).
        /// </summary>
        /// <param name="query"></param>
        public async Task<int> SaveQueryAsync(MeasurementQueryModel query)
        {
            if (query.Name == null)
            {
                return 0;
            }
            AccessKeyModel keyModel = AccessKeyModel.FromKey(query.Key);
            int result = 0;
            if (keyModel.ProviderID > 0)
            {
                var q = query.ToQuery(keyModel);
                try
                {
                    using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
                    {
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            SqlCommand command = new SqlCommand();
                            var sql = string.Format("INSERT INTO Query (ProviderID,[Key],Object,Tag,Take,[From],[To],Sensors,Name) Values (@ProviderID0,@Key0,@Object0,@Tag0,@Take0,@From0,@To0,@Sensors0,@Name0) SELECT SCOPE_IDENTITY()");
                            List<Query> lq = new List<Query>();
                            lq.Add(q);
                            List<string> exclude = new List<string>();
                            exclude.Add(nameof(q.AccessKey));
                            var parameters = CreateSQLParameters<Query>(lq, exclude);
                            command.Parameters.AddRange(parameters.ToArray());
                            command.CommandText = sql;
                            command.Connection = connection;
                            connection.Open();
                            var r = await command.ExecuteScalarAsync();
                            result = int.Parse(r.ToString());
                            connection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// Deletes specified query by id, providerID and key of measurementQueryModel. Returns result how many rows were affected. 
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<int> DeleteQueryAsync(MeasurementQueryModel queryModel)
        {

            AccessKeyModel keyModel = AccessKeyModel.FromKey(queryModel?.Key);
            int result = 0;
            if (keyModel?.ProviderID > 0)
            {
                try
                {
                    var query = queryModel.ToQuery(keyModel);
                    using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
                    {
                        var q = await db.Queries.FindAsync(query.ID).ConfigureAwait(false);
                        if (q?.ProviderID == query.ProviderID && q.Key == query.Key)
                        {
                            db.Queries.Remove(q);
                            result = await db.SaveChangesAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            throw new ArgumentNullException("Id, providerID and key does not match row in Queries table");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return result;
        }
        /// <summary>
        /// Gets Queries with given accesskey. Returns null if no queries are found from the DB.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<MeasurementQueryModel> GetQueries(string key)
        {
            List<MeasurementQueryModel> models = new List<MeasurementQueryModel>();
            AccessKeyModel keyModel = AccessKeyModel.FromKey(key);
            key = keyModel.Key;

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                if (keyModel.ProviderID > 0)
                {
                    var ek = keyModel.Key.Hash(keyModel.ProviderID.ToString());
                    var queries = db.Queries.Where(q => q.ProviderID == keyModel.ProviderID && q.Key == ek).ToList();

                    foreach (var q in queries)
                    {
                        models.Add(q.ToMeasurementQueryModel());
                    }
                }
                else
                {
                    models = null;
                    //no access key found!!
                }
            }
            return models;
        }

        /// <summary>
        /// Creates list of sql parameters from properties and values of data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Data which properties are parametrisized</param>
        /// <param name="exclude">List of exclude property names</param>
        /// <returns></returns>
        public List<SqlParameter> CreateSQLParameters<T>(List<T> data, List<string> exclude)
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<SqlParameter> parameters = new List<SqlParameter>();
            var type = data[0].GetType();
            var properties = type.GetProperties();
            for (int i = 0; i < data.Count; i++)
            {
                foreach (var prop in properties)
                {
                    if (exclude == null || !exclude.Any(e => e == prop.Name))
                    {
                        try
                        {
                            if (prop.GetIndexParameters().Length == 0)
                            {
                                var propType = prop.PropertyType;
                                var value = prop.GetValue(data[i]);
                                if (value == null)
                                {
                                    value = DBNull.Value;
                                }
                                parameters.Add(new SqlParameter("@" + prop.Name + i, value));
                            }
                            else
                            {
                                Trace.TraceInformation("   {0} ({1}): <Indexed>", prop.Name,
                                                 prop.PropertyType.Name);
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.TraceInformation(e.Message);
                            throw e;
                        }
                    }
                }
            }
            return parameters;
        }

        /// <summary>
        /// Creates sql paremeters from List<string>
        /// </summary>
        /// <param name="datatags"></param>
        /// <returns></returns>
        private SqlParameter[] CreateSqlParameters(List<string> datatags)
        {
            SqlParameter[] parameters = new SqlParameter[datatags.Count];
            for (int i = 0; i < datatags.Count; i++)
            {
                parameters[i] = new SqlParameter("@tag" + i, datatags[i]);
            }

            return parameters;

        }
        //TODO: should sqloperators and appendsqlmethods be in dbhelper or should they be extend methods
        private enum SqlOperators
        {
            AND,
            OR
        }

        /// <summary>
        /// Appends sql string with given parameters and operators and returns parametrization ready string.
        /// </summary>
        /// <param name="baseSQL">sql string that will be appended</param>
        /// <param name="parameters">parameters which will be appended to string</param>
        /// <param name="column">column to compare</param>
        /// <param name="first">first which will be appended</param>
        /// <param name="subsequent">subsequent operators</param>
        /// <returns>[baseSQL] + first + (column=parameters[i] subsequent column=parameter[i])</returns>
        /// <example>[SELECT * FROM TableT where object='a'] AND (tag =@p1 OR tag=@p2)</example>
        private string AppendSqlStringUsingParameters(string baseSQL, SqlParameter[] parameters, string column, SqlOperators first = SqlOperators.AND, SqlOperators subsequent = SqlOperators.OR)
        {
            baseSQL += " " + first.ToString() + "( " + column + " =" + parameters[0].ParameterName;

            for (int i = 1; i < parameters.Length; i++)
            {
                baseSQL += " " + subsequent.ToString() + " " + column + " =" + parameters[i].ParameterName;
            }
            baseSQL += ")";
            return baseSQL;
        }

        /// <summary>
        /// Gets sensor async
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<SensorModel>> GetSensorsAsync(string key)
        {
            var access = await GetProviderAccessAsync(key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be read.");
            }
            if (!access.AccessAllowed(AccessControl.Read, true))
            {
                throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
            }
            return await this.GetSensorsAsync(access.ProviderID).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets sensor async
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        public async Task<List<SensorModel>> GetSensorsAsync(int providerID)
        {
            List<SensorModel> model = new List<SensorModel>();

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var sensors = await db.Sensors.Where(s => s.ProviderID == providerID).ToListAsync().ConfigureAwait(false);
                foreach (var sensor in sensors)
                {
                    model.Add(sensor.ToSensorModel());
                }
            }
            return model;
        }

        /// <summary>
        /// Gets sensor
        /// </summary>
        /// <param name="providerID"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public SensorModel GetSensor(int providerID, string tag)
        {
            SensorModel model = new SensorModel();

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var sensor = db.Sensors.Where(s => s.ProviderID == providerID && s.Tag == tag).SingleOrDefault();
                model = sensor.ToSensorModel();
            }
            return model;
        }

        /// <summary>
        /// Gets measurement async
        /// </summary>
        /// <param name="key"></param>
        /// <param name="id"></param>
        /// <param name="sensors"></param>
        /// <returns></returns>
        public async Task<MeasurementModel> GetMeasurementAsync(string key, long id, List<string> sensors)
        {
            MeasurementModel model;
            var access = await GetProviderAccessAsync(key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be read.");
            }
            if (!access.AccessAllowed(AccessControl.Read))
            {
                throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
            }
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var measurement = await db.Measurements.FindAsync(id).ConfigureAwait(false);
                if (measurement.ProviderID != access.ProviderID)
                {
                    // allow reading only measurements that belongs to specified key.
                    throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
                }
                model = measurement.ToMeasurementModel(measurement.Data, sensors);
            }
            return model;
        }

        /// <summary>
        /// Gets distinct objects under provider.
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        public async Task<List<string>> GetDistinctObjectsAsync(string key)
        {
            List<string> objects = new List<string>();
            var access = await GetProviderAccessAsync(key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be read.");
            }
            if (!access.AccessAllowed(AccessControl.Read))
            {
                throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
            }
#if DEBUG
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    string sql;
                    SqlCommand command = new SqlCommand();
                    sql = string.Format("Select Distinct Object From Measurement where ProviderID=@providerID");
                    SqlParameter p = new SqlParameter("@providerID", access.ProviderID);
                    command.Parameters.Add(p);
                    command.CommandText = sql;
                    command.Connection = connection;
                    connection.Open();
                    SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string temp;
                            temp = reader.IsDBNull(0) ? null : reader.GetString(0);
                            //TODO: should object be NOT NULL in database?
                            // temp =  reader.GetString(0);
                            if (temp != null)
                            {
                                objects.Add(temp);
                            }
                        }
                    }
                    reader.Close();
                }
            }
#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Getting distinct objects took {0} ms", sw.ElapsedMilliseconds);
#endif
            return objects;
        }


        /// <summary>
        /// Gets distinct objects that belong to provider with given provider id.
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        public async Task<List<string>> GetDistinctObjectsAsync(int providerID)
        {
            List<string> objects = new List<string>();
#if DEBUG
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                try
                {
                    objects = await (from u in db.Measurements
                               where u.ProviderID.Equals(providerID)
                               select u.Object).Distinct().ToListAsync().ConfigureAwait(false);
                }
                catch (ArgumentNullException)
                {
                    throw;
                }
            }
#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Getting distinct objects took {0} ms", sw.ElapsedMilliseconds);
#endif
            return objects;
        }

        /// <summary>
        /// Gets distinct tags that belong to provider with given provider id.
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        public async Task<List<string>> GetDistinctTagsAsync(int providerID)
        {
            List<string> tags = new List<string>();
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                try
                {
                    tags = await (from u in db.Measurements
                            where u.ProviderID.Equals(providerID)
                            select u.Tag).Distinct().ToListAsync().ConfigureAwait(false);
                }
                catch (ArgumentNullException e)
                {
                    throw e;
                }
            }
            return tags;
        }

        /// <summary>
        /// Gets distinct tags that belong to provider with given key.
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        public async Task<List<string>> GetDistinctTagsAsync(string key)
        {
            List<string> tags = new List<string>();
            var access = await GetProviderAccessAsync(key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be read.");
            }
            if (!access.AccessAllowed(AccessControl.Read))
            {
                throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
            }
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    string sql;
                    SqlCommand command = new SqlCommand();
                    sql = string.Format("Select Distinct Tag From Measurement where ProviderID=@providerID");
                    SqlParameter p = new SqlParameter("@providerID", access.ProviderID);
                    command.Parameters.Add(p);
                    command.CommandText = sql;
                    command.Connection = connection;
                    connection.Open();
                    SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string temp;
                            temp = reader.IsDBNull(0) ? null : reader.GetString(0);
                            //TODO: should tag be NOT NULL in database?
                            // temp =  reader.GetString(0);
                            if (temp != null)
                            {
                                tags.Add(temp);
                            }
                        }
                    }
                    reader.Close();
                }
            }
            return tags;
        }

        /// <summary>
        /// Gets providers
        /// </summary>
        /// <returns></returns>
        public List<ProviderModel> GetProviders()
        {
            List<ProviderModel> model = new List<ProviderModel>();
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var providers = db.Providers;
                foreach (var p in providers)
                {
                    model.Add(p.ToProviderModel());
                }
            }
            return model;
        }

        /// <summary>
        /// Gets provider
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ProviderModel GetProvider(int id)
        {
            ProviderModel model = null;
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var provider = db.Providers.SingleOrDefault(p => p.ID == id);
                model = provider.ToProviderModel();
            }
            return model;
        }

        /// <summary>
        /// Gets provider
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<ProviderModel> GetProvider(string key)
        {
            ProviderModel model = null;
            var access = await GetProviderAccessAsync(key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be read.");
            }
            if (!access.AccessAllowed(AccessControl.Read))
            {
                throw new UnauthorizedAccessException("Reading data with specified key is not allowed. Data can't be read.");
            }
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var provider = db.Providers.SingleOrDefault(p => p.ID == access.ProviderID);
                model = provider.ToProviderModel();
            }
            return model;
        }

        /// <summary>
        /// Gets access right of provider
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<DataAccess> GetProviderAccessAsync(string key)
        {
            DataAccess access = null;
            AccessKeyModel keyModel = AccessKeyModel.FromKey(key);
#if DEBUG
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                if (keyModel.ProviderID > 0)
                {
                    var ek = keyModel.Key.Hash(keyModel.ProviderID.ToString());
                    var ak = await db.AccessKeys.FindAsync(keyModel.ProviderID, ek).ConfigureAwait(false);
                    if (null != ak)
                    {
                        access = new DataAccess(ak.ToAccessKeyModel());
                    }
                }
                else
                {
                    var ek = keyModel.Key.Encrypt();
                    var provider = db.Providers.SingleOrDefault(p => p.Key == ek);
                    if (null != provider)
                    {
                        // key is found directly from Provider-object --> grant full access.
                        access = new DataAccess(
                            new AccessKeyModel()
                            {
                                ProviderID = provider.ID,
                                Key = provider.Key,
                                AccessControl = AccessControl.Full
                            }
                        );
                    }
                }
            }
#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Access key check took {0} ms (includes Hash or Encrypt).", sw.ElapsedMilliseconds);
#endif
            return access;
        }

        /// <summary>
        /// Checks if key matches required access level.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public bool IsAccessAllowed(string key, AccessControl controlType)
        {
            var access = GetProviderAccess(key);
            return access.AccessAllowed(controlType);
        }

        /// <summary>
        /// Gets provider access
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DataAccess GetProviderAccess(string key)
        {
            DataAccess access = null;
            AccessKeyModel keyModel = AccessKeyModel.FromKey(key);
#if DEBUG
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                if (keyModel.ProviderID > 0)
                {
                    var ek = keyModel.Key.Hash(keyModel.ProviderID.ToString());
                    var ak = db.AccessKeys.Find(keyModel.ProviderID, ek);
                    if (null != ak)
                    {
                        access = new DataAccess(ak.ToAccessKeyModel());
                    }
                }
                else
                {
                    var ek = keyModel.Key.Encrypt();
                    var provider = db.Providers.SingleOrDefault(p => p.Key == ek);
                    if (null != provider)
                    {
                        // key is found directly from Provider-object --> grant full access.
                        access = new DataAccess(
                            new AccessKeyModel()
                            {
                                ProviderID = provider.ID,
                                Key = provider.Key,
                                AccessControl = AccessControl.Full
                            }
                        );
                    }
                }
            }
#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Access key check took {0} ms (includes Hash or Encrypt).", sw.ElapsedMilliseconds);
#endif
            return access;
        }

        /// <summary>
        /// Gets count of Accesskeys, Measurements and Sensors of Provider. Returns Dictionary which has keys AccessKeys, Measurements and Sensors
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, int>> GetProviderDataCounts(int providerID)
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {

                int count = await db.AccessKeys.CountAsync(a => a.ProviderID == providerID).ConfigureAwait(false);
                d.Add("AccessKeys", count);
                count = 0;

                count = await db.Measurements.CountAsync(m => m.ProviderID == providerID).ConfigureAwait(false);
                d.Add("Measurements", count);
                count = 0;

                count = await db.Sensors.CountAsync(s => s.ProviderID == providerID).ConfigureAwait(false);
                d.Add("Sensors", count);

            }
            return d;
        }

        /// <summary>
        /// Saves provider to database
        /// </summary>
        /// <param name="providerModel"></param>
        public void SaveNewProvider(ProviderModel providerModel)
        {
            var provider = providerModel.ToProvider();

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                db.Providers.Add(provider);
                if (null != providerModel.Keys)
                {
                    foreach (var key in providerModel.Keys)
                    {
                        db.AccessKeys.Add(key.ToAccessKey());
                    }
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Updates provider async
        /// </summary>
        /// <param name="providerID"></param>
        /// <param name="providerModel"></param>
        /// <returns></returns>
        public async Task UpdateProviderAsync(int providerID, ProviderModel providerModel)
        {
            var updatedProvider = providerModel.ToProvider();
            updatedProvider.ID = providerID;

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                db.Providers.Attach(updatedProvider);
                var entry = db.Entry(updatedProvider);
                entry.Property(e => e.Key).IsModified = true;
                entry.Property(e => e.Info).IsModified = true;
                entry.Property(e => e.Name).IsModified = true;
                entry.Property(e => e.IsPublicDomain).IsModified = true;
                entry.Property(e => e.Tag).IsModified = true;
                entry.Property(e => e.ContactEmail).IsModified = true;
                entry.Property(e => e.ActiveFrom).IsModified = true;
                entry.Property(e => e.ActiveTo).IsModified = true;
                entry.Property(e => e.DataStorageUntil).IsModified = true;
                entry.Property(e => e.Location).IsModified = true;
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes all data that is related to provider (Sensors, AccessKeys, Measurements, Data). 
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        public async Task DeleteProviderAsync(int providerID)
        {
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var provider = await db.Providers.FindAsync(providerID).ConfigureAwait(false);
                if (provider != null)
                {
                    db.Providers.Remove(provider);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                else
                {
                    throw new System.Data.DataException("Could not find provider with specific id");
                }
            }
        }


        /// <summary>
        /// Creates new sensor. Throws error if already exists.
        /// </summary>
        /// <param name="sensorModel"></param>
        /// <returns></returns>
        public async Task CreateNewSensorAsync(SensorModel sensorModel)
        {
            var sensor = sensorModel.ToSensor(sensorModel.ProviderID);

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                db.Sensors.Add(sensor);
                try
                {
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// Tries to create new sensor. Returns true if succesfull.
        /// </summary>
        /// <param name="sensorModel"></param>
        /// <returns></returns>
        public async Task<bool> TryCreateNewSensorAsync(SensorModel sensorModel)
        {
            bool result = false;
            var sensor = sensorModel.ToSensor(sensorModel.ProviderID);

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var r = db.Sensors.AddIfNotExists(sensor, x => x.Tag == sensor.Tag && x.ProviderID == sensor.ProviderID);
                if (r != null)
                {
                    result = true;
                }
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        /// Updates sensor async. (Tag cannot be changed!)
        /// </summary>
        /// <param name="sensorModel"></param>
        /// <returns></returns>
        public async Task<int> UpdateSensorAsync(SensorModel sensorModel)
        {
            int result = 0;
            try
            {
                var updatedSensor = sensorModel.ToSensor(sensorModel.ProviderID);

                using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
                {
                    db.Sensors.Attach(updatedSensor);
                    var entry = db.Entry(updatedSensor);
                    entry.Property(e => e.Name).IsModified = true;
                    entry.Property(e => e.Description).IsModified = true;
                    entry.Property(e => e.Rounding).IsModified = true;
                    entry.Property(e => e.Unit).IsModified = true;
                    entry.Property(e => e.Location).IsModified = true;
                    result = await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        /// <summary>
        /// Deletes sensor async
        /// </summary>
        /// <param name="providerID"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<int> DeleteSensorAsync(int providerID, string tag)
        {
            int result = 0;
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var sensor = await db.Sensors.FindAsync(providerID, tag).ConfigureAwait(false);
                if (sensor != null)
                {
                    db.Sensors.Remove(sensor);
                    result = await db.SaveChangesAsync().ConfigureAwait(false);
                }
                else
                {
                    throw new System.Data.DataException("Could not delete sensor with specified values!");
                }
            }
            return result;
        }

        /// <summary>
        /// Deletes measurement async.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> DeleteMeasurementAsync(string key, long id)
        {
            int result = 0;
            var access = await GetProviderAccessAsync(key).ConfigureAwait(false);
            if (null == access)
            {
                throw new KeyNotFoundException("Provider with specified key was not found. Data can't be deleted.");
            }
            if (!access.AccessAllowed(AccessControl.Delete))
            {
                throw new UnauthorizedAccessException("Deleting data with specified key is not allowed.");
            }

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var measurement = await db.Measurements.FindAsync(id).ConfigureAwait(false);
                if (measurement.ProviderID == access.ProviderID)
                {
                    // allow deleting only measurements that belongs to specified key.
                    db.Measurements.Remove(measurement);
                    result = await db.SaveChangesAsync().ConfigureAwait(false);
                }
                else
                {
                    throw new UnauthorizedAccessException("Deleting data with specified key is not allowed.");
                }
            }
            return result;
        }
        /// <summary>
        /// Gets accessKey async
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<AccessKeyModel> GetAccessKeyAsync(string key)
        {
            AccessKeyModel model = null;

            AccessKeyModel keyModel = AccessKeyModel.FromKey(key);

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                if (keyModel.ProviderID > 0)
                {
                    var ek = keyModel.Key.Hash(keyModel.ProviderID.ToString());
                    var ak = await db.AccessKeys.FindAsync(keyModel.ProviderID, ek).ConfigureAwait(false);
                    model = ak.ToAccessKeyModel(true);
                }
            }
            return model;
        }

        /// <summary>
        /// Creates new accessKey. Throws error if already exists.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> CreateNewAccessKeyAsync(AccessKeyModel model)
        {
            int result = 0;
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var accessKey = model.ToAccessKey();

                var currentMaxKeyId = db.AccessKeys.Where(k => k.ProviderID == model.ProviderID).Max(k => k.KeyId);
                if (currentMaxKeyId.HasValue)
                {
                    accessKey.KeyId = (short)(currentMaxKeyId.Value + 1);
                }
                else
                {
                    accessKey.KeyId = 1;
                }
                try
                {
                    db.AccessKeys.Add(accessKey);
                }
                catch (Exception)
                {

                    throw;
                }
                result = await db.SaveChangesAsync().ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        /// Tries to create new AccessKey. Returns true if succesfull.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> TryCreateNewAccessKeyAsync(AccessKeyModel model)
        {
            bool result = false;
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                var accessKey = model.ToAccessKey();

                var currentMaxKeyId = db.AccessKeys.Where(k => k.ProviderID == model.ProviderID).Max(k => k.KeyId);
                if (currentMaxKeyId.HasValue)
                {
                    accessKey.KeyId = (short)(currentMaxKeyId.Value + 1);
                }
                else
                {
                    accessKey.KeyId = 1;
                }
                var r = db.AccessKeys.AddIfNotExists(accessKey, x => x.Key == accessKey.Key && x.ProviderID == model.ProviderID);
                if (r != null)
                {
                    result = true;
                }
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        /// Updates accesskey async (key cannot be changed!)
        /// </summary>
        /// <param name="accessKeyModel"></param>
        /// <returns></returns>
        public async Task<int> UpdateAccessKeyAsync(AccessKeyModel accessKeyModel)
        {
            int result = 0;
            var updatedAccessKey = accessKeyModel.ToAccessKey();

            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                db.AccessKeys.Attach(updatedAccessKey);
                var entry = db.Entry(updatedAccessKey);
                entry.Property(e => e.AccessControl).IsModified = true;
                entry.Property(e => e.ValidFrom).IsModified = true;
                entry.Property(e => e.ValidTo).IsModified = true;
                entry.Property(e => e.Info).IsModified = true;
                result = await db.SaveChangesAsync().ConfigureAwait(false);
            }
            return result;
        }
        /// <summary>
        /// Deletes accessKey async
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> DeleteAccessKeyAsync(AccessKeyModel model)
        {
            int result = 0;
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                try
                {
                    var ek = model.Key.Hash(model.ProviderID.ToString());
                    var accessKey = await db.AccessKeys.FindAsync(model.ProviderID, ek).ConfigureAwait(false);
                    if (accessKey != null)
                    {
                        db.AccessKeys.Remove(accessKey);
                    }
                    result = await db.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks Provider table if provided newKey exists. Returns true when newKey is not found
        /// from Provider table.
        /// </summary>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public bool IsNewProviderKeyValid(string newKey)
        {
            bool isValid = false;
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                newKey = newKey.Encrypt();
                isValid = !db.Providers.Any(p => p.Key == newKey);
            }
            return isValid;
        }

        /// <summary>
        /// Rehashes all the keys async
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReHashAllKeysAsync()
        {
            int result = 0;
            using (var db = new SavoniaMeasurementsV2Entities())
            {
                var keys = await db.AccessKeys.Where(a => !string.IsNullOrEmpty(a.KeyEncrypt)).ToListAsync().ConfigureAwait(false);
                List<AccessKey> newKeys = new List<AccessKey>(keys.Count);
                foreach (var item in keys)
                {
                    newKeys.Add(new AccessKey()
                    {
                        AccessControl = item.AccessControl,
                        Key = item.KeyEncrypt.Decrypt(item.ProviderID.ToString()).Hash(item.ProviderID.ToString()),
                        KeyEncrypt = item.KeyEncrypt,
                        KeyId = item.KeyId,
                        ProviderID = item.ProviderID,
                        ValidFrom = item.ValidFrom,
                        ValidTo = item.ValidTo
                    });
                    db.AccessKeys.Remove(item);
                }
                db.AccessKeys.AddRange(newKeys);
                result = await db.SaveChangesAsync().ConfigureAwait(false);
            }
            return result;
        }


        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != _db)
                {
                    _db.Dispose();
                    _db = null;
                }
            }
        }

    }
}