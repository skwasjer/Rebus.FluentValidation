using System;
using FluentAssertions;
using Rebus.Logging;
using Xunit;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	public class PassThroughTests
	{
		[Fact]
		public void When_creating_with_null_logger_it_should_throw()
		{
			ILog logger = null;

			// ReSharper disable once ExpressionIsAlwaysNull
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new PassThrough(logger);

			// Assert
			act.Should().Throw<ArgumentNullException>()
				.Which.ParamName.Should().Be(nameof(logger));
		}
	}
}
