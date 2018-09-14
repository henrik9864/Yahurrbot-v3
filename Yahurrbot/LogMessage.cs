using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework
{
	public class LogMessage
	{
		public LogLevel LogLevel { get; }

		public string Message { get; }

		public string Source { get; }

		public DateTime Timestamp { get; }

		public Exception Exception { get; }

		public LogMessage(LogLevel logLevel, string message, string source)
		{
			LogLevel = logLevel;
			Message = message;
			Source = source;
			Timestamp = DateTime.Now;
		}

		public LogMessage(Exception exception, string source)
		{
			Exception = exception;
			Source = source;
			Message = exception?.Message ?? "Unknown";
			LogLevel = LogLevel.Error;
			Timestamp = DateTime.Now;
		}
	}
}
