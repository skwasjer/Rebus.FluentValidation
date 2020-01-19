using System;
using System.Collections.Generic;
using Rebus.Logging;
using Xunit.Abstractions;

namespace Rebus.FluentValidation.Logging
{
	public class XunitRebusLoggerFactory : AbstractRebusLoggerFactory
	{
		private readonly ITestOutputHelper _testOutputHelper;

		public XunitRebusLoggerFactory(ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper;
		}

		public List<LogEvent> LogEvents { get; } = new List<LogEvent>();

		protected override ILog GetLogger(Type type)
		{
			return new XunitLogger(this, type.Name);
		}

		private class XunitLogger : ILog
		{
			private readonly XunitRebusLoggerFactory _loggerFactory;
			private readonly string _name;

			public XunitLogger(XunitRebusLoggerFactory loggerFactory, string name)
			{
				_loggerFactory = loggerFactory;
				_name = name;
			}

			public void Debug(string message, params object[] objs)
			{
				Log(LogLevel.Debug, message, objs);
			}

			public void Info(string message, params object[] objs)
			{
				Log(LogLevel.Info, message, objs);
			}

			public void Warn(string message, params object[] objs)
			{
				Log(LogLevel.Warn, message, objs);
			}

			public void Warn(Exception exception, string message, params object[] objs)
			{
				Log(LogLevel.Warn, message, objs, exception);
			}

			public void Error(Exception exception, string message, params object[] objs)
			{
				Log(LogLevel.Error, message, objs, exception);
			}

			public void Error(string message, params object[] objs)
			{
				Log(LogLevel.Error, message, objs);
			}

			private void Log(LogLevel level, string message, object[] objs, Exception exception = null)
			{
				var logEvent = new LogEvent
				{
					Level = level,
					Message = message,
					FormattedMessage = SafeFormat(message, objs),
					FormatParameters = objs,
					Exception = exception
				};
				_loggerFactory.LogEvents.Add(logEvent);
				_loggerFactory._testOutputHelper.WriteLine($"{level}: {logEvent.FormattedMessage} {exception}");
			}

			private string SafeFormat(string message, object[] objs)
			{
				return _loggerFactory.RenderString(message, objs);
			}
		}

	}
}
