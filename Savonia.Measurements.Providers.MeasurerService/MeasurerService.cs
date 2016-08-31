using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Providers.Models;
using Savonia.Measurements.Providers.MeasurerService.Models;
using System.IO;
using System.Configuration;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using System.Threading;
using Savonia.Measurements.Models;

namespace Savonia.Measurements.Providers.MeasurerService
{
    public partial class MeasurerService : ServiceBase
    {
        private System.Timers.Timer localStoreTimer;
        private Dictionary<System.Timers.Timer, IMeasurer> timers;
        private PluginSettings settings;

        private event EventHandler<PluginExecuteEventArgs> ExecutePluginProcess;

        private IMeasurementRepository repository;

        private string LocalStoreFolder = string.Empty;

        private Logger log;

        private bool lastSaveState = false;
        private int localStorePurgeCount = 20;
        private int localStorePurgeInterval = 30;
        private bool stopServiceWhenPluginProcessThrows = false;
        private static object locker = new object();

        public MeasurerService() : base()
        {
            InitializeComponent();

            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (null != this.EventLog)
            {
                Trace.TraceInformation("{0}: ServiceBase.Eventlog - source: {1}, log: {2}.", DateTime.Now, this.EventLog.Source, this.EventLog.Log);
            }
            else
            {
                Trace.TraceInformation("{0}: ServiceBase.EventLog is null.", DateTime.Now);
            }
            this.log = new Logger(this.EventLog);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.timers = new Dictionary<System.Timers.Timer, IMeasurer>();

            ExecutePluginProcess += MeasurerService_ExecutePluginProcess;

            var confValue = ConfigurationManager.AppSettings["MeasurementsRepositoryDll"];
            if (string.IsNullOrEmpty(confValue))
            {
                log.Append(string.Format("MeasurementsRepositoryDll setting value is missing or empty! No repository where to store measurements."), EventLogEntryType.Error, true);
                this.repository = null;
            }
            else
            {
                this.LoadRepository(confValue, ConfigurationManager.AppSettings["MeasurementsRepositoryConfig"]);
            }
            bool b;
            confValue = ConfigurationManager.AppSettings["StopServiceWhenPluginProcessThrows"];
            if (bool.TryParse(confValue, out b))
            {
                this.stopServiceWhenPluginProcessThrows = b;
            }
            int i;
            confValue = ConfigurationManager.AppSettings["LocalStorePurgeTake"];
            if (int.TryParse(confValue, out i))
            {
                this.localStorePurgeCount = i;
            }
            confValue = ConfigurationManager.AppSettings["LocalStorePurgeInterval"];
            if (int.TryParse(confValue, out i))
            {
                this.localStorePurgeInterval = i;
            }
            this.LocalStoreFolder = ConfigurationManager.AppSettings["LocalStoreFolder"];
            if (string.IsNullOrEmpty(this.LocalStoreFolder))
            {
                log.Append(string.Format("LocalStoreFolder setting value is missing or empty! Local storage is disabled."), EventLogEntryType.Warning, true);
            }
            else
            {
                this.LocalStoreFolder = this.LocalStoreFolder.GetRootedPath();
            }
        }

        void MeasurerService_ExecutePluginProcess(object sender, PluginExecuteEventArgs e)
        {
            IMeasurer im = e.MeasurerPlugin;
            if (im != null)
            {
#if DEBUG
                log.Append(string.Format("Starting new Plugin {0} Process() task.", im.Name), EventLogEntryType.Information, true);
#endif
                // see: http://stackoverflow.com/questions/17298269/catching-exceptions-caused-in-different-threads
                var task = Task.Factory.StartNew(im.Process);
                task.ContinueWith(t => PluginProcess_UnhandledException(t.Exception, im.Name), TaskContinuationOptions.OnlyOnFaulted);
#if DEBUG
                log.Append(string.Format("Task run {0}.", im.Name), EventLogEntryType.Information, true);
#endif
            }
            else
            {
                log.Append(string.Format("Measurer plugin class is null, cannot perform Measurer process. Service will be stopped."), EventLogEntryType.Error, 100, true);
                this.ExitCode = 100;
                this.Stop();
            }
        }

        void PluginProcess_UnhandledException(AggregateException ex, string pluginName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in ex.Flatten().InnerExceptions)
            {
                sb.AppendLine(ex.ToString());
            }
            if (this.stopServiceWhenPluginProcessThrows)
            {
                log.Append(string.Format("Plugin {1} Process() threw exception. Service will be stopped.{0}{0}Details: {2}", Environment.NewLine, pluginName, sb.ToString()), EventLogEntryType.Error, 601, true);
                this.ExitCode = 600;
                this.Stop();
            }
            else
            {
                log.Append(string.Format("Plugin {1} Process() threw exception.{0}{0}Details: {2}", Environment.NewLine, pluginName, sb.ToString()), EventLogEntryType.Error, 601, true);
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
            log.Append(string.Format("Unhandled exception caught. Service is shutting down.{0}{0}Sender: {1}{0}Exception object: {2}{0}{0}CLR is terminating = {3}", Environment.NewLine, sender, e.ExceptionObject.ToString(), e.IsTerminating), EventLogEntryType.Error, 500, true);

            this.ExitCode = 500;
            this.Stop();
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            if (null == this.repository)
            {
                this.ExitCode = 30;
                this.Stop();
            }

            this.LoadSettings();
            try
            {
                this.LoadPlugins();
            }
            catch (PluginAssemblyException pEx)
            {
                string innerMsg = string.Empty;
                if (pEx.InnerException != null)
                {
                    innerMsg = pEx.InnerException.Message;
                }
                log.Append(string.Format("Measurer Plugins assembly dll error. Service will not be started.{0}Check Measurer assembly {1} and it's settings.{0}{0}Error details: {2}", Environment.NewLine, pEx.Name, pEx.ToString()), EventLogEntryType.Error, true);
                this.ExitCode = 30;
                this.Stop();
            }

            log.Append(string.Format("Started MeasurerService. Save measurements via {0}. Current working directory is {1}.", this.repository.Name, System.IO.Directory.GetCurrentDirectory()), EventLogEntryType.Information, true);
            this.lastSaveState = true; // default save state to true.
            this.StartTimers();
        }

        protected override void OnStop()
        {
            this.StopTimers();
            this.ClearTimers();
            this.settings = null;

            this.ExecutePluginProcess -= MeasurerService_ExecutePluginProcess;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            
            Trace.TraceInformation("{0}: MeasurerService stopped.", DateTime.Now);
            
            base.OnStop();
        }

        private void LoadSettings()
        {
            // load settings xml file
            // and deserialize xml file to PluginSettings
            string settingsFile = ConfigurationManager.AppSettings["PluginSettings"];
            if (string.IsNullOrEmpty(settingsFile))
            {
                throw new Exception("PluginSettings setting in config is invalid.");
            }
            // set plugins folder path to filename helper static class.
            FileNameHelper.PluginsFolder = System.IO.Path.GetDirectoryName(settingsFile);
            using (StreamReader sr = new StreamReader(settingsFile.GetRootedPath()))
            {
                XmlSerializer xs = new XmlSerializer(typeof(PluginSettings));
                this.settings = xs.Deserialize(sr) as PluginSettings;
                xs = null;
                sr.Close();
            }
        }

        void LoadRepository(string repositoryAssembly, string config)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(repositoryAssembly.GetRootedPath());
                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsNotPublic)
                    {
                        continue;
                    }
                    Type[] interfaces = type.GetInterfaces();
                    if (((IList)interfaces).Contains(typeof(IMeasurementRepository)))
                    {
                        this.repository = (IMeasurementRepository)Activator.CreateInstance(type);
                        this.repository.Initialize(config);
                        this.repository.Log = new Logger(this.EventLog);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PluginAssemblyException(repositoryAssembly, "Failed to load repository assembly.", ex);
            }
        }

        void LoadPlugins()
        {
            IMeasurer im;
            foreach (MeasurerProcessPlugin mpp in this.settings.Plugins)
            {
                if (mpp.Enabled)
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFile(mpp.PluginAssembly.GetPluginRootedPath());
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (!type.IsClass || type.IsNotPublic)
                            {
                                continue;
                            }
                            Type[] interfaces = type.GetInterfaces();
                            if (((IList)interfaces).Contains(typeof(IMeasurer)))
                            {
                                im = (IMeasurer)Activator.CreateInstance(type);
                                im.ConfigurationFile = mpp.ConfigFile;
                                im.Name = mpp.Name;
                                im.Log = new Logger(this.EventLog);
                                im.LoadConfig();
                                im.ProcessCompleted += measurer_ProcessCompleted;
                                this.CreateTimer(mpp, im);
#if DEBUG
                                log.Append(string.Format("Loaded Measurer plugin assembly {0}.", mpp.PluginAssembly), true);
#endif
                                if (mpp.ExecuteOnServiceStart)
                                {
                                    this.OnExecutePluginProcess(this, new PluginExecuteEventArgs(im, DateTime.Now));
                                }
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new PluginAssemblyException(mpp.PluginAssembly, "Failed to load plugin assembly.", ex);
                    }
                }
            }
        }

        void measurer_ProcessCompleted(object sender, ProcessCompletedEventArgs e)
        {
            this.Persist(e);
        }

        private bool Persist(ProcessCompletedEventArgs e, bool storeLocallyIfFailed = true)
        {
            bool saved = false;
            MeasurementPersistResult result = null;
            try
            {
                result = this.repository.Persist(e.MeasurementPackage);
                if (null == result || !result.Persisted)
                {
                    if (storeLocallyIfFailed)
                    {
                        this.StoreLocally(e, result);
                        if (null != result.Exception)
                        {
                            log.Append(string.Format("Persist to {1} failed for {2} with exception.{0}{0}Measurements stored locally.{0}{0}Exception details: {3}", Environment.NewLine, this.repository.Name, e.Source, result.Exception.ToString()), EventLogEntryType.Warning, true);
                        }
                        else
                        {
                            log.Append(string.Format("Persist to {1} failed for {2}.{0}{0}Measurements stored locally.", Environment.NewLine, this.repository.Name, e.Source), EventLogEntryType.Warning, true);
                        }
                    }
                    lock (locker)
                    {
                        this.lastSaveState = false;
                    }
                }
                else
                {
                    saved = true;
                    // save was successfull
                    lock (locker)
                    {
                        this.lastSaveState = true;
                    }
                }
            }
            catch (Exception ex)
            {
                lock (locker)
                {
                    this.lastSaveState = false;
                }
                log.Append(string.Format("Persist to {1} failed for {2}.{0}{0}Exception details: {3}", Environment.NewLine, this.repository.Name, e.Source, ex.ToString()), EventLogEntryType.Error, true);
                if (storeLocallyIfFailed)
                {
                    this.StoreLocally(e, result);
                }
            }
            return saved;
        }

        private void StoreLocally(ProcessCompletedEventArgs e, MeasurementPersistResult result)
        {
            if (string.IsNullOrEmpty(this.LocalStoreFolder))
            {
                log.Append(string.Format("Local storage is disabled. Data from {0} is lost.", e.Source), EventLogEntryType.Warning, true);
                return;
            }
            if (!System.IO.Directory.Exists(this.LocalStoreFolder))
            { 
                System.IO.Directory.CreateDirectory(this.LocalStoreFolder);
            }

            string filename = string.Format("{0}-{1}.xml", DateTime.Now.ToString("yyyyMMddHHmmssfff"), e.Source.GetSafeFileName());

            if (null != result && null != result.SaveResult)
            {
                // remove successfully saved measurements from package before local storage

                List<MeasurementModel> failedMeasurements = new List<MeasurementModel>();
                foreach (var f in result.SaveResult.Failures)
                {
                    // list failed measurements
                    failedMeasurements.Add(e.MeasurementPackage.Measurements[f.Index]);
                }

                // remowe all measurements and add failed measurements
                e.MeasurementPackage.Measurements.Clear();
                e.MeasurementPackage.Measurements = failedMeasurements;
            }
            this.SerializeToFile(e.MeasurementPackage, System.IO.Path.Combine(this.LocalStoreFolder, filename));
        }

        private void SerializeToFile(Savonia.Measurements.Models.MeasurementPackage mp, string path)
        {
            var data = SerializeHelper.SerializeToStringWithDCS<Savonia.Measurements.Models.MeasurementPackage>(mp);
            System.IO.File.WriteAllText(path, data);
        }

        private void OnExecutePluginProcess(object sender, PluginExecuteEventArgs e)
        {
            var handler = ExecutePluginProcess;
            if (null != handler)
            {
                handler(sender, e);
            }
        }

        private void CreateTimer(MeasurerProcessPlugin mpp, IMeasurer im)
        {
            System.Timers.Timer timer;
            timer = new System.Timers.Timer();
            timer.AutoReset = true;

            if (mpp.ExecuteInterval > this.settings.MinimumExecutionInterval)
            {
                timer.Interval = (double)mpp.ExecuteInterval.SecondsToMilliseconds();
            }
            else
            {
                timer.Interval = (double)this.settings.MinimumExecutionInterval.SecondsToMilliseconds();
            }
            timer.Elapsed += timer_Elapsed;

            this.timers.Add(timer, im);
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            IMeasurer im = null;
            im = this.timers[(System.Timers.Timer)sender];

            PluginExecuteEventArgs pe = new PluginExecuteEventArgs(im, e.SignalTime);

            this.OnExecutePluginProcess(sender, pe);
        }

        private void StartTimers()
        {
            foreach (System.Timers.Timer t in this.timers.Keys)
            {
                t.Start();
            }
            this.localStoreTimer = new System.Timers.Timer(this.localStorePurgeInterval.SecondsToMilliseconds());
            this.localStoreTimer.Elapsed += localStoreTimer_Elapsed;
            this.localStoreTimer.Start();
        }

        void localStoreTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!this.lastSaveState)
            {
                // do nothing if last save state was false.
                return;
            }
            System.IO.DirectoryInfo di = new DirectoryInfo(this.LocalStoreFolder);
            if (!di.Exists)
            {
                return;
            }
            // pause timer
            System.Timers.Timer t = (System.Timers.Timer)sender;
            t.Stop();

            // get files
            var files = di.GetFiles("*.xml").Take(this.localStorePurgeCount);
            if (null != files && files.Count() > 0)
            {
                log.Append(string.Format("Read {0} xml files from local store {1}.", files.Count(), this.LocalStoreFolder), EventLogEntryType.Information, true);
                Measurements.Models.MeasurementPackage mp;
                ProcessCompletedEventArgs pe;
                bool saveSuccessfull = false;
                foreach (var f in files)
                {
                    try
                    {
                        mp = SerializeHelper.DeserializeWithDCS<Measurements.Models.MeasurementPackage>(System.IO.File.ReadAllText(f.FullName));
                        if (!string.IsNullOrEmpty(mp.Key))
                        {
                            // try to save only packages with key.

                            pe = new ProcessCompletedEventArgs("local store", mp);
                            saveSuccessfull = this.Persist(pe, false);
                            if (saveSuccessfull)
                            {
                                // local store data successfully saved to back end --> delete local file
                                f.Delete();
                                log.Append(string.Format("Successfully saved measurements from file {0}. The file was deleted.", f.FullName), EventLogEntryType.Information, true);
                            }
                            else
                            {
                                log.Append(string.Format("Could not save measurements from file {0}.", f.FullName), EventLogEntryType.Warning, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Append(string.Format("Deserializing local store file {1} failed.{0}{0}Details: {2}", Environment.NewLine, f.FullName, ex.ToString()), EventLogEntryType.Warning, 300, true);
                    }
                    if (!this.lastSaveState)
                    {
                        // break looping if lastSaveState changes to false during local store purge.
                        break;
                    }
                }
            }
            t.Start(); // re-start local store timer.
        }

        private void StopTimers()
        {
            if (null != this.timers)
            {
                foreach (System.Timers.Timer t in this.timers.Keys)
                {
                    t.Stop();
                }
            }
            if (null != this.localStoreTimer)
            {
                this.localStoreTimer.Stop();
            }
        }

        private void ClearTimers()
        {
            if (null != this.timers)
            {
                IMeasurer im;
                foreach (System.Timers.Timer t in this.timers.Keys)
                {
                    t.Elapsed -= timer_Elapsed;
                    t.Close();
                    im = this.timers[t];
                    im.ProcessCompleted -= measurer_ProcessCompleted;
                    im.Dispose();
                }
                this.timers.Clear();
            }
            if (null != this.localStoreTimer)
            {
                this.localStoreTimer.Elapsed -= localStoreTimer_Elapsed;
                this.localStoreTimer = null;
            }
        }
    }

    public class PluginExecuteEventArgs : EventArgs
    {
        public IMeasurer MeasurerPlugin { get; private set; }
        public DateTimeOffset SignalTime { get; private set; }

        public PluginExecuteEventArgs(IMeasurer im, DateTimeOffset executedTime)
            : base()
        {
            this.MeasurerPlugin = im;
            this.SignalTime = executedTime;
        }
    }

    public class PluginAssemblyException : Exception
    {
        private string assemblyName;

        public PluginAssemblyException(string assemblyName, string message)
            : base(message)
        {
            this.assemblyName = assemblyName;
        }

        public PluginAssemblyException(string assemblyName, string message, Exception innerException)
            : base(message, innerException)
        {
            this.assemblyName = assemblyName;
        }

        public string Name
        {
            get { return this.assemblyName; }
        }

        public override string Message
        {
            get
            {
                return string.Format("{0} (Assembly name {1})", base.Message, this.Name);
            }
        }
    }
}
