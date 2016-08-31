using Savonia.Measurements.Models;
using Savonia.Measurements.Providers.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginTester
{
    class Program
    {
        private static string Output = "";

        private static Stopwatch sw;

        static void Main(string[] args)
        {
            Logger log = new Logger();
            Trace.Listeners.Add(new ConsoleTraceListener());
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: PluginTester.exe plugin-dll-path plugin-config-path output-file");
                Console.WriteLine("{0}PluginTester loads the provided measurer plugin (plugin-dll-path) and executes the plugin's Process method.{0}Returned measurement(s) are serialized to output-file.{0} - If the plugin writes to the Log those texts are shown in the console.", Environment.NewLine);
                return;
            }
            var pluginFile = args[0].GetRootedPath();
            var settingsFile = args[1].GetRootedPath();
            Output = args[2].GetRootedPath();

            var measurer = LoadPlugin(pluginFile, settingsFile, log);

            if (null != measurer)
            {
                sw = Stopwatch.StartNew();
                Console.WriteLine("Measurer process started. Wait for results or press any key to exit.{0}", Environment.NewLine);
                measurer.Process();
                Console.ReadKey();
                sw.Stop();
            }
            else
            {
                Console.WriteLine("Could not load measurer plugin from file {0}.", pluginFile);
            }
        }

        static IMeasurer LoadPlugin(string pluginPath, string settingsPath, Logger log)
        {
            IMeasurer im;
            Assembly assembly = Assembly.LoadFile(pluginPath);
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
                    im.ConfigurationFile = settingsPath;
                    im.Name = System.IO.Path.GetFileNameWithoutExtension(pluginPath);
                    im.Log = log;
                    im.LoadConfig();
                    im.ProcessCompleted += im_ProcessCompleted;
                    return im;
                }
            }
            return null;
        }

        static void im_ProcessCompleted(object sender, ProcessCompletedEventArgs e)
        {
            sw.Stop();
            Console.WriteLine("Got results from measurer. {0}Plugin Process method took {1}.", Environment.NewLine, sw.Elapsed);
            var xml = SerializeHelper.SerializeToStringWithDCS<MeasurementPackage>(e.MeasurementPackage);
            System.IO.File.WriteAllText(Output, xml);
            Console.WriteLine(string.Format("{0} measurements with key {1} serialized to {2}.", e.MeasurementPackage.Measurements.Count, e.MeasurementPackage.Key, Output));
            Console.WriteLine(string.Format("{0}Press any key to exit.", Environment.NewLine));
        }
    }
}
