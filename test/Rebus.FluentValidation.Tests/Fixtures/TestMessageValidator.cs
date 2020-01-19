using FluentValidation;

namespace Rebus.FluentValidation.Fixtures
{
	/// <summary>
	/// Validator that validates successfully or unsuccessfully depending on <see cref="TestMessage.ShouldPassValidation"/>.
	/// </summary>
	public class TestMessageValidator : AbstractValidator<TestMessage>
	{
		public TestMessageValidator()
		{
			RuleFor(x => x.ShouldPassValidation)
				.Must((message, shouldPass) =>
				{
					message.IsValidated = true;
					return shouldPass;
				});
		}
	}
}
