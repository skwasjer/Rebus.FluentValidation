using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	/// <summary>
	/// Wraps the message in <see cref="IValidationFailed{TMessage}"/>, so that it can be handled using a <see cref="IHandleMessages{TMessage}"/> handler.
	/// </summary>
	internal class WrapAsValidationFailed : IValidationFailedStrategy
	{
		private readonly ILog _logger;
		private static readonly IDictionary<Type, MethodInfo> WrapperMethodCache = new Dictionary<Type, MethodInfo>();

		public WrapAsValidationFailed(ILog logger)
		{
			_logger = logger;
		}

		public Task ProcessAsync(StepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult)
		{
			Message message = context.Load<Message>();
			object body = message.Body;

			_logger.Info(string.Format(CultureInfo.CurrentCulture, Resources.ValidationFailed_WrapAsValidationFailed, "{MessageType}", "{MessageId}", typeof(IValidationFailed<>).FullName), message.GetMessageType(), message.GetMessageId());

			Type validatorType = validator.GetType();
			object wrappedBody = WrapMessage(body, message.Headers, validationResult, validatorType);
			context.Save(new Message(message.Headers, wrappedBody));

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
