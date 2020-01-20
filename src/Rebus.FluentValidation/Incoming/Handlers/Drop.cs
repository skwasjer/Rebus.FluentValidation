using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Logging;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	/// <summary>
	/// Drops the message that failed to validate.
	/// </summary>
	internal class Drop : IValidationFailedStrategy
	{
		private readonly ILog _logger;

		public Drop(ILog logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public Task ProcessAsync(StepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult)
		{
			// TODO: log that we're skipping message.
//			_logger.Debug("");
			return Task.CompletedTask;
		}
	}
}
