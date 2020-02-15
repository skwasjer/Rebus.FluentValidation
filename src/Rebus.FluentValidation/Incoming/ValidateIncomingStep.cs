using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Bus;
using Rebus.Extensions;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Incoming
{
	/// <summary>
	/// Step that validates incoming messages using FluentValidation and handles it using configured options (dead letter, drop, pass-through, wrap as <see cref="IValidationFailed{TMessage}"/>, etc. if validation fails.
	/// </summary>
	[StepDocumentation("Step that validates incoming messages using FluentValidation and handles it using configured options (dead letter, drop, pass-through, wrap as IValidationFailed<>, etc. if validation fails.")]
	public sealed class ValidateIncomingStep : IIncomingStep
	{
		/// <summary>
		/// Header key in which the validator type is saved when message validation fails.
		/// </summary>
		public const string ValidatorTypeKey = "ValidatorType";

		private readonly ILog _logger;
		private readonly IValidatorFactory _validatorFactory;
		private readonly IReadOnlyDictionary<Type, IValidationFailedStrategy> _failHandlers;
		private readonly IValidationFailedStrategy _defaultFailHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidateIncomingStep"/>.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="validatorFactory"></param>
		/// <param name="failHandlers"></param>
		/// <param name="defaultFailHandler"></param>
		internal ValidateIncomingStep(
			ILog logger,
			IValidatorFactory validatorFactory,
			IReadOnlyDictionary<Type, IValidationFailedStrategy> failHandlers,
			IValidationFailedStrategy defaultFailHandler
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
			_failHandlers = failHandlers;
			_defaultFailHandler = defaultFailHandler ?? throw new ArgumentNullException(nameof(defaultFailHandler));
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

				Type validatorType = validator.GetType();
				message.Headers[ValidatorTypeKey] = validatorType.GetSimpleAssemblyQualifiedName();

				if (validationResult.IsValid)
				{
					_logger.Debug(string.Format(CultureInfo.CurrentCulture, Resources.ValidationSucceeded, "{MessageId}"), message.GetMessageId());
				}
				else
				{
					_logger.Debug(string.Format(CultureInfo.CurrentCulture, Resources.ValidationFailed, "{MessageId}", "{ValidationResult}"), message.GetMessageId(), validationResult);

					if (!_failHandlers.TryGetValue(messageType, out IValidationFailedStrategy handler))
					{
						handler = _defaultFailHandler;
					}

					await handler.ProcessAsync(context, next, validator, validationResult).ConfigureAwait(false);
					return;
				}
			}

			await next().ConfigureAwait(false);
		}
	}
}
