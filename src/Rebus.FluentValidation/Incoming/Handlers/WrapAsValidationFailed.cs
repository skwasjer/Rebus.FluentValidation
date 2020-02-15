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
		private const string WrapMethodName = nameof(Wrap);

		private readonly ILog _logger;
		private static readonly IDictionary<Type, MethodInfo> WrapperMethodCache = new Dictionary<Type, MethodInfo>();

		public WrapAsValidationFailed(ILog logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
			MethodInfo methodInfo = GetWrapperMethod(messageType);
			try
			{
				return methodInfo.Invoke(this, new[] { message, headers, validationResult, validatorType });
			}
			catch (Exception ex) when (!(ex is InvalidOperationException))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.RebusApplicationException_CouldNotWrapMessage, message), ex);
			}
		}

		private MethodInfo GetWrapperMethod(Type messageType)
		{
			if (WrapperMethodCache.TryGetValue(messageType, out MethodInfo methodInfo))
			{
				return methodInfo;
			}

			MethodInfo wrapMethodInfo = GetType().GetMethod(WrapMethodName, BindingFlags.Static | BindingFlags.NonPublic);
			if (wrapMethodInfo is null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidOperationException_WrapMethodDoesNotExist, WrapMethodName));
			}

			return WrapperMethodCache[messageType] = wrapMethodInfo.MakeGenericMethod(messageType);
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
