# Rebus.FluentValidation

Message validation using [FluentValidation](https://fluentvalidation.net/) for [Rebus](https://github.com/rebus-org/Rebus).

---

[![Build status](https://ci.appveyor.com/api/projects/status/n3t7ny3j7cryt92t/branch/master?svg=true)](https://ci.appveyor.com/project/skwasjer/rebus-fluentvalidation)
[![Tests](https://img.shields.io/appveyor/tests/skwasjer/Rebus.FluentValidation/master.svg)](https://ci.appveyor.com/project/skwasjer/rebus-fluentvalidation/build/tests)
[![codecov](https://codecov.io/gh/skwasjer/Rebus.FluentValidation/branch/master/graph/badge.svg)](https://codecov.io/gh/skwasjer/Rebus.FluentValidation)

| | | |
|---|---|---|
| `Rebus.FluentValidation` | [![NuGet](https://img.shields.io/nuget/v/Rebus.FluentValidation.svg)](https://www.nuget.org/packages/Rebus.FluentValidation/) [![NuGet](https://img.shields.io/nuget/dt/Rebus.FluentValidation.svg)](https://www.nuget.org/packages/Rebus.FluentValidation/) | |

## Usage example ###

```csharp
// FluentValidation factory, get from IoC container of choice.
IValidatorFactory validatorFactory = .. 

// Configure Rebus handlers and options.
Configure
    .With(..)
    .Options(o =>	{
        // Throws on Send/Publish.
        o.ValidateOutgoingMessages(validatorFactory);
        // Configure strategy per incoming message.
        o.ValidateIncomingMessages(validatorFactory, v =>
        {
            // Move messages of type MessageType1 to error queue.
            v.DeadLetter<MessageType1>();
            // Drop messages of type MessageType2.
            v.Drop<MessageType2>();
            // Do nothing to messages of type MessageType3 (just log warn).
            v.PassThrough<MessageType3>();
        });
    });

// If not explicitly configured, messages will be wrapped in IValidationFailed<> 
// and can be handled using custom handler logic:
public class MyService : IHandleMessages<IValidationFailed<MessageType4>>
{
    Task Handle(IValidationFailed<MessageType4> message)
    {
        // Custom handler for validation failure.
    }
}
```

