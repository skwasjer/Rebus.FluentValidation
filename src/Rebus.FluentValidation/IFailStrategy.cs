using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Pipeline;

namespace Rebus.FluentValidation
{
	internal interface IFailStrategy
	{
		Task ProcessAsync(IncomingStepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult);
	}
}
