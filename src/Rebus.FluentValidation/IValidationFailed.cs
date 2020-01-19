using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace Rebus.FluentValidation
{
	/// <summary>
	/// Describes the wrapper of a message that failed to validate.
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	public interface IValidationFailed<out TMessage>
	{
		/// <summary>
		/// Gets the message that failed to validate.
		/// </summary>
		TMessage Message { get; }

		/// <summary>
		/// Gets the headers of the message that failed to validate.
		/// </summary>
		IDictionary<string, string> Headers { get; }

		/// <summary>
		/// Gets the validation result.
		/// </summary>
		ValidationResult ValidationResult { get; }

		/// <summary>
		/// Gets the type of the validator that caused the message to fail to validate.
		/// </summary>
		Type ValidatorType { get; }
	}
}
