using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Rebus.Config;
using Rebus.FluentValidation.Incoming.Handlers;
using Rebus.Injection;
using Rebus.Logging;
using Rebus.Retry;

namespace Rebus.FluentValidation.Incoming
{
	/// <summary>
	/// Enables configuration of how messages should be handled when message validation fails.
	/// </summary>
	public class ValidationConfigurer
	{
		private class HandlerRegistration
		{
			public Type MessageType { get; set; }
			public Type StrategyType { get; set; }
			public Func<IResolutionContext, IValidationFailedStrategy> ImplementationFactory { get; set; }
		}

		private readonly OptionsConfigurer _configurer;
		private readonly Dictionary<Type, IValidationFailedStrategy> _handlers;
		private readonly Dictionary<Type, HandlerRegistration> _registeredHandlerTypes;

		internal ValidationConfigurer(OptionsConfigurer configurer)
		{
			_configurer = configurer;
			_handlers = new Dictionary<Type, IValidationFailedStrategy>();
			_registeredHandlerTypes = new Dictionary<Type, HandlerRegistration>();

			_configurer.Register(ctx =>
			{
				foreach (HandlerRegistration registration in _registeredHandlerTypes.Values)
				{
					_handlers[registration.MessageType] = registration.ImplementationFactory(ctx);
				}

				return (IReadOnlyDictionary<Type, IValidationFailedStrategy>)new ReadOnlyDictionary<Type, IValidationFailedStrategy>(_handlers);
			});

			_configurer.Register(ctx => new Drop(
				ctx.Get<IRebusLoggerFactory>().GetLogger<Drop>()
			));
			_configurer.Register(ctx => new DeadLetter(
				ctx.Get<IRebusLoggerFactory>().GetLogger<DeadLetter>(),
				ctx.Get<IErrorHandler>()
			));
			_configurer.Register(ctx => new WrapAsValidationFailed(
				ctx.Get<IRebusLoggerFactory>().GetLogger<WrapAsValidationFailed>()
			));
			_configurer.Register(ctx => new PassThrough(
				ctx.Get<IRebusLoggerFactory>().GetLogger<PassThrough>()
			));
		}

		/// <summary>
		/// Automatically moves messages of type <typeparamref name="TMessage" /> that failed to validate to the error queue.
		/// </summary>
		/// <typeparam name="TMessage">The message type.</typeparam>
		public ValidationConfigurer DeadLetter<TMessage>()
		{
			return OnValidationFailed<TMessage, DeadLetter>();
		}

		/// <summary>
		/// Drops messages of type <typeparamref name="TMessage" /> that failed to validate.
		/// </summary>
		/// <typeparam name="TMessage">The message type.</typeparam>
		public ValidationConfigurer Drop<TMessage>()
		{
			return OnValidationFailed<TMessage, Drop>();
		}

		/// <summary>
		/// Only emits a warning for messages of type <typeparamref name="TMessage" /> that failed to validate, but otherwise lets the message pass through as it normally would.
		/// </summary>
		/// <typeparam name="TMessage">The message type.</typeparam>
		public ValidationConfigurer PassThrough<TMessage>()
		{
			return OnValidationFailed<TMessage, PassThrough>();
		}

		private ValidationConfigurer OnValidationFailed<TMessage, TValidationFailedStrategy>()
			where TValidationFailedStrategy : IValidationFailedStrategy
		{
			Type messageType = typeof(TMessage);
			if (_registeredHandlerTypes.TryGetValue(messageType, out HandlerRegistration registeredHandlerType))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ArgumentNullException_MessageTypeAlreadyConfigured, messageType.FullName, registeredHandlerType.StrategyType.FullName));
			}

			_registeredHandlerTypes[messageType] = new HandlerRegistration
			{
				MessageType = messageType,
				StrategyType = typeof(TValidationFailedStrategy),
				ImplementationFactory = ctx => ctx.Get<TValidationFailedStrategy>()
			};

			return this;
		}
	}
}
