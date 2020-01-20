using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Logging;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	/// <summary>
	/// Passes the message through unchanged to the next pipeline step.
	/// </summary>
	internal class PassThrough : IValidationFailedStrategy
	{
		private readonly ILog _logger;

		public PassThrough(ILog logger)
		{
			_logger = logger;
		}

		public Task ProcessAsync(StepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult)
		{
			return next();
		}
	}
}
