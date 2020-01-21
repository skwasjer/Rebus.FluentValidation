using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
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
			Logger = new NullLoggerFactory().GetLogger<ValidationFailedStrategyTests>();

			_tx = new RebusTransactionScope();

			var headers = new Dictionary<string, string>
			{
				{ Headers.MessageId, Guid.NewGuid().ToString() },
				{ Headers.Type, GetType().Name }
			};
			var transportMessage = new TransportMessage(headers, new byte[0]);

			StepContext = new IncomingStepContext(transportMessage, _tx.TransactionContext);
			StepContext.Save(new Message(headers, new byte[0]));

			ValidatorMock = new Mock<IValidator>();
			ValidatorMock
				.Setup(m => m.ValidateAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(ValidationResult);
		}

		public ILog Logger { get; set; }

		public IncomingStepContext StepContext { get; set; }

		public Func<Task> Next { get; set; } = () => Task.CompletedTask;

		public Mock<IValidator> ValidatorMock { get; set; }

		public ValidationResult ValidationResult { get; set; } = new ValidationResult();

		public void Dispose()
		{
			_tx?.Dispose();
		}
	}
}
