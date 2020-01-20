using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Pipeline;

namespace Rebus.FluentValidation
{
	/// <summary>
	/// Describes how a message that failed to validate should be processed further.
	/// </summary>
	internal interface IValidationFailedStrategy
	{
		/// <summary>
		/// Processes a message that failed to validate.
		/// </summary>
		/// <param name="context">The step context.</param>
		/// <param name="next">A factory returning the next pipeline task to execute.</param>
		/// <param name="validator">The validator that was used to validate the message.</param>
		/// <param name="validationResult">The (failed) validation result.</param>
		/// <returns>A task that should be awaited to continue the pipeline.</returns>
		Task ProcessAsync(StepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult);
	}
}
