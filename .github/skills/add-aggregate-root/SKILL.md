---
name: add-aggregate-root
description: "Define a domain aggregate root inheriting from AggregateRoot<TId> that buffers domain events for outbox persistence."
---

> Evidence: value gate verified the facts below against this repository.

Define a domain aggregate root inheriting from AggregateRoot<TId> that buffers domain events for outbox persistence.

Applies in: `claim-flow-claims-domain`, `claim-flow-shared-kernel`.

## What is specific to this repository

- Expected business failures are returned as Result values holding domain errors rather than throwing exceptions. [src/ClaimFlow.SharedKernel/Result.cs:L3-L6]
- Aggregates define a get-only Version property of type uint that serves as an optimistic concurrency token mapped to PostgreSQL's xmin column. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L76-L78]

## Rules this change touches

- [Invariant] A claim approval cannot exceed the user's approval authority limit in EUR; amounts exceeding the authority limit require approval by a claims manager. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L41-L43)
- [Invariant] The internal _domainEvents list is encapsulated and exposed externally only as a read-only collection via DomainEvents.AsReadOnly(), preventing external mutation without going through Raise or ClearDomainEvents. (approved, conviction 1.00, src/ClaimFlow.SharedKernel/AggregateRoot.cs:L21-L24)
- [Invariant] Every status transition must conform to the static Workflow mapping dictionary; on transition, Status is updated, LastUpdatedAt is set to now, a ClaimStatusChange record is appended to _history, and a ClaimStatusChanged domain event is raised. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L199-L210)
- [Invariant] Amounts in the claim domain are always rounded to 2 decimal places using MidpointRounding.ToEven (banker's rounding). (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L195-L195)
- [BusinessRule] The claimed amount must be strictly greater than 0 EUR and cannot exceed Claim.MaxClaimedAmount EUR. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L22-L23)
- [BusinessRule] Approving a claim transitions it from UnderReview to Approved, requiring the rounded approved amount to be greater than zero, no greater than the ClaimedAmount, and within the approving actor's authority limit. Upon success, ApprovedAmount and ApprovedById are recorded. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L125-L148)
- [BusinessRule] The plan's limit applies: 12 months from the date of service for out-of-network claims. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L28-L29)
- [BusinessRule] AllowedActionsFor filters the claim's currently valid workflow actions to exclude ClaimAction.Settle if the specified actor was the one who approved the claim. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L82-L87)
- [BusinessRule] A claim action cannot be performed if it is not a valid transition from the claim's current status. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L43-L44)
- [BusinessRule] Under the four-eyes principle, the user who approved a claim is prohibited from releasing its payment. (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L37-L38)

## Context

- AggregateRoot<TId> is an abstract base class serving as a consistency boundary for domain entity clusters, constrained to value type identifiers (where TId : struct) and implementing IHasDomainEvents. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L15-L20]
- The Result class models an operation outcome as a value instead of throwing exceptions for expected business failures, collecting zero or more Error instances to allow multi-field validation error reporting in a single round trip. [src/ClaimFlow.SharedKernel/Result.cs:L3-L15]
- Claim defines an optimistic concurrency token property Version of type uint mapped to PostgreSQL's xmin system column, where only the database writes the column value and EF Core populates the compiler-generated backing field. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L66-L68]

## Start here

`src/ClaimFlow.SharedKernel/Result.cs` [src/ClaimFlow.SharedKernel/Result.cs:L14-L45] is the worked example for this change, as it stands in the repository:

```csharp

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new([]);

    public static Result<T> Success<T>(T value) => new(value, []);

    public static Result Failure(params Error[] errors) => new(EnsureNotEmpty(errors));

    public static Result<T> Failure<T>(params Error[] errors) => new(default, EnsureNotEmpty(errors));

    public static implicit operator Result(Error error) => Failure(error);

    private static Error[] EnsureNotEmpty(Error[] errors) =>
        errors.Length > 0 ? errors : throw new ArgumentException("A failure needs at least one error.", nameof(errors));
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, IReadOnlyList<Error> errors)
        : base(errors) => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}
```

## The same change elsewhere in this repository

- `src/ClaimFlow.SharedKernel/AggregateRoot.cs` [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L3-L3]: `public interface IDomainEvent`
- `src/ClaimFlow.SharedKernel/AggregateRoot.cs` [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L8-L8]: `public interface IHasDomainEvents`
- `src/ClaimFlow.SharedKernel/AggregateRoot.cs` [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L19-L19]: `public abstract class AggregateRoot<TId> : IHasDomainEvents`
- `src/ClaimFlow.SharedKernel/AggregateRoot.cs` [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L30-L30]: `protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);`
- `src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs` [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L22-L22]: `public sealed partial class Claim : AggregateRoot<ClaimId>`
- `src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs` [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L31-L31]: `public static readonly Error NotFound =`

## Patterns

Required:
- `AggregateRoot`
- `Error`
- `IDomainEvent`
- `Result`
Forbidden:
- Avoid `DbContext` in this change.
- Avoid `throw new BusinessException` in this change.

## Workflow

1. Inherit from AggregateRoot<TId> specifying a struct identifier type for TId. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L19-L19]
2. Create aggregate class inheriting from AggregateRoot<TId> in the module's Domain project. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L19-L19]
3. Implement static factory methods returning Result<T> rather than throwing exceptions. [src/ClaimFlow.SharedKernel/Result.cs:L7-L7]
4. Collect invariant violations and return Result.Failure<T> using domain Error instances. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L102-L102]
5. Declare public uint Version { get; } to serve as the optimistic concurrency token mapped to PostgreSQL xmin. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L76-L76]
6. Record state changes as domain events implementing IDomainEvent via the protected Raise method [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L3-L3]

## Checklist

- [ ] Verify the class inherits from AggregateRoot<TId> with a struct constraint on TId. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L19-L19]
- [ ] Verify internal state mutations invoke Raise to buffer an IDomainEvent. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L3-L3]
- [ ] Verify the root implements IHasDomainEvents through AggregateRoot<TId>. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L8-L8]
- [ ] Verify DomainEvents returns a read-only collection of buffered events. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L10-L10]
- [ ] Aggregate inherits from AggregateRoot<TId>. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L19-L19]
- [ ] Creation goes through a static factory method returning Result<T>. [src/ClaimFlow.SharedKernel/Result.cs:L7-L7]
- [ ] Invariants are validated and return Result.Failure with domain Error instances. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L102-L102]
- [ ] Actor identities use Actor from ClaimFlow.SharedKernel. [src/ClaimFlow.SharedKernel/Result.cs:L1-L1]
- [ ] Verify that uint Version is declared for PostgreSQL xmin optimistic concurrency. [src/Modules/Claims/ClaimFlow.Claims.Domain/Claim.cs:L76-L76]
- [ ] Verify that state mutations raise domain events using Raise instead of mutating external state directly. [src/ClaimFlow.SharedKernel/AggregateRoot.cs:L30-L30]
- [ ] Ensure domain errors use factory methods (Error.Validation, Error.Conflict, Error.Forbidden, Error.NotFound) with namespaced string codes [src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimErrors.cs:L8-L8]
- [ ] Ensure methods return Result or Result<T> rather than throwing business exceptions [src/ClaimFlow.SharedKernel/Result.cs:L7-L7]
