using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	internal class ValidationFailedWrapper<TMessage> : IValidationFailed<TMessage>
	{
		public ValidationFailedWrapper
		(
			TMessage message,
			IDictionary<string, string> headers,
			ValidationResult validationResult,
			Type validatorType)
		{
			Message = message;
			Headers = headers;
			ValidationResult = validationResult;
			ValidatorType = validatorType;
		}

		public TMessage Message { get; }
		public IDictionary<string, string> Headers { get; }
		public ValidationResult ValidationResult { get; }
		public Type ValidatorType { get; }
	}
}
