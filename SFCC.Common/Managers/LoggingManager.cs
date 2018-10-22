using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SFCC.Common.Managers
{
    public class LoggingManager
    {
        public static void TrackEvent(string message)
        {
            try
            {
                Debug.WriteLine($"Track Event: ~~~~~~~~ {message}");
            }
            catch { }
        }

        public static void LogMessage(string message)
        {
            try
            {
                Debug.WriteLine($"Message: {message}");
            }
            catch { }
        }

        public static void LogWarning(string message)
        {
            try
            {
                Debug.WriteLine($"Warning: {message}");
            }
            catch { }
        }

        public static void LogException(string message, Exception ex)
        {
            try
            {
                Debug.WriteLine($"~~~Exception: {ex}");
            }
            catch { }
        }
    }
}
