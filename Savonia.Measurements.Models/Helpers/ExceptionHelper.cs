using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Models.Helpers
{
    public static class ExceptionHelper
    {
        public static string FriendlyException(this Exception ex)
        {
            string message = "";

            var iex = ex;
            while (null != iex.InnerException)
            {
                iex = iex.InnerException;
            }

            message = iex.Message;

            return message;
        }
    }
}
