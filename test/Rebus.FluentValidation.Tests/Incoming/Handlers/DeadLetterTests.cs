using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using Rebus.Bus;
using Rebus.FluentValidation.Logging;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Retry;
using Rebus.Transport;
using Xunit;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	public class DeadLetterTests : ValidationFailedStrategyTests
	{
		private readonly Mock<IErrorHandler> _errorHandlerMock;
		private readonly DeadLetter _sut;

		public DeadLetterTests()
		{
			_errorHandlerMock = new Mock<IErrorHandler>();

			_sut = new DeadLetter(Logger, _errorHandlerMock.Object);
		}

		public class When_creating : DeadLetterTests
		{
			[Fact]
			public void With_null_logger_it_should_throw()
			{
				ILog logger = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => new DeadLetter(logger, Mock.Of<IErrorHandler>());

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(logger));
			}

			[Fact]
			public void With_null_errorHandler_it_should_throw()
			{
				IErrorHandler errorHandler = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => new DeadLetter(Mock.Of<ILog>(), errorHandler);

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(errorHandler));
			}
		}

		public class When_processing : DeadLetterTests
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

			[Fact]
			public async Task It_should_log_debug()
			{
				// Act
				await _sut.ProcessAsync(StepContext, Next, ValidatorMock.Object, ValidationResult);

				// Assert
				LoggerFactory.LogEvents.Should()
					.BeEquivalentTo(new LogEvent
					{
						Level = LogLevel.Debug,
						Message = "Validation -> {MessageType} {MessageId} is configured to be moved to error queue.",
						FormatParameters = new object[]
						{
							Message.GetMessageType(),
							Message.GetMessageId()
						}
					});
			}

			[Fact]
			public async Task Given_transport_message_is_replaced_it_should_forward_original_message_to_error_handler()
			{
				var newTransportMessage = new TransportMessage(new Dictionary<string, string>(), new byte[0]);
				StepContext.Save(newTransportMessage);

				TransportMessage originalTransportMessage = StepContext.Load<OriginalTransportMessage>().TransportMessage;
				ITransactionContext transactionContext = StepContext.Load<ITransactionContext>();

				// Act
				await _sut.ProcessAsync(StepContext, Next, ValidatorMock.Object, ValidationResult);

				// Assert
				_errorHandlerMock.Verify(
					m => m.HandlePoisonMessage(
						originalTransportMessage,
						transactionContext,
						It.Is<Exception>(ex =>
							ex.GetType() == typeof(ValidationException)
						 && ((ValidationException)ex).Errors.Equals(ValidationResult.Errors))
					),
					Times.Once
				);
				_errorHandlerMock.Verify(
					m => m.HandlePoisonMessage(
						newTransportMessage,
						It.IsAny<ITransactionContext>(),
						It.IsAny<Exception>()
					),
					Times.Never
				);
			}

			[Fact]
			public async Task It_should_forward_to_error_handler()
			{
				TransportMessage transportMessage = StepContext.Load<OriginalTransportMessage>().TransportMessage;
				ITransactionContext transactionContext = StepContext.Load<ITransactionContext>();

				// Act
				await _sut.ProcessAsync(StepContext, Next, ValidatorMock.Object, ValidationResult);

				// Assert
				_errorHandlerMock.Verify(
					m => m.HandlePoisonMessage(
						transportMessage,
						transactionContext,
						It.Is<Exception>(ex =>
							ex.GetType() == typeof(ValidationException)
						 && ((ValidationException)ex).Errors.Equals(ValidationResult.Errors))
					),
					Times.Once
				);
			}
		}
	}
}
