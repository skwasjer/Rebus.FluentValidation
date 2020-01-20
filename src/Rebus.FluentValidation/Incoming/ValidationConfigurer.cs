using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Rebus.Config;
using Rebus.FluentValidation.Incoming.Handlers;
using Rebus.Handlers;
using Rebus.Logging;
using Rebus.Retry;

namespace Rebus.FluentValidation.Incoming
{
	/// <summary>
	/// Enables configuration of how messages should be handled when message validation fails.
	/// </summary>
	public class ValidationConfigurer
	{
		private readonly OptionsConfigurer _configurer;
		private readonly Dictionary<Type, IValidationFailedStrategy> _handlers;

		internal ValidationConfigurer(OptionsConfigurer configurer)
		{
			_configurer = configurer;
			_handlers = new Dictionary<Type, IValidationFailedStrategy>();
			_configurer.Register(_ => (IReadOnlyDictionary<Type, IValidationFailedStrategy>)new ReadOnlyDictionary<Type, IValidationFailedStrategy>(_handlers));

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
		public void DeadLetter<TMessage>()
		{
			OnValidationFailed<TMessage, DeadLetter>();
		}

		/// <summary>
		/// Drops messages of type <typeparamref name="TMessage" /> that failed to validate.
		/// </summary>
		/// <typeparam name="TMessage">The message type.</typeparam>
		public void Drop<TMessage>()
		{
			OnValidationFailed<TMessage, Drop>();
		}

		/// <summary>
		/// Wraps messages of type <typeparamref name="TMessage" /> that failed to validate into an <see cref="IValidationFailed{TMessage}"/> wrapper message, allowing it to be handled separately using <see cref="IHandleMessages{TMessage}"/>.
		/// </summary>
		/// <typeparam name="TMessage">The message type.</typeparam>
		public void HandleAsFailed<TMessage>()
		{
			OnValidationFailed<TMessage, WrapAsValidationFailed>();
		}

		/// <summary>
		/// Only emits a warning for messages of type <typeparamref name="TMessage" /> that failed to validate, but otherwise lets the message pass through as it normally would.
		/// </summary>
		/// <typeparam name="TMessage">The message type.</typeparam>
		public void PassThrough<TMessage>()
		{
			OnValidationFailed<TMessage, PassThrough>();
		}

		private void OnValidationFailed<TMessage, TValidationFailedHandler>()
			where TValidationFailedHandler : IValidationFailedStrategy
		{
			_configurer.Decorate(ctx =>
			{
				_handlers[typeof(TMessage)] = ctx.Get<TValidationFailedHandler>();
				return (IReadOnlyDictionary<Type, IValidationFailedStrategy>)_handlers;
			});
		}
	}
}
