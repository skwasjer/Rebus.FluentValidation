using System;
using System.Collections.Generic;
using FluentValidation;
using Rebus.Config;
using Rebus.FluentValidation.Incoming;
using Rebus.FluentValidation.Incoming.Handlers;
using Rebus.FluentValidation.Outgoing;
using Rebus.Logging;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Pipeline.Send;

namespace Rebus.FluentValidation
{
	/// <summary>
	/// Extensions for <see cref="RebusConfigurer" />.
	/// </summary>
	public static class OptionsConfigurerExtensions
	{
		/// <summary>
		/// Enables message validation for outgoing messages using FluentValidation.
		/// <para>
		/// When an outgoing message sent/published to the bus fails to validate, a <see cref="ValidationException"/> will be thrown immediately.
		/// </para>
		/// <para>
		/// When an incoming message fails to validate, by default it is wrapped in a <see cref="IValidationFailed{TMessage}"/> message and dispatched to handlers implementing this wrapped message type. Use the <paramref name="onFailed"/> builder to configure if messages should be handled differently (f.ex. move to error queue, drop, etc.).
		/// </para>
		/// </summary>
		/// <param name="configurer">The options configurer.</param>
		/// <param name="validatorFactory">The FluentValidation validator factory to resolve message validators from.</param>
		/// <param name="onFailed">A builder to configure how messages should be handled when validation fails.</param>
		public static void ValidateMessages(this OptionsConfigurer configurer, IValidatorFactory validatorFactory, Action<ValidationConfigurer> onFailed = null)
		{
			configurer.ValidateIncomingMessages(validatorFactory, onFailed);
			configurer.ValidateOutgoingMessages(validatorFactory);
		}

		/// <summary>
		/// Enables message validation for outgoing messages using FluentValidation.
		/// <para>
		/// When an outgoing message sent/published to the bus fails to validate, a <see cref="ValidationException"/> will be thrown immediately.
		/// </para>
		/// </summary>
		/// <param name="configurer">The options configurer.</param>
		/// <param name="validatorFactory">The FluentValidation validator factory to resolve message validators from.</param>
		public static void ValidateOutgoingMessages(this OptionsConfigurer configurer, IValidatorFactory validatorFactory)
		{
			if (configurer is null)
			{
				throw new ArgumentNullException(nameof(configurer));
			}

			if (validatorFactory is null)
			{
				throw new ArgumentNullException(nameof(validatorFactory));
			}

			configurer.Register(ctx => new ValidateOutgoingStep(validatorFactory));

			configurer.Decorate<IPipeline>(ctx =>
			{
				IPipeline pipeline = ctx.Get<IPipeline>();
				var pipelineInjector = new PipelineStepInjector(pipeline);

				ValidateOutgoingStep outgoingStep = ctx.Get<ValidateOutgoingStep>();
				pipelineInjector.OnSend(
					outgoingStep,
					PipelineRelativePosition.Before,
					typeof(AssignDefaultHeadersStep)
				);

				return pipelineInjector;
			});
		}

		/// <summary>
		/// Enables message validation for incoming messages using FluentValidation.
		/// <para>
		/// When an incoming message fails to validate, by default it is wrapped in a <see cref="IValidationFailed{TMessage}"/> message and dispatched to handlers implementing this wrapped message type. Use the <paramref name="onFailed"/> builder to configure if messages should be handled differently (f.ex. move to error queue, drop, etc.).
		/// </para>
		/// </summary>
		/// <param name="configurer">The options configurer.</param>
		/// <param name="validatorFactory">The FluentValidation validator factory to resolve message validators from.</param>
		/// <param name="onFailed">A builder to configure how messages should be handled when validation fails.</param>
		public static void ValidateIncomingMessages(this OptionsConfigurer configurer, IValidatorFactory validatorFactory, Action<ValidationConfigurer> onFailed = null)
		{
			if (configurer is null)
			{
				throw new ArgumentNullException(nameof(configurer));
			}

			if (validatorFactory is null)
			{
				throw new ArgumentNullException(nameof(validatorFactory));
			}

			var opts = new ValidationConfigurer(configurer);
			onFailed?.Invoke(opts);

			configurer.Register(ctx =>
			{
				IRebusLoggerFactory loggerFactory = ctx.Get<IRebusLoggerFactory>();
				return new ValidateIncomingStep(
					loggerFactory.GetLogger<ValidateIncomingStep>(),
					validatorFactory,
					ctx.Get<IReadOnlyDictionary<Type, IValidationFailedStrategy>>(),
					// By default, handle as IValidationFailed<>
					ctx.Get<WrapAsValidationFailed>()
				);
			});

			configurer.Decorate<IPipeline>(ctx =>
			{
				IPipeline pipeline = ctx.Get<IPipeline>();
				var pipelineInjector = new PipelineStepInjector(pipeline);

				ValidateIncomingStep incomingStep = ctx.Get<ValidateIncomingStep>();
				pipelineInjector.OnReceive(
					incomingStep,
					PipelineRelativePosition.After,
					typeof(DeserializeIncomingMessageStep)
				);

				return pipelineInjector;
			});
		}
	}
}
