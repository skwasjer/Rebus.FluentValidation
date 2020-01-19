using System;
using FluentValidation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Pipeline.Send;

namespace Rebus.FluentValidation
{
	/// <summary>
	/// Extensions for <see cref="RebusConfigurer"/>.
	/// </summary>
	public static class OptionsConfigurerExtensions
	{
		/// <summary>
		/// Enables message validation using FluentValidation.
		/// </summary>
		/// <param name="configurer">The options configurer.</param>
		/// <param name="validatorFactory">The FluentValidation validator factory to resolve message validators from.</param>
		/// <param name="directions">The direction of validation.</param>
		/// <returns>The options configurer to chain configuration.</returns>
		public static void ValidateMessages(this OptionsConfigurer configurer, IValidatorFactory validatorFactory, Directions directions)
		{
			if (configurer is null)
			{
				throw new ArgumentNullException(nameof(configurer));
			}

			if (validatorFactory is null)
			{
				throw new ArgumentNullException(nameof(validatorFactory));
			}

			if (directions.HasFlag(Directions.Outgoing))
			{
				configurer.Register(ctx => new ValidateOutgoingStep(validatorFactory));
			}

			if (directions.HasFlag(Directions.Incoming))
			{
				configurer.Register(ctx =>
				{
					IRebusLoggerFactory loggerFactory = ctx.Get<IRebusLoggerFactory>();
					return new ValidateIncomingStep(
						loggerFactory.GetLogger<ValidateIncomingStep>(),
						validatorFactory
					);
				});
			}

			configurer.Decorate<IPipeline>(ctx =>
			{
				IPipeline pipeline = ctx.Get<IPipeline>();
				var pipelineInjector = new PipelineStepInjector(pipeline);

				if (directions.HasFlag(Directions.Incoming))
				{
					ValidateIncomingStep incomingStep = ctx.Get<ValidateIncomingStep>();
					pipelineInjector.OnReceive(
						incomingStep,
						PipelineRelativePosition.After,
						typeof(DeserializeIncomingMessageStep)
					);
				}

				if (directions.HasFlag(Directions.Outgoing))
				{
					ValidateOutgoingStep outgoingStep = ctx.Get<ValidateOutgoingStep>();
					pipelineInjector.OnSend(
						outgoingStep,
						PipelineRelativePosition.Before,
						typeof(AssignDefaultHeadersStep)
					);
				}

				return pipelineInjector;
			});
		}
	}
}
