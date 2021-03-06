﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.FluentValidation.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Rebus.FluentValidation
{
	public class OutgoingValidationTests : BusValidationTests
	{
		public OutgoingValidationTests(ITestOutputHelper testOutputHelper)
			: base(testOutputHelper)
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
			ex.Errors.Should()
				.HaveCount(1)
				.And.Subject.Single()
				.PropertyName.Should()
				.Be(nameof(TestMessage.ShouldPassValidation));

			_loggerFactory.LogEvents
				.Select(le => le.ToString())
				.Should()
				.ContainMatch($"Debug: Message \"*{nameof(TestMessage)}*\" failed to validate.*The specified condition was not met for 'Should Pass Validation'.*");
		}

		[Fact]
		public async Task When_receiving_valid_message_it_should_not_throw()
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
