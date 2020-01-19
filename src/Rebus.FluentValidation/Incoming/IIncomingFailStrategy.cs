using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Rebus.Pipeline;

namespace Rebus.FluentValidation.Incoming
{
	internal interface IIncomingFailStrategy
	{
		Task ProcessAsync(IncomingStepContext context, Func<Task> next, IValidator validator, ValidationResult validationResult);
	}
}
