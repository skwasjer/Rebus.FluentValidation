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
		/// Enables message validation using FluentValidation.
		/// </summary>
		/// <param name="configurer">The options configurer.</param>
		/// <param name="validatorFactory">The FluentValidation validator factory to resolve message validators from.</param>
		/// <returns>The options configurer to chain configuration.</returns>
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
		/// Enables message validation using FluentValidation.
		/// </summary>
		/// <param name="configurer">The options configurer.</param>
		/// <param name="validatorFactory">The FluentValidation validator factory to resolve message validators from.</param>
		/// <returns>The options configurer to chain configuration.</returns>
		public static void ValidateIncomingMessages(this OptionsConfigurer configurer, IValidatorFactory validatorFactory)
		{
			ValidateIncomingMessages(configurer, validatorFactory, _ => { });
		}

		/// <summary>
		/// Enables message validation using FluentValidation.
		/// </summary>
		/// <param name="configurer">The options configurer.</param>
		/// <param name="validatorFactory">The FluentValidation validator factory to resolve message validators from.</param>
		/// <param name="onFailed">A builder to configure how messages should be handled when validation fails.</param>
		/// <returns>The options configurer to chain configuration.</returns>
		public static void ValidateIncomingMessages(this OptionsConfigurer configurer, IValidatorFactory validatorFactory, Action<ValidationConfigurer> onFailed)
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
