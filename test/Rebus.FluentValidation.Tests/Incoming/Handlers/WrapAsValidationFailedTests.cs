using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Rebus.Logging;
using Xunit;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	public class WrapAsValidationFailedTests : ValidationFailedStrategyTests
	{
		private readonly WrapAsValidationFailed _sut;

		public WrapAsValidationFailedTests()
		{
			_sut = new WrapAsValidationFailed(Logger);
		}

		public class When_creating : WrapAsValidationFailedTests
		{
			[Fact]
			public void When_creating_with_null_logger_it_should_throw()
			{
				ILog logger = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => new WrapAsValidationFailed(logger);

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(logger));
			}
		}

		public class When_processing : WrapAsValidationFailedTests
		{
			[Fact]
			public async Task It_should_call_next()
			{
				var nextMock = new Mock<Func<Task>>();

				// Act
				await _sut.ProcessAsync(StepContext, nextMock.Object, ValidatorMock.Object, ValidationResult);

				// Assert
				nextMock.Verify(func => func(), Times.Once);
			}
		}
	}
}
