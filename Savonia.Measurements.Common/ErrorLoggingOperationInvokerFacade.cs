using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Common
{
    // http://stackoverflow.com/questions/26761990/how-do-i-get-accurate-exception-stack-in-wcf-task-based-operation
    public class ErrorLoggingOperationInvokerFacade : IOperationInvoker
    {
        public static string KEY_ERROR_RECIPIENTS = "ExceptionMailsRecipients";

        private readonly IOperationInvoker _invoker;

        public ErrorLoggingOperationInvokerFacade(IOperationInvoker invoker)
        {
            _invoker = invoker;
        }

        public object[] AllocateInputs()
        {
            return _invoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            return _invoker.Invoke(instance, inputs, out outputs);
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            return _invoker.InvokeBegin(instance, inputs, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            var task = result as Task;
            if (task != null && task.IsFaulted && task.Exception != null)
            {
                this.Log(task.Exception);
            }

            return _invoker.InvokeEnd(instance, out outputs, result);
        }

        public bool IsSynchronous { get { return _invoker.IsSynchronous; } }

        private void Log(AggregateException ex)
        {
            if (null == ex)
            {
                return;
            }
            var flattened = ex.Flatten();
            string recipients = ConfigurationManager.AppSettings[KEY_ERROR_RECIPIENTS];
            string error = this.GenerateMessageFromException(flattened);

            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.Subject = string.Format("Exception: {0}", (null != ex.TargetSite) ? ex.TargetSite.Name : "no target site");
            message.Body = error;
            message.Body += "\n\nTimestamp: " + DateTime.Now.ToString();

            if (!string.IsNullOrEmpty(recipients))
            {
                try
                {
                    message.To.Add(recipients);
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                    smtp.Send(message);
                }
                catch (Exception sendEx)
                {
                    Trace.WriteLine(string.Format("Failed to send error message. Exception: {0}", sendEx.Message));
                    Trace.WriteLine(string.Format("Failed error message content: {0}", null == message ? "no message" : message.Body));
                }
            }
            else
            {
                Trace.WriteLine(string.Format("{0}{1}{2}", message.Subject, Environment.NewLine, message.Body));
            }
        }

        /// <summary>
        /// Generates a string message from exception details.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns></returns>
        public string GenerateMessageFromException(AggregateException ex)
        {
            if (null == ex)
            {
                return string.Empty;
            }
            string message = string.Format("Source: {1}{0}Message: {2}{0}{0}StackTrace: {3}{0}", Environment.NewLine, ex.Source, ex.Message, ex.StackTrace);
            Exception iex = ex.InnerException;
            while (null != iex)
            {
                message = string.Format("{0}{1}{0}Source: {2}{0}Message: {3}{0}{0}StackTrace: {4}{0}", Environment.NewLine, message, iex.Source, iex.Message, iex.StackTrace);
                iex = iex.InnerException;
            }
            foreach (var aiex in ex.InnerExceptions)
            {
                message += string.Format("{0}{1}{0}", Environment.NewLine, aiex.ToString());
            }

            return message;
        }
    }
}
