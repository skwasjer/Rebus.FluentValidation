using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Incoming
{
	/// <summary>
	/// 
	/// </summary>
	[StepDocumentation("")]
	public sealed class ValidateIncomingStep : IIncomingStep
	{
		/// <summary>
		/// Header key in which the validator type is saved when message validation fails.
		/// </summary>
		public const string ValidatorTypeKey = "ValidatorType";

		private readonly ILog _logger;
		private readonly IValidatorFactory _validatorFactory;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidateIncomingStep"/>.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="validatorFactory"></param>
		internal ValidateIncomingStep(
			ILog logger,
			IValidatorFactory validatorFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
		}

		/// <inheritdoc />
		async Task IIncomingStep.Process(IncomingStepContext context, Func<Task> next)
		{
			Message message = context.Load<Message>();
			object body = message.Body;
			Type messageType = body.GetType();

			IValidator validator = _validatorFactory.GetValidator(messageType);
			if (validator != null && validator.CanValidateInstancesOfType(messageType))
			{
				ValidationResult validationResult = await validator.ValidateAsync(body).ConfigureAwait(false);
				if (!validationResult.IsValid)
				{
					_logger.Debug("Message failed to validate.");

					// TODO: select from multiple strategies how to handle it
					// - as IValidationFailed
					// - dead letter
					// - poison?
					// - ignore
					var handler = new WrapAsValidationFailedStrategy();
					await handler.ProcessAsync(context, next, validator, validationResult).ConfigureAwait(false);
					return;
				}
			}

			await next().ConfigureAwait(false);
		}
	}
}
