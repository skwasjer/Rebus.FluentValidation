using System;
using FluentValidation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;

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
		/// <returns>The options configurer to chain configuration.</returns>
		public static void ValidateMessages(this OptionsConfigurer configurer, IValidatorFactory validatorFactory)
		{
			if (configurer is null)
			{
				throw new ArgumentNullException(nameof(configurer));
			}

			if (validatorFactory is null)
			{
				throw new ArgumentNullException(nameof(validatorFactory));
			}

			configurer.Decorate<IPipeline>(ctx =>
			{
				IPipeline pipeline = ctx.Get<IPipeline>();
				IRebusLoggerFactory loggerFactory = ctx.Get<IRebusLoggerFactory>();
				var incomingStep = new ValidateIncomingStep(loggerFactory.GetLogger<ValidateIncomingStep>(), validatorFactory);

				return new PipelineStepInjector(pipeline)
					.OnReceive(incomingStep, PipelineRelativePosition.After, typeof(DeserializeIncomingMessageStep));
			});
		}
	}
}
