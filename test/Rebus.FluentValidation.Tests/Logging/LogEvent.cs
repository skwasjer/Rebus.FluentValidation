using System;
using Rebus.Logging;

namespace Rebus.FluentValidation.Logging
{
	public class LogEvent
	{
		public LogLevel Level { get; set; }
		public string Message { get; set; }
		public string FormattedMessage { get; set; }
		public object[] FormatParameters { get; set; }
		public Exception Exception { get; set; }

		public override string ToString()
		{
			return $"{Level}: {FormattedMessage} {Exception}";
		}
	}
}
