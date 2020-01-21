using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Rebus.Logging;
using Xunit;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	public class PassThroughTests : ValidationFailedStrategyTests
	{
		private readonly PassThrough _sut;

		public PassThroughTests()
		{
			_sut = new PassThrough(Logger);
		}

		public class When_creating : PassThroughTests
		{
			[Fact]
			public void When_creating_with_null_logger_it_should_throw()
			{
				ILog logger = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => new PassThrough(logger);

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(logger));
			}
		}

		public class When_processing : PassThroughTests
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
