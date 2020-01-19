using System;
using FluentValidation;
using Moq;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.FluentValidation.Fixtures;
using Rebus.FluentValidation.Logging;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit.Abstractions;

namespace Rebus.FluentValidation
{
	public abstract class BusValidationTests
	{
		private const string InputQueueName = "input";

		protected readonly Mock<IValidatorFactory> _validatorFactoryMock;
		protected readonly XunitRebusLoggerFactory _loggerFactory;

		protected BusValidationTests(ITestOutputHelper testOutputHelper)
		{
			_validatorFactoryMock = new Mock<IValidatorFactory>();
			_validatorFactoryMock
				.Setup(m => m.GetValidator(typeof(TestMessage)))
				.Returns(new TestMessageValidator());

			_loggerFactory = new XunitRebusLoggerFactory(testOutputHelper);
		}

		protected IBus CreateBus(IHandlerActivator activator, Action<OptionsConfigurer> optionsConfigurer)
		{
			return Configure
				.With(activator)
				.Logging(l => l.Use(_loggerFactory))
				.Options(o =>
				{
					//o.LogPipeline(true);
					optionsConfigurer?.Invoke(o);
				})
				.Transport(t => t.UseInMemoryTransport(new InMemNetwork(), InputQueueName))
				.Routing(r => r.TypeBased().MapAssemblyOf<TestMessage>(InputQueueName))
				.Start();
		}

	}
}
