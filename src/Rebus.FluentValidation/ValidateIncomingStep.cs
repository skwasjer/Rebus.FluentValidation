using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation
{
	/// <summary>
	/// 
	/// </summary>
	[StepDocumentation("")]
	public sealed class ValidateIncomingStep : IIncomingStep
	{
#pragma warning disable 1591
		public const string ValidatorTypeKey = "ValidatorType";
#pragma warning restore 1591

		private static readonly IDictionary<Type, MethodInfo> WrapperMethodCache = new Dictionary<Type, MethodInfo>();

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

					Type validatorType = validator.GetType();
					object wrappedBody = WrapMessage(body, message.Headers, validationResult, validatorType);
					var clonedHeaders = new Dictionary<string, string>(message.Headers)
					{
						[ValidatorTypeKey] = validatorType.FullName
					};
					context.Save(new Message(clonedHeaders, wrappedBody));
				}
			}

			await next().ConfigureAwait(false);
		}

		private object WrapMessage(
			object message,
			IDictionary<string, string> headers,
			ValidationResult validationResult,
			Type validatorType
		)
		{
			Type messageType = message.GetType();
			// ReSharper disable once InvertIf
			if (!WrapperMethodCache.TryGetValue(messageType, out MethodInfo methodInfo))
			{
				MethodInfo wrapMethodInfo = GetType().GetMethod(nameof(Wrap), BindingFlags.Static | BindingFlags.NonPublic);
				methodInfo = wrapMethodInfo.MakeGenericMethod(messageType);
				WrapperMethodCache[messageType] = methodInfo;
			}

			return methodInfo.Invoke(this, new[] { message, headers, validationResult, validatorType });
		}

		private static IValidationFailed<TMessage> Wrap<TMessage>(
			TMessage message,
			IDictionary<string, string> headers,
			ValidationResult validationResult,
			Type validatorType
		)
		{
			return new ValidationFailedWrapper<TMessage>(message, headers, validationResult, validatorType);
		}
	}
}
