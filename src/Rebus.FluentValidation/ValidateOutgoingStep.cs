using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class ValidateOutgoingStep : IOutgoingStep
	{
		private readonly IValidatorFactory _validatorFactory;

		internal ValidateOutgoingStep(IValidatorFactory validatorFactory)
		{
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
					throw new ValidationException(validationResult.Errors);
				}
			}

			await next().ConfigureAwait(false);
		}
	}
}
