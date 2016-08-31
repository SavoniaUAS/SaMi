using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Providers.MeasurerService.Models
{
    public static class TimeHelpers
    {
        public static int HoursToSeconds(this int hours) {
            return hours.HoursToMinutes().MinutesToSeconds();
        }
        public static int HoursToMinutes(this int hours) {
            return hours * 60;
        }
        public static int MinutesToSeconds(this int minutes) {
            return minutes * 60;
        }
        public static int SecondsToMilliseconds(this int seconds) {
            return seconds * 1000;
        }
        public static int DaysToHours(this int days) {
            return days * 24;
        }
        public static int DaysToSeconds(this int days) {
            return days.DaysToHours().HoursToSeconds();
        }
    }
}
