﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WinMemoryCleaner
{
    /// <summary>
    /// Logger
    /// </summary>
    internal static class Logger
    {
        #region Fields

        private static Enums.Log.Level _level = Enums.Log.Level.Debug | Enums.Log.Level.Information | Enums.Log.Level.Warning | Enums.Log.Level.Error;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        public static Enums.Log.Level Level
        {
            set
            {
                switch (value)
                {
                    case Enums.Log.Level.Debug:
                        _level = Enums.Log.Level.Debug | Enums.Log.Level.Information | Enums.Log.Level.Warning | Enums.Log.Level.Error;
                        break;

                    case Enums.Log.Level.Information:
                        _level = Enums.Log.Level.Information | Enums.Log.Level.Warning | Enums.Log.Level.Error;
                        break;

                    case Enums.Log.Level.Warning:
                        _level = Enums.Log.Level.Warning | Enums.Log.Level.Error;
                        break;

                    case Enums.Log.Level.Error:
                        _level = Enums.Log.Level.Error;
                        break;
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="method">Method</param>
        public static void Debug(string message, [CallerMemberName] string method = null)
        {
            Log(Enums.Log.Level.Debug, message, method);
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="message">Custom message about the Exception</param>
        /// <param name="method">Method</param>
        public static void Error(Exception exception, string message = null, [CallerMemberName] string method = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = exception.GetBaseException().Message;

            if ((_level & Enums.Log.Level.Debug) != 0)
            {
                try
                {
                    StackTrace st = new StackTrace(exception, true);
                    StackFrame frame = st.GetFrame(st.FrameCount - 1);
                    MethodBase methodBase = frame.GetMethod();

                    if (methodBase.DeclaringType != null)
                        method = string.Format(CultureInfo.CurrentCulture, "{0} > LN: {1}", methodBase.DeclaringType.Name + "." + methodBase.Name, frame.GetFileLineNumber());
                }
                catch
                {
                    // ignored
                }
            }

            Log(Enums.Log.Level.Error, message, method);
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="method">Method</param>
        public static void Error(string message, [CallerMemberName] string method = null)
        {
            Log(Enums.Log.Level.Error, message, method);
        }

        /// <summary>
        /// Windows Event
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="type">Type</param>
        private static void Event(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            try
            {
                EventLog.WriteEntry(Constants.App.Title, message, type);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="method">Method</param>
        public static void Information(string message, [CallerMemberName] string method = null)
        {
            Log(Enums.Log.Level.Information, message, method);
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="message">Message</param>
        /// <param name="method">Method</param>
        private static void Log(Enums.Log.Level level, string message, [CallerMemberName] string method = null)
        {
            try
            {
                Log log = new Log
                {
                    DateTime = DateTime.Now,
                    Level = level,
                    Method = method,
                    Message = message
                };

                string traceMessage = string.Format(CultureInfo.CurrentCulture, "{0}\t{1}\t{2}",
                    log.DateTime.ToString(Constants.App.Log.DatetimeFormat, CultureInfo.CurrentCulture),
                    log.Level.ToString().ToUpper(CultureInfo.CurrentCulture),
                    string.IsNullOrWhiteSpace(log.Method) ? log.Message : string.Format(CultureInfo.CurrentCulture, "[{0}] {1}", log.Method, log.Message));

                switch (level)
                {
                    case Enums.Log.Level.Debug:
                        if ((_level & Enums.Log.Level.Debug) != 0)
                        {
                            Event(message);
                            Trace.WriteLine(traceMessage);
                        }
                        break;

                    case Enums.Log.Level.Information:
                        if ((_level & Enums.Log.Level.Information) != 0)
                        {
                            Event(message);
                            Trace.TraceInformation(traceMessage);
                        }
                        break;

                    case Enums.Log.Level.Warning:
                        if ((_level & Enums.Log.Level.Warning) != 0)
                        {
                            Event(message, EventLogEntryType.Warning);
                            Trace.TraceWarning(traceMessage);
                        }
                        break;

                    case Enums.Log.Level.Error:
                        if ((_level & Enums.Log.Level.Error) != 0)
                        {
                            Event(message, EventLogEntryType.Error);
                            Trace.TraceError(traceMessage);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                try
                {
                    Trace.TraceError(e.GetBaseException().Message);
                }
                catch
                {
                    // ignored
                }

                Event(string.Format(CultureInfo.CurrentCulture, Localizer.String.ErrorCanNotSaveLog, message, e.GetBaseException().Message), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="method">Method</param>
        public static void Warning(string message, [CallerMemberName] string method = null)
        {
            Log(Enums.Log.Level.Warning, message, method);
        }
    }

    #endregion Methods
}