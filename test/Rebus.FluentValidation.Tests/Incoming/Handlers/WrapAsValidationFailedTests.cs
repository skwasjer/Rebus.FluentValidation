using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Rebus.Bus;
using Rebus.FluentValidation.Fixtures;
using Rebus.FluentValidation.Logging;
using Rebus.Logging;
using Rebus.Messages;
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

			[Fact]
			public async Task It_should_log_info()
			{
				// Act
				await _sut.ProcessAsync(StepContext, Next, ValidatorMock.Object, ValidationResult);

				// Assert
				LoggerFactory.LogEvents.Should()
					.BeEquivalentTo(new LogEvent
					{
						Level = LogLevel.Info,
						Message = "Validation -> {MessageType} {MessageId} is configured to be wrapped as " + typeof(IValidationFailed<>).FullName + ".",
						FormatParameters = new object[]
						{
							Message.GetMessageType(),
							Message.GetMessageId()
						}
					});
			}

			[Fact]
			public async Task It_should_wrap_message_in_validation_failed()
			{
				Message message = StepContext.Load<Message>();
				message.Body.Should().BeOfType<TestMessage>();

				// Act
				await _sut.ProcessAsync(StepContext, Next, ValidatorMock.Object, ValidationResult);

				// Assert
				Message newMessage = StepContext.Load<Message>();
				IValidationFailed<TestMessage> wrappedMessageBody = newMessage.Body.Should()
					.BeAssignableTo<IValidationFailed<TestMessage>>()
					.Which;
				wrappedMessageBody.Message.Should().BeSameAs(message.Body);
				wrappedMessageBody.Headers.Should().BeSameAs(message.Headers);
				wrappedMessageBody.ValidationResult.Should().BeSameAs(ValidationResult);
				wrappedMessageBody.ValidatorType.Should().Be(ValidatorMock.Object.GetType());
			}
		}
	}
}
