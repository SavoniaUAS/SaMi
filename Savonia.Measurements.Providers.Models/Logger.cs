using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Providers.Models
{
    /// <summary>
    /// Logger that will log events to Trace log. Events are logged to Windows event log if it is specified.
    /// See https://msdn.microsoft.com/en-us/library/System.Diagnostics.Trace(v=vs.110).aspx for configuring Trace listeners.
    /// </summary>
    public class Logger
    {
        private EventLog eventLog = null;

        /// <summary>
        /// Create logger that logs only to Trace log.
        /// </summary>
        public Logger()
        { }

        /// <summary>
        /// Create logger that logs to Trace log and event log if eventLog parameter is not null.
        /// </summary>
        /// <param name="eventLog">The event log.</param>
        public Logger(EventLog eventLog)
        {
            this.eventLog = eventLog;
        }

        public bool IsEventLogDefined 
        {
            get { return null != this.eventLog; }
        }

        /// <summary>
        /// Append message to log. Message is inserted as EventLogEntryType.Information
        /// and with event id = 0.
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="addTimestamp">When true a timestamp string is added to Trace log.</param>
        public void Append(string message, bool addTimestamp = false)
        {
            Append(message, EventLogEntryType.Information, 0, addTimestamp);
        }
        /// <summary>
        /// Append message to log. Message is inserted with event id = 0.
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="type">EventLogEntryType</param>
        /// <param name="addTimestamp">When true a timestamp string is added to Trace log.</param>
        public void Append(string message, EventLogEntryType type, bool addTimestamp = false)
        {
            Append(message, type, 0, addTimestamp);
        }
        /// <summary>
        /// Append message to log.
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="type">EventLogEntryType</param>
        /// <param name="id">Application specific event id</param>
        /// <param name="addTimestamp">When true a timestamp string is added to Trace log.</param>
        public void Append(string message, EventLogEntryType type, int id, bool addTimestamp = false)
        {
            if (null != this.eventLog)
            {
                this.eventLog.WriteEntry(message, type, id);
            }
            if (addTimestamp)
            {
                message = string.Format("{0} {1}", DateTime.Now, message);
            }
            switch (type)
            { 
                case EventLogEntryType.Error:
                    Trace.TraceError("{0}: {1}", id, message);
                    break;
                case EventLogEntryType.Warning:
                    Trace.TraceWarning("{0}: {1}", id, message);
                    break;
                case EventLogEntryType.Information:
                    Trace.TraceInformation("{0}: {1}", id, message);
                    break;
                default:
                    Trace.WriteLine(message, type.ToString());
                    break;
            }
        }
    }
}
