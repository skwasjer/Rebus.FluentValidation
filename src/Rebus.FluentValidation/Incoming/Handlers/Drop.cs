using System;
using System.Globalization;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Bus;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	/// <summary>
	/// Drops the message that failed to validate.
	/// </summary>
	internal class Drop : IValidationFailedStrategy
	{
		private readonly ILog _logger;

		public Drop(ILog logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public Task ProcessAsync(StepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult)
		{
			Message message = context.Load<Message>();

			_logger.Warn(string.Format(CultureInfo.CurrentCulture, Resources.ValidationFailed_Drop, "{MessageType}", "{MessageId}"), message.GetMessageType(), message.GetMessageId());

#if NET45
			return Task.WhenAny();
#else
			return Task.CompletedTask;
#endif
		}
	}
}
