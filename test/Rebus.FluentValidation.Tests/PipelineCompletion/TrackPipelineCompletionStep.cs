using System;
using System.Threading.Tasks;
using Rebus.Messages;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.PipelineCompletion
{
	public class TrackPipelineCompletionStep<TMessage> : IIncomingStep, IOutgoingStep
	{
		private readonly Action<TMessage> _onSent;
		private readonly Action<TMessage> _onReceived;

		public TrackPipelineCompletionStep(Action<TMessage> onSent, Action<TMessage> onReceived)
		{
			_onSent = onSent;
			_onReceived = onReceived;
		}

		public async Task Process(IncomingStepContext context, Func<Task> next)
		{
			await Callback(context, next, _onReceived);
		}

		public async Task Process(OutgoingStepContext context, Func<Task> next)
		{
			await Callback(context, next, _onSent);
		}

		private static async Task Callback(StepContext context, Func<Task> next, Action<TMessage> callback)
		{
			await next();
			if (callback == null)
			{
				return;
			}

			Message message = context.Load<Message>();
			Type messageType = message.Body.GetType();
			Type callbackType = callback.GetType().GetGenericArguments()[0];
			if (callbackType.IsAssignableFrom(messageType))
			{
				callback((TMessage)message.Body);
			}
		}
	}
}
