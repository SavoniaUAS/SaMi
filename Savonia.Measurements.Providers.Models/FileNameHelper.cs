using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Providers.Models
{
    /// <summary>
    /// Helpers for file names.
    /// </summary>
    public static class FileNameHelper
    {
        /// <summary>
        /// Default plugins folder name. The SAMI service will set this to a correct value. Do not change!
        /// </summary>
        public static string PluginsFolder = "plugins";

        /// <summary>
        /// Get safe filename helper. Removes illegal characters for a filename from a string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacementCharacter"></param>
        /// <returns></returns>
        public static string GetSafeFileName(this string text, char? replacementCharacter = null)
        {
            if (null == text)
            {
                return string.Empty;
            }
#if DEBUG
            Stopwatch sw = Stopwatch.StartNew();
#endif
            string safe = text;
            var inv = System.IO.Path.GetInvalidFileNameChars();
            if (replacementCharacter.HasValue)
            {
                safe = new string(text.Select(s => inv.Contains(s) ? replacementCharacter.Value : s).ToArray()); 
            }
            else
            {
                safe = new string(text.Where(s => !inv.Contains(s)).ToArray()); 
            }
#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format("GetSafeFileName took {0} ms. Replacement was {1}.", sw.Elapsed.TotalMilliseconds, replacementCharacter));
#endif
            return safe;
        }

        /// <summary>
        /// Get rooted path for target. If target is allready rooted then it is returned unmodified.
        /// If target is not rooted then app path is added and possible relative forder is added to target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="appRelativeFolder"></param>
        /// <returns></returns>
        public static string GetRootedPath(this string target, string appRelativeFolder = "")
        {
            if (null == target)
            {
                return null;
            }

            if (System.IO.Path.IsPathRooted(target))
            {
                return target;
            }
            else
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appRelativeFolder, target);
            }
        }

        /// <summary>
        /// Gets rooted path for a file in the plugins folder. If target is not rooted then app path\plugins is added to target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string GetPluginRootedPath(this string target)
        {
            return target.GetRootedPath(PluginsFolder);
        }
    }
}
