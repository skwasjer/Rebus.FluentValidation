using System;
using System.Globalization;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Bus;
using Rebus.Extensions;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Retry;
using Rebus.Transport;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	/// <summary>
	/// Moves the message to the error queue.
	/// </summary>
	internal class DeadLetter : IValidationFailedStrategy
	{
		private readonly ILog _logger;
		private readonly IErrorHandler _errorHandler;

		public DeadLetter(ILog logger, IErrorHandler errorHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
		}

		public Task ProcessAsync(StepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult)
		{
			OriginalTransportMessage originalTransportMessage = context.Load<OriginalTransportMessage>();
			TransportMessage transportMessage = originalTransportMessage.TransportMessage;
			ITransactionContext transactionContext = context.Load<ITransactionContext>();

			_logger.Debug(string.Format(CultureInfo.CurrentCulture, Resources.ValidationFailed_MovingToErrorQueue, "{MessageType}", "{MessageId}"), transportMessage.GetMessageType(), transportMessage.GetMessageId());

			var ex = new ValidationException(validationResult.Errors);
			return _errorHandler.HandlePoisonMessage(transportMessage, transactionContext, ex);
		}
	}
}
