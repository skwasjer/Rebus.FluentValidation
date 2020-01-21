using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Rebus.Extensions;
using Rebus.FluentValidation.Fixtures;
using Rebus.FluentValidation.Logging;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace Rebus.FluentValidation.Incoming.Handlers
{
	public abstract class ValidationFailedStrategyTests : IDisposable
	{
		private readonly RebusTransactionScope _tx;

		public ValidationFailedStrategyTests()
		{
			LoggerFactory = new XunitRebusLoggerFactory();
			Logger = LoggerFactory.GetLogger<ValidationFailedStrategyTests>();

			_tx = new RebusTransactionScope();

			var headers = new Dictionary<string, string>
			{
				{ Headers.MessageId, Guid.NewGuid().ToString() },
				{ Headers.Type, typeof(TestMessage).GetSimpleAssemblyQualifiedName() }
			};
			var transportMessage = new TransportMessage(headers, new byte[0]);

			StepContext = new IncomingStepContext(transportMessage, _tx.TransactionContext);
			StepContext.Save(Message = new Message(headers, new byte[0]));

			ValidatorMock = new Mock<IValidator>();
			ValidatorMock
				.Setup(m => m.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(ValidationResult);
		}

		protected XunitRebusLoggerFactory LoggerFactory { get; }

		protected ILog Logger { get; }

		protected IncomingStepContext StepContext { get; }

		protected Message Message { get; }

		protected Func<Task> Next { get; set; } = () => Task.CompletedTask;

		protected Mock<IValidator> ValidatorMock { get; }

		protected ValidationResult ValidationResult { get; set; } = new ValidationResult();

		public void Dispose()
		{
			_tx?.Dispose();
		}
	}
}
