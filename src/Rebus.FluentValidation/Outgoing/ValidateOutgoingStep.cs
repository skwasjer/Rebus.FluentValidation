using System;
using System.Globalization;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Extensions;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Outgoing
{
	/// <summary>
	/// Step that validates outgoing messages using FluentValidation and throws a <see cref="ValidationException"/> if the validation failed.
	/// </summary>
	[StepDocumentation("Step that validates outgoing messages using FluentValidation and throws a 'ValidationException' if the validation failed.")]
	public sealed class ValidateOutgoingStep : IOutgoingStep
	{
		private readonly ILog _logger;
		private readonly IValidatorFactory _validatorFactory;

		internal ValidateOutgoingStep(
			ILog logger,
			IValidatorFactory validatorFactory
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
		}

		/// <inheritdoc />
		async Task IOutgoingStep.Process(OutgoingStepContext context, Func<Task> next)
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
					_logger.Debug(string.Format(CultureInfo.CurrentCulture, Resources.ValidationFailed, "{MessageType}", "{ValidationResult}"), messageType.GetSimpleAssemblyQualifiedName(), validationResult);
					throw new ValidationException(validationResult.Errors);
				}
			}

			await next().ConfigureAwait(false);
		}
	}
}
