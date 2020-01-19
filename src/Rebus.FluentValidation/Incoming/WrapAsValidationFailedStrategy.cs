using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Incoming
{
	/// <summary>
	/// Wraps message in <see cref="IValidationFailed{TMessage}"/> when incoming message validation failed, so that it can be handled using separate handler.
	/// </summary>
	internal class WrapAsValidationFailedStrategy : IIncomingFailStrategy
	{
		private static readonly IDictionary<Type, MethodInfo> WrapperMethodCache = new Dictionary<Type, MethodInfo>();

		public Task ProcessAsync(IncomingStepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult)
		{
			Message message = context.Load<Message>();
			object body = message.Body;

			Type validatorType = validator.GetType();
			object wrappedBody = WrapMessage(body, message.Headers, validationResult, validatorType);
			var clonedHeaders = new Dictionary<string, string>(message.Headers)
			{
				[ValidateIncomingStep.ValidatorTypeKey] = validatorType.FullName
			};
			context.Save(new Message(clonedHeaders, wrappedBody));

			return next();
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
