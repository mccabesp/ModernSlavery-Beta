using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ModernSlavery.Core.Extensions
{
    public static class EventLog
    {
        public const string LogName = "GenderPayGap";

        private static Assembly _TopAssembly;
        public static string LogSource;

        public static System.Diagnostics.EventLog Log;

        static EventLog()
        {
            LogSource = TopAssembly.GetName().Name;
        }

        public static Assembly TopAssembly
        {
            get
            {
                if (_TopAssembly == null)
                {
                    if (Assembly.GetEntryAssembly() != null)
                    {
                        _TopAssembly = Assembly.GetEntryAssembly();
                    }
                    //else if (HttpContext.Current != null && HttpContext.Current.ApplicationInstance != null)
                    //{
                    //    _TopAssembly = HttpContext.Current.ApplicationInstance.GetType().Assembly;
                    //}
                    else
                    {
                        var stackTrace = new StackTrace(); // get call stack
                        var stackFrames = stackTrace.GetFrames();

                        // write call stack method names
                        for (var i = stackFrames.Length - 1; i > -1; i--)
                        {
                            _TopAssembly = stackFrames[i].GetMethod().ReflectedType.Assembly;
                            if (_TopAssembly.GetName() != null && _TopAssembly.GetName().Name.ContainsI(LogName)) break;

                            try
                            {
                                if (((AssemblyCompanyAttribute) Attribute.GetCustomAttribute(
                                    _TopAssembly,
                                    typeof(AssemblyCompanyAttribute),
                                    false)).Company.ContainsI(LogName))
                                    break;
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                return _TopAssembly;
            }
        }


        public static string GetTitleText(this Exception ex)
        {
            var title = ex.Message;
            if (ex.InnerException != null) title += Environment.NewLine + ex.InnerException.GetTitleText();

            return title;
        }

        public static string GetDetailsText(this Exception ex)
        {
            return JsonConvert.SerializeObject(ex.GetDetails(), Formatting.Indented);
        }

        private static object GetDetails(this Exception ex)
        {
            if (ex == null) return null;

            var message = ex.Message;

            if (ex is AggregateException)
            {
                var aex = (AggregateException) ex;

                var c = 0;
                foreach (var innerEx in aex.InnerExceptions)
                    message += ++c
                               + " of "
                               + aex.InnerExceptions.Count
                               + ":"
                               + JsonConvert.SerializeObject(GetDetails(innerEx))
                               + Environment.NewLine;
            }

            if (ex.InnerException != null)
                return new ErrorDetails
                {
                    Message = message,
                    Source = ex.Source,
                    Type = ex.GetType(),
                    StackTrace = ex.FullStackTrace(),
                    InnerException = ex.InnerException.GetDetails()
                };

            return new ErrorDetails
                {Message = message, Source = ex.Source, Type = ex.GetType(), StackTrace = ex.FullStackTrace()};
        }


        /// <summary>
        ///     Provides full stack trace for the exception that occurred.
        /// </summary>
        /// <param name="exception">Exception object.</param>
        /// <param name="environmentStackTrace">Environment stack trace, for pulling additional stack frames.</param>
        public static string FullStackTrace(this Exception exception)
        {
            if (exception == null) return null;
            return exception.InnerException?.FullStackTrace() + exception.Message + Environment.NewLine + exception.StackTrace + Environment.NewLine;
        }

        public static Exception GetInnermostException(this Exception ex)
        {
            Exception innerException = null;
            while (ex.InnerException != null)
            {
                innerException = ex.InnerException;
                ex = ex.InnerException;
            }

            return innerException;
        }

        public class ErrorDetails
        {
            public string Message { get; set; }
            public string Source { get; set; }
            public Type Type { get; set; }
            public string StackTrace { get; set; }
            public object InnerException { get; set; }
        }
    }
}