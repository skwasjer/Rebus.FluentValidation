using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.FluentValidation.Fixtures;
using Xunit;

namespace Rebus.FluentValidation
{
	public class OutgoingValidationTests : BusValidationTests
	{
		public OutgoingValidationTests()
			: base(null)
		{
		}

		[Fact]
		public async Task When_sending_invalid_message_it_should_throw()
		{
			var activator = new BuiltinHandlerActivator();
			using IBus bus = CreateBus(activator, o => o.ValidateOutgoingMessages(_validatorFactoryMock.Object));

			// Act
			var testMessage = new TestMessage
			{
				ShouldPassValidation = false
			};

			Func<Task> act = () => bus.Send(testMessage);

			// Assert
			ValidationException ex = (await act.Should().ThrowAsync<ValidationException>()).Which;
			ex.Errors.Should().HaveCount(1);
		}

		[Fact]
		public async Task When_receiving_valid_message_it_should_be_wrapped_as_invalid_message()
		{
			var activator = new BuiltinHandlerActivator();
			using IBus bus = CreateBus(activator, o => o.ValidateOutgoingMessages(_validatorFactoryMock.Object));

			// Act
			var testMessage = new TestMessage
			{
				ShouldPassValidation = true
			};

			Func<Task> act = () => bus.Send(testMessage);

			// Assert
			await act.Should().NotThrowAsync();
		}
	}
}
