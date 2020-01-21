using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Rebus.Logging;
using Xunit;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	public class DropTests : ValidationFailedStrategyTests
	{
		private readonly Drop _sut;

		public DropTests()
		{
			_sut = new Drop(Logger);
		}

		public class When_creating : DropTests
		{
			[Fact]
			public void With_null_logger_it_should_throw()
			{
				ILog logger = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => new Drop(logger);

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(logger));
			}
		}

		public class When_processing : DropTests
		{
			[Fact]
			public async Task It_should_not_call_next()
			{
				var nextMock = new Mock<Func<Task>>();

				// Act
				await _sut.ProcessAsync(StepContext, nextMock.Object, ValidatorMock.Object, ValidationResult);

				// Assert
				nextMock.Verify(func => func(), Times.Never);
			}
		}
	}
}
