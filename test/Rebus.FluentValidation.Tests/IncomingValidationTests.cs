using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.FluentValidation.Fixtures;
using Rebus.FluentValidation.PipelineCompletion;
using Xunit;
using Xunit.Abstractions;

namespace Rebus.FluentValidation
{
	public class IncomingValidationTests : BusValidationTests
	{
		public IncomingValidationTests(ITestOutputHelper testOutputHelper)
			: base(testOutputHelper)
		{
		}

		[Fact]
		public async Task When_receiving_invalid_message_it_should_be_wrapped_as_invalid_message()
		{
			TestMessage receivedMessage = null;
			IValidationFailed<TestMessage> failedMessage = null;
			var sync = new ManualResetEvent(false);

			var activator = new BuiltinHandlerActivator();
			activator
				.Handle<TestMessage>(message =>
				{
					// This should not happen, but signal completion.
					receivedMessage = message;
					sync.Set();
					return Task.CompletedTask;
				})
				.Handle<IValidationFailed<TestMessage>>(failed =>
				{
					// This should happen.
					failedMessage = failed;
					sync.Set();
					return Task.CompletedTask;
				});

			using IBus bus = CreateBus(activator, o => o.ValidateIncomingMessages(_validatorFactoryMock.Object));

			// Act
			var testMessage = new TestMessage
			{
				ShouldPassValidation = false
			};

			await bus.Send(testMessage);

			// Assert
			sync.WaitOne(Debugger.IsAttached ? -1 : 5000);

			receivedMessage.Should().BeNull();

			failedMessage.Should().NotBeNull();
			failedMessage.ValidationResult.Should().NotBeNull();
			failedMessage.Headers.Should().NotBeNull().And.NotBeEmpty();
			failedMessage.Message.IsValidated.Should().BeTrue();
			failedMessage.ValidatorType.Should().Be<TestMessageValidator>();
		}

		[Fact]
		public async Task When_receiving_valid_message_it_should_flow_through_pipeline_normally()
		{
			TestMessage receivedMessage = null;
			IValidationFailed<TestMessage> failedMessage = null;
			var sync = new ManualResetEvent(false);

			var activator = new BuiltinHandlerActivator();
			activator
				.Handle<TestMessage>(message =>
				{
					// This should happen.
					receivedMessage = message;
					sync.Set();
					return Task.CompletedTask;
				})
				.Handle<IValidationFailed<TestMessage>>(failed =>
				{
					// This should not happen, but signal completion.
					failedMessage = failed;
					sync.Set();
					return Task.CompletedTask;
				});

			using IBus bus = CreateBus(activator, o => o.ValidateIncomingMessages(_validatorFactoryMock.Object));

			// Act
			var testMessage = new TestMessage
			{
				ShouldPassValidation = true
			};

			await bus.Send(testMessage);

			// Assert
			sync.WaitOne(Debugger.IsAttached ? -1 : 5000);

			failedMessage.Should().BeNull();

			receivedMessage.Should().NotBeNull();
			receivedMessage.IsValidated.Should().BeTrue();
		}

		[Fact]
		public async Task Given_that_failed_handler_is_not_registered_when_receiving_invalid_message_it_should_throw()
		{
			IValidationFailed<TestMessage> failedMessage = null;
			var sync = new ManualResetEvent(false);

			var activator = new BuiltinHandlerActivator();

			using IBus bus = CreateBus(activator, o =>
			{
				o.ValidateIncomingMessages(_validatorFactoryMock.Object);
				o.OnPipelineCompletion<IValidationFailed<TestMessage>>(failed =>
				{
					// This should happen.
					failedMessage = failed;
					sync.Set();
				});
			});

			// Act
			var testMessage = new TestMessage
			{
				ShouldPassValidation = false
			};

			await bus.Send(testMessage);

			// Assert
			sync.WaitOne(Debugger.IsAttached ? -1 : 5000);

			failedMessage.Should().NotBeNull();
			failedMessage.ValidationResult.Should().NotBeNull();
			failedMessage.Headers.Should().NotBeNull().And.NotBeEmpty();
			failedMessage.Message.IsValidated.Should().BeTrue();
			failedMessage.ValidatorType.Should().Be<TestMessageValidator>();

			bus.Dispose();
			_loggerFactory.LogEvents
				.Select(le => le.Exception)
				.Should()
				.Contain(ex => ex is MessageCouldNotBeDispatchedToAnyHandlersException);
		}
	}
}
