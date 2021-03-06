# Rebus.FluentValidation

Message validation using [FluentValidation](https://fluentvalidation.net/) for [Rebus](https://github.com/rebus-org/Rebus).

---

[![Build status](https://ci.appveyor.com/api/projects/status/ucj1da4jgoi0xjd4/branch/master?svg=true)](https://ci.appveyor.com/project/skwasjer/rebus-fluentvalidation)
[![Tests](https://img.shields.io/appveyor/tests/skwasjer/rebus-fluentvalidation/master.svg)](https://ci.appveyor.com/project/skwasjer/rebus-fluentvalidation/build/tests)
[![codecov](https://codecov.io/gh/skwasjer/Rebus.FluentValidation/branch/master/graph/badge.svg)](https://codecov.io/gh/skwasjer/Rebus.FluentValidation)

| | | |
|---|---|---|
| `Rebus.FluentValidation` | [![NuGet](https://img.shields.io/nuget/v/Rebus.FluentValidation.svg)](https://www.nuget.org/packages/Rebus.FluentValidation/) [![NuGet](https://img.shields.io/nuget/dt/Rebus.FluentValidation.svg)](https://www.nuget.org/packages/Rebus.FluentValidation/) | |

## Usage example ###

```csharp
// Create validators and register with FluentValidation using IoC container of choice.
public class MessageType1Validator : AbstractValidator<MessageType1>
{
    public MessageType1Validator()
    {
        RuleFor(x => ...);
    }
}

// Get FluentValidation factory from IoC container.
IValidatorFactory validatorFactory = .. 

// Configure Rebus handlers and options.
Configure
    .With(..)
    .Options(o =>
    {
        // Throws on Send/Publish.
        o.ValidateOutgoingMessages(validatorFactory);
        // Configure strategy per incoming message.
        o.ValidateIncomingMessages(validatorFactory, v =>
        {
            // Configure how messages that failed validation should be 
            // handled:
            
            // Move messages of type MessageType1 to error queue.
            v.DeadLetter<MessageType1>();
            // Drop messages of type MessageType2.
            v.Drop<MessageType2>();
            // Do nothing to messages of type MessageType3 (just log warn)
            // allowing them to be handled normally with IHandleMessages<MessageType3>
            v.PassThrough<MessageType3>();
        });
    });

// If not explicitly configured how to handle incoming messages that failed
// validation, a message will be wrapped in IValidationFailed<> and should be 
// handled using custom handler logic:
public class MyService : IHandleMessages<IValidationFailed<MessageType4>>
{
    Task Handle(IValidationFailed<MessageType4> message)
    {
        // Custom handler for validation failure.
    }
}
```

