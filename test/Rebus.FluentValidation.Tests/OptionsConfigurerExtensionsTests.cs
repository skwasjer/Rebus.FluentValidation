using System;
using FluentAssertions;
using FluentValidation;
using Moq;
using Rebus.Activation;
using Rebus.Config;
using Rebus.FluentValidation.Incoming;
using Rebus.FluentValidation.Outgoing;
using Xunit;

namespace Rebus.FluentValidation
{
	public class OptionsConfigurerExtensionsTests
	{
		private OptionsConfigurer _sut;

		public OptionsConfigurerExtensionsTests()
		{
			Configure.With(new BuiltinHandlerActivator())
				.Options(configurer => _sut = configurer);
		}

		public class When_configuring_incoming_and_outgoing : OptionsConfigurerExtensionsTests
		{
			[Fact]
			public void It_should_register_components()
			{
				var onFailedMock = new Mock<Action<ValidationConfigurer>>();

				// Act
				_sut.ValidateMessages(Mock.Of<IValidatorFactory>(), onFailedMock.Object);

				// Assert
				_sut.Has<ValidateIncomingStep>().Should().BeTrue();
				_sut.Has<ValidateOutgoingStep>().Should().BeTrue();
				onFailedMock.Verify(m => m(
					It.Is<ValidationConfigurer>(vc => vc != null)),
					Times.Once);
			}
		}

		public class When_configuring_incoming : OptionsConfigurerExtensionsTests
		{
			[Fact]
			public void It_should_register_components()
			{
				var onFailedMock = new Mock<Action<ValidationConfigurer>>();

				// Act
				_sut.ValidateIncomingMessages(Mock.Of<IValidatorFactory>(), onFailedMock.Object);

				// Assert
				_sut.Has<ValidateIncomingStep>().Should().BeTrue();
				_sut.Has<ValidateOutgoingStep>().Should().BeFalse();
				onFailedMock.Verify(m => m(
						It.Is<ValidationConfigurer>(vc => vc != null)),
					Times.Once);
			}
		}

		public class When_configuring_outgoing : OptionsConfigurerExtensionsTests
		{
			[Fact]
			public void It_should_register_components()
			{
				// Act
				_sut.ValidateOutgoingMessages(Mock.Of<IValidatorFactory>());

				// Assert
				_sut.Has<ValidateIncomingStep>().Should().BeFalse();
				_sut.Has<ValidateOutgoingStep>().Should().BeTrue();
			}
		}

		public class When_configuring_with_null_argument : OptionsConfigurerExtensionsTests
		{
			[Fact]
			public void With_null_configurer_it_should_throw()
			{
				OptionsConfigurer configurer = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => configurer.ValidateMessages(Mock.Of<IValidatorFactory>(), _ => { });

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(configurer));
			}

			[Fact]
			public void With_null_validatorFactory_it_should_throw()
			{
				IValidatorFactory validatorFactory = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => _sut.ValidateMessages(validatorFactory, _ => { });

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(validatorFactory));
			}

			[Fact]
			public void With_null_onFailed_it_should_not_throw()
			{
				Action<ValidationConfigurer> onFailed = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => _sut.ValidateMessages(Mock.Of<IValidatorFactory>(), onFailed);

				// Assert
				act.Should().NotThrow();
			}
		}

		public class When_configuring_incoming_with_null_argument : OptionsConfigurerExtensionsTests
		{
			[Fact]
			public void With_null_configurer_it_should_throw()
			{
				OptionsConfigurer configurer = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => configurer.ValidateIncomingMessages(Mock.Of<IValidatorFactory>(), _ => { });

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(configurer));
			}

			[Fact]
			public void With_null_validatorFactory_it_should_throw()
			{
				IValidatorFactory validatorFactory = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => _sut.ValidateIncomingMessages(validatorFactory, _ => { });

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(validatorFactory));
			}

			[Fact]
			public void With_null_onFailed_it_should_not_throw()
			{
				Action<ValidationConfigurer> onFailed = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => _sut.ValidateIncomingMessages(Mock.Of<IValidatorFactory>(), onFailed);

				// Assert
				act.Should().NotThrow();
			}
		}

		public class When_configuring_outgoing_with_null_argument : OptionsConfigurerExtensionsTests
		{
			[Fact]
			public void With_null_configurer_it_should_throw()
			{
				OptionsConfigurer configurer = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => configurer.ValidateOutgoingMessages(Mock.Of<IValidatorFactory>());

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(configurer));
			}

			[Fact]
			public void With_null_validatorFactory_it_should_throw()
			{
				IValidatorFactory validatorFactory = null;

				// ReSharper disable once ExpressionIsAlwaysNull
				// ReSharper disable once ObjectCreationAsStatement
				Action act = () => _sut.ValidateOutgoingMessages(validatorFactory);

				// Assert
				act.Should()
					.Throw<ArgumentNullException>()
					.Which.ParamName.Should()
					.Be(nameof(validatorFactory));
			}
		}
	}
}
