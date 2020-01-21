using System;
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
	/// Passes the message through unchanged to the next pipeline step.
	/// </summary>
	internal class PassThrough : IValidationFailedStrategy
	{
		private readonly ILog _logger;

		public PassThrough(ILog logger)
		{
			_logger = logger;
		}

		public Task ProcessAsync(StepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult)
		{
			Message message = context.Load<Message>();

			_logger.Warn("Validation -> {MessageType} {MessageId} is configured to pass through.", message.GetMessageType(), message.GetMessageId());
			return next();
		}
	}
}
