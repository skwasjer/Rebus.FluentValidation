using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
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
			_logger = logger;
			_errorHandler = errorHandler;
		}

		public Task ProcessAsync(StepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult)
		{
			TransportMessage transportMessage = context.Load<TransportMessage>();
			ITransactionContext transactionContext = context.Load<ITransactionContext>();

			Type validatorType = validator.GetType();
			transportMessage.Headers[ValidateIncomingStep.ValidatorTypeKey] = validatorType.FullName;

			var ex = new ValidationException(validationResult.Errors);
			return _errorHandler.HandlePoisonMessage(transportMessage, transactionContext, ex);
		}
	}
}
