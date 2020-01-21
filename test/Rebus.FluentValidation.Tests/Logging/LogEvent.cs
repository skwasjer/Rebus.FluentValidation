using System;
using Rebus.Logging;

namespace Rebus.FluentValidation.Logging
{
	public class LogEvent
	{
		public LogLevel Level { get; set; }
		public string Message { get; set; }
		public object[] FormatParameters { get; set; }
		public Exception Exception { get; set; }
		public string FormattedMessage => XunitRebusLoggerFactory.FormatMessage(Message, FormatParameters);

		public override string ToString()
		{
			return $"{Level}: {FormattedMessage} {Exception}";
		}
	}
}
