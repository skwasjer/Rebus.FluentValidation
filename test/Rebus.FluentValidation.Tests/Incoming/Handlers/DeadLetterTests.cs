using System;
using FluentAssertions;
using Moq;
using Rebus.Logging;
using Rebus.Retry;
using Xunit;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	public class DeadLetterTests
	{
		[Fact]
		public void When_creating_with_null_logger_it_should_throw()
		{
			ILog logger = null;

			// ReSharper disable once ExpressionIsAlwaysNull
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new DeadLetter(logger, Mock.Of<IErrorHandler>());

			// Assert
			act.Should().Throw<ArgumentNullException>()
				.Which.ParamName.Should().Be(nameof(logger));
		}

		[Fact]
		public void When_creating_with_null_errorHandler_it_should_throw()
		{
			IErrorHandler errorHandler = null;

			// ReSharper disable once ExpressionIsAlwaysNull
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new DeadLetter(Mock.Of<ILog>(), errorHandler);

			// Assert
			act.Should().Throw<ArgumentNullException>()
				.Which.ParamName.Should().Be(nameof(errorHandler));
		}
	}
}
