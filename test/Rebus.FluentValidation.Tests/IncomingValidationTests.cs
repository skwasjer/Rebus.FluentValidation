using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Extensions;
using Rebus.FluentValidation.Fixtures;
using Rebus.FluentValidation.Incoming;
using Rebus.FluentValidation.PipelineCompletion;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport.InMem;
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
			string messageId = null;
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
				.Handle<IValidationFailed<TestMessage>>((bus, context, failed) =>
				{
					// This should happen.
					failedMessage = failed;
					messageId = context.Message.GetMessageId();
					sync.Set();
					return Task.CompletedTask;
				});

			using (IBus bus = CreateBus(activator, o => o.ValidateIncomingMessages(_validatorFactoryMock.Object)))
			{
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
				failedMessage.Headers.Should()
					.NotBeNull()
					.And.NotBeEmpty()
					.And.ContainKey(ValidateIncomingStep.ValidatorTypeKey)
					.WhichValue.Should()
					.Be(typeof(TestMessageValidator).GetSimpleAssemblyQualifiedName());
				failedMessage.Message.IsValidated.Should().BeTrue();
				failedMessage.ValidatorType.Should().Be<TestMessageValidator>();
			}

			_loggerFactory.LogEvents
				.Select(le => le.Exception)
				.Should()
				.NotContain(ex => ex is MessageCouldNotBeDispatchedToAnyHandlersException);
			_loggerFactory.LogEvents
				.Select(le => le.ToString())
				.Should()
				.ContainMatch($"Debug: Message \"{messageId}\" failed to validate.*The specified condition was not met for 'Should Pass Validation'.*");
			_loggerFactory.LogEvents
				.Select(le => le.Message)
				.Should()
				.ContainMatch("*is configured to be wrapped as *");
		}

		[Fact]
		public async Task When_receiving_valid_message_it_should_flow_through_pipeline_normally()
		{
			TestMessage receivedMessage = null;
			Dictionary<string, string> receivedHeaders = null;
			IValidationFailed<TestMessage> failedMessage = null;
			var sync = new ManualResetEvent(false);

			var activator = new BuiltinHandlerActivator();
			activator
				.Handle<TestMessage>((bus, context, message) =>
				{
					// This should happen.
					receivedMessage = message;
					receivedHeaders = context.Message.Headers;
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

			using (IBus bus = CreateBus(activator, o => o.ValidateIncomingMessages(_validatorFactoryMock.Object)))
			{
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
				receivedHeaders.Should()
					.ContainKey(ValidateIncomingStep.ValidatorTypeKey)
					.WhichValue.Should()
					.Be(typeof(TestMessageValidator).GetSimpleAssemblyQualifiedName());
			}

			_loggerFactory.LogEvents
				.Select(le => le.Exception)
				.Should()
				.NotContain(ex => ex is MessageCouldNotBeDispatchedToAnyHandlersException);
		}

		[Fact]
		public async Task Given_that_failed_handler_is_not_registered_when_receiving_invalid_message_it_should_throw()
		{
			IValidationFailed<TestMessage> failedMessage = null;
			var sync = new ManualResetEvent(false);

			var activator = new BuiltinHandlerActivator();

			using (IBus bus = CreateBus(activator,
				o =>
				{
					o.ValidateIncomingMessages(_validatorFactoryMock.Object);
					o.OnPipelineCompletion<IValidationFailed<TestMessage>>(failed =>
					{
						// This should happen.
						failedMessage = failed;
						sync.Set();
					});
				}))
			{
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
				failedMessage.Headers.Should()
					.NotBeNull()
					.And.NotBeEmpty()
					.And.ContainKey(ValidateIncomingStep.ValidatorTypeKey)
					.WhichValue.Should()
					.Be(typeof(TestMessageValidator).GetSimpleAssemblyQualifiedName());
				failedMessage.Message.IsValidated.Should().BeTrue();
				failedMessage.ValidatorType.Should().Be<TestMessageValidator>();
			}

			_loggerFactory.LogEvents
				.Select(le => le.Exception)
				.Should()
				.Contain(ex => ex is MessageCouldNotBeDispatchedToAnyHandlersException);
		}

		[Fact]
		public async Task Given_passThrough_is_configured_when_receiving_invalid_message_it_should_be_normally_handled()
		{
			string messageId = null;
			TestMessage receivedMessage = null;
			var sync = new ManualResetEvent(false);

			var activator = new BuiltinHandlerActivator();
			activator
				.Handle<TestMessage>((bus, context, message) =>
				{
					// This should happen.
					receivedMessage = message;
					messageId = context.Message.GetMessageId();
					sync.Set();
					return Task.CompletedTask;
				});

			using (IBus bus = CreateBus(activator,
				o =>
				{
					o.ValidateIncomingMessages(_validatorFactoryMock.Object,
						v => v.PassThrough<TestMessage>()
					);
				}))
			{
				// Act
				var testMessage = new TestMessage
				{
					ShouldPassValidation = false
				};

				await bus.Send(testMessage);

				// Assert
				sync.WaitOne(Debugger.IsAttached ? -1 : 5000);

				receivedMessage.Should().NotBeNull();
				receivedMessage.IsValidated.Should().BeTrue();
			}

			_loggerFactory.LogEvents
				.Select(le => le.Exception)
				.Should()
				.NotContain(ex => ex is MessageCouldNotBeDispatchedToAnyHandlersException);
			_loggerFactory.LogEvents
				.Select(le => le.ToString())
				.Should()
				.ContainMatch($"Debug: Message \"{messageId}\" failed to validate.*The specified condition was not met for 'Should Pass Validation'.*");
			_loggerFactory.LogEvents
				.Select(le => le.Message)
				.Should()
				.ContainMatch("*is configured to pass through.");
		}

		[Fact]
		public async Task Given_drop_is_configured_when_receiving_invalid_message_it_should_be_dropped()
		{
			string messageId = null;
			TestMessage receivedMessage = null;
			TestMessage droppedMessage = null;
			var sync = new ManualResetEvent(false);

			var activator = new BuiltinHandlerActivator();
			activator
				.Handle<TestMessage>(message =>
				{
					// This should not happen, but signal completion.
					receivedMessage = message;
					sync.Set();
					return Task.CompletedTask;
				});

			using (IBus bus = CreateBus(activator,
				o =>
				{
					o.ValidateIncomingMessages(_validatorFactoryMock.Object,
						v => v.Drop<TestMessage>()
					);
					o.OnPipelineCompletion<TestMessage>(failed =>
					{
						droppedMessage = failed;
						messageId = MessageContext.Current.Message.GetMessageId();
						sync.Set();
					});
				}))
			{
				// Act
				var testMessage = new TestMessage
				{
					ShouldPassValidation = false
				};

				await bus.Send(testMessage);

				// Assert
				sync.WaitOne(Debugger.IsAttached ? -1 : 5000);

				receivedMessage.Should().BeNull();

				droppedMessage.Should().NotBeNull();
				droppedMessage.IsValidated.Should().BeTrue();
			}

			_loggerFactory.LogEvents
				.Select(le => le.Exception)
				.Should()
				.NotContain(ex => ex is MessageCouldNotBeDispatchedToAnyHandlersException);
			_loggerFactory.LogEvents
				.Select(le => le.ToString())
				.Should()
				.ContainMatch($"Debug: Message \"{messageId}\" failed to validate.*The specified condition was not met for 'Should Pass Validation'.*");
			_loggerFactory.LogEvents
				.Select(le => le.Message)
				.Should()
				.ContainMatch("*is configured to be dropped.");
		}

		[Fact]
		public async Task Given_deadLetter_is_configured_when_receiving_invalid_message_it_should_be_moved_to_error_queue()
		{
			string messageId = null;
			TestMessage receivedMessage = null;
			var sync = new ManualResetEvent(false);

			var inMemNetwork = new InMemNetwork();
			var activator = new BuiltinHandlerActivator();
			activator
				.Handle<TestMessage>(message =>
				{
					// This should not happen, but signal completion.
					receivedMessage = message;
					sync.Set();
					return Task.CompletedTask;
				});

			using (IBus bus = CreateBus(activator,
				o =>
				{
					o.ValidateIncomingMessages(_validatorFactoryMock.Object,
						v => v.DeadLetter<TestMessage>()
					);
					o.OnPipelineCompletion<TestMessage>(failed =>
					{
						messageId = MessageContext.Current.Message.GetMessageId();
						sync.Set();
					});
				},
				inMemNetwork))
			{
				// Act
				var testMessage = new TestMessage
				{
					ShouldPassValidation = false
				};

				await bus.Send(testMessage);

				// Assert
				sync.WaitOne(Debugger.IsAttached ? -1 : 5000);

				receivedMessage.Should().BeNull();

				var errorQueueMessages = inMemNetwork.GetMessages(ErrorQueueName).ToList();
				InMemTransportMessage message = errorQueueMessages.Should()
					.HaveCount(1)
					.And.Subject.Single();
				message.Headers.Should()
					.ContainKey(Headers.ErrorDetails)
					.WhichValue.Should()
					.Match($"*{nameof(TestMessage.ShouldPassValidation)}: The specified condition was not met*");
				message.Headers.Should()
					.ContainKey(Headers.Type)
					.WhichValue.Should()
					.StartWith(typeof(TestMessage).FullName);
			}

			_loggerFactory.LogEvents
				.Select(le => le.Exception)
				.Should()
				.NotContain(ex => ex is MessageCouldNotBeDispatchedToAnyHandlersException);
			_loggerFactory.LogEvents
				.Select(le => le.ToString())
				.Should()
				.ContainMatch($"Debug: Message \"{messageId}\" failed to validate.*The specified condition was not met for 'Should Pass Validation'.*");
			_loggerFactory.LogEvents
				.Select(le => le.Message)
				.Should()
				.ContainMatch("*is configured to be moved to error queue.")
				.And
				.ContainMatch("Moving message with ID *");
		}

		[Fact]
		public async Task Given_multiple_are_configured_when_receiving_invalid_message_it_should_select_appropriate_strategy()
		{
			string messageId1 = null;
			string messageId2 = null;
			TestMessage1 receivedMessage1 = null;
			TestMessage2 receivedMessage2 = null;
			var sync = new AutoResetEvent(false);

			var activator = new BuiltinHandlerActivator();
			activator
				.Handle<TestMessage1>((bus, context, message) =>
				{
					// This should happen.
					receivedMessage1 = message;
					messageId1 = context.Message.GetMessageId();
					sync.Set();
					return Task.CompletedTask;
				})
				.Handle<TestMessage2>((bus, context, message) =>
				{
					// This should not happen.
					receivedMessage2 = message;
					messageId2 = context.Message.GetMessageId();
					sync.Set();
					return Task.CompletedTask;
				});

			using (IBus bus = CreateBus(activator,
				o =>
				{
					o.ValidateIncomingMessages(_validatorFactoryMock.Object,
						v => v
							.PassThrough<TestMessage1>()
							.Drop<TestMessage2>()
					);

					o.OnPipelineCompletion<TestMessage2>(failed =>
					{
						receivedMessage2 = failed;
						messageId2 = MessageContext.Current.Message.GetMessageId();
						sync.Set();
					});
				}))
			{
				// Act
				await bus.Send(new TestMessage1
				{
					ShouldPassValidation = false
				});

				await bus.Send(new TestMessage2
				{
					ShouldPassValidation = false
				});

				// Assert
				sync.WaitOne(Debugger.IsAttached ? -1 : 5000);
				sync.WaitOne(Debugger.IsAttached ? -1 : 5000);

				receivedMessage1.Should().NotBeNull();
				receivedMessage2.Should().NotBeNull();
			}

			var logMessages = _loggerFactory.LogEvents
				.Select(le => le.ToString())
				.ToList();

			logMessages
				.Should()
				.ContainMatch($"*\"{messageId1}\" is configured to pass through.*");
			logMessages
				.Should()
				.ContainMatch($"*\"{messageId2}\" is configured to be dropped.*");
		}
	}
}
