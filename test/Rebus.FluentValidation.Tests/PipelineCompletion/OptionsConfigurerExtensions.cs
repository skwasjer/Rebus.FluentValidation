using System;
using Rebus.Config;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.PipelineCompletion
{
	public static class OptionsConfigurerExtensions
	{
		public static void OnPipelineCompletion<TMessage>(this OptionsConfigurer configurer, Action<TMessage> onReceived)
		{
			configurer.Decorate<IPipeline>(ctx =>
			{
				IPipeline pipeline = ctx.Get<IPipeline>();
				return new PipelineStepConcatenator(pipeline)
					.OnReceive(new TrackPipelineCompletionStep<TMessage>(null, onReceived), PipelineAbsolutePosition.Front);
			});
		}
	}
}
