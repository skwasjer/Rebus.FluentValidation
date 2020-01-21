using System;
using FluentValidation;
using Moq;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.FluentValidation.Fixtures;
using Rebus.FluentValidation.Logging;
using Rebus.Logging;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit.Abstractions;

namespace Rebus.FluentValidation
{
	public abstract class BusValidationTests
	{
		protected const string InputQueueName = "input";
		protected const string ErrorQueueName = "error";

		protected readonly Mock<IValidatorFactory> _validatorFactoryMock;
		protected readonly XunitRebusLoggerFactory _loggerFactory;

		protected BusValidationTests(ITestOutputHelper testOutputHelper)
		{
			_validatorFactoryMock = new Mock<IValidatorFactory>();
			_validatorFactoryMock
				.Setup(m => m.GetValidator(typeof(TestMessage)))
				.Returns(new TestMessageValidator());

			_loggerFactory = testOutputHelper == null
				? null
				: new XunitRebusLoggerFactory(testOutputHelper);
		}

		protected IBus CreateBus(IHandlerActivator activator, Action<OptionsConfigurer> optionsConfigurer, InMemNetwork inMemNetwork = null)
		{
			return Configure
				.With(activator)
				.Logging(l => l.Use(_loggerFactory ?? (IRebusLoggerFactory)new NullLoggerFactory()))
				.Options(o =>
				{
					o.SimpleRetryStrategy(ErrorQueueName, 1);
					//o.LogPipeline(true);
					optionsConfigurer?.Invoke(o);
				})
				.Transport(t => t.UseInMemoryTransport(inMemNetwork ?? new InMemNetwork(), InputQueueName))
				.Routing(r => r.TypeBased().MapAssemblyOf<TestMessage>(InputQueueName))
				.Start();
		}

	}
}
