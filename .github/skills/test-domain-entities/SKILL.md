---
name: test-domain-entities
description: "Write unit tests for domain entities and aggregates verifying factory creation, business invariants, and domain events."
---

> Evidence: value gate verified the facts below against this repository.

Write unit tests for domain entities and aggregates verifying factory creation, business invariants, and domain events.

Applies in: `claim-flow-claims-domain-unit-tests`, `claim-flow-documents-unit-tests`.

## What is specific to this repository

- A newly declared claim has StartReview as its only allowed action. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L20-L20]
- Declaring a valid claim raises a single ClaimDeclared domain event. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L28-L28]
- Attempting a forbidden claim transition returns an error with ErrorType.Conflict. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L62-L62]
- Reviewing, approving, and settling a claim records status transitions to UnderReview, Approved, and Settled in history. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L24-L24]
- An uploaded claim document formats its storage key as the claim ID and document ID formatted as 32 digits separated by a slash. [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L22-L22]
- Uploading an oversized file with an unknown format simultaneously returns both TooLarge and UnsupportedFormat errors. [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L30-L30]

## Rules this change touches

- [Invariant] Sanitized file name length is bounded to ClaimDocument.FileNameMaxLength (approved, conviction 1.00, tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L59-L62)
- [Invariant] Uploading a claim document stores the detected MIME content type, the uploader's name, and assigns a storage key matching the format '{ClaimId:N}/{document.Id.Value:N}'. (approved, conviction 1.00, tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L17-L25)
- [Invariant] A successfully declared claim is created with status Declared, empty history, and StartReview as its only allowed action. (approved, conviction 1.00, tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L8-L21)
- [BusinessRule] Uploading an empty document of size 0 bytes is rejected with DocumentErrors.Empty. (approved, conviction 1.00, tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L35-L39)
- [BusinessRule] Uploading a document to a claim that already has ClaimDocument.MaxDocumentsPerClaim documents is rejected with DocumentErrors.TooManyDocuments. (approved, conviction 1.00, tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L41-L45)
- [BusinessRule] Document file names are sanitized to keep display names only, stripping directory paths and HTML markup, defaulting to 'document' if empty or whitespace, and appending the detected format's file extension. (approved, conviction 1.00, tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L47-L59)
- [BusinessRule] A claim's allowed actions are governed by its status: Declared permits StartReview; UnderReview permits RequestInformation, Approve, and Reject; InformationRequested permits ResumeReview and Reject; and Approved permits Settle. (approved, conviction 1.00, tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L121-L143)
- [BusinessRule] Uploading a document with an unknown format and a size exceeding ClaimDocument.MaxSizeBytes is rejected with both DocumentErrors.TooLarge and DocumentErrors.UnsupportedFormat. (approved, conviction 1.00, tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L27-L33)
- [BusinessRule] UnderReview, so it follows the normal path again: request information, approve or reject. (inferred, not yet reviewed, conviction 0.95, tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L121-L143)

## Start here

`tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs` [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L8-L47] is the worked example for this change, as it stands in the repository:

```csharp
    [Fact]
    public void Declare_CreatesDeclaredClaim_WhenDataIsValid()
    {
        var result = Claim.Declare(ValidData(), Number, Handler, Now);

        result.IsSuccess.ShouldBeTrue();
        var claim = result.Value;
        claim.Status.ShouldBe(ClaimStatus.Declared);
        claim.Number.ShouldBe(Number);
        claim.DeclaredAt.ShouldBe(Now);
        claim.DeclaredByName.ShouldBe(Handler.Name);
        claim.History.ShouldBeEmpty();
        claim.AllowedActions.ShouldBe([ClaimAction.StartReview]);
    }

    [Fact]
    public void Declare_RaisesClaimDeclared()
    {
        var claim = Declared();

        var domainEvent = claim.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ClaimDeclared>();
        domainEvent.ClaimId.ShouldBe(claim.Id);
        domainEvent.Number.ShouldBe(claim.Number);
    }

    [Fact]
    public void Declare_NormalizesInput()
    {
        var claim = Claim.Declare(ValidData() with { PolicyNumber = "  pol-123456 ", Description = "  Broken window.  ", ClaimedAmount = 10.005m }, Number, Handler, Now).Value;

        claim.PolicyNumber.ShouldBe("POL-123456");
        claim.Description.ShouldBe("Broken window.");
        claim.ClaimedAmount.ShouldBe(10.00m);
    }

    [Fact]
    public void Declare_ReturnsEveryViolatedRule_AtOnce()
    {
        var data = new DeclareClaimData("123", ClaimType.Home, new DateOnly(2026, 10, 3), "short", 0m);

```

## The same change elsewhere in this repository

- `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs` [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L17-L17]: `claim.StartReview(Handler, Later).IsSuccess.ShouldBeTrue();`
- `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs` [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L62-L62]: `result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);`
- `tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs` [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L77-L77]: `claim.Reject(Handler, reason, Later).Errors.ShouldBe([ClaimErrors.ReasonRequired]);`
- `tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs` [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L28-L28]: `var result = ClaimDocument.Upload(ClaimId, "x.pdf", null, ClaimDocument.MaxSizeBytes + 1, new string('a', 64), 0, Lea, Now);`
- `tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs` [tests/ClaimFlow.Documents.UnitTests/ClaimDocumentTests.cs:L30-L30]: `result.Errors.ShouldBe([DocumentErrors.TooLarge, DocumentErrors.UnsupportedFormat], ignoreOrder: true);`

## Patterns

Required:
- `DomainEvents`
- `Errors`
- `IsSuccess`
- `Result`
- `ShouldBe`
Forbidden:
- Avoid `Assert.Equal` in this change.
- Avoid `Assert.True` in this change.
- Avoid `throw new` in this change.

## Workflow

1. Set up test fixtures or reuse an existing builder like ClaimBuilder with fixed actors (Handler, Manager) and timestamps (Now) to construct the aggregate in a known state. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L2-L2]
2. Invoke the aggregate factory method (e.g., Claim.Declare, ClaimDocument.Upload) or workflow action method (e.g., StartReview, Approve, Settle, Reject). [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L11-L11]
3. Assert operation success using result.IsSuccess.ShouldBeTrue(), inspecting the aggregate's Status, AllowedActions, and audit History trail. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L13-L13]
4. Verify domain events raised on the aggregate using DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<T>() or clear them before testing transitions. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L28-L28]
5. Assert validation failures using result.Errors.ShouldBe([...], ignoreOrder: true) to confirm all violated rules are returned at once without throwing exceptions. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L51-L51]
6. Verify invalid state transitions return ErrorType.Conflict and leave the aggregate state, history, and domain events untouched. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L62-L62]
7. Set up test data using domain builder fixtures or factory helper methods (e.g. ClaimBuilder or local Upload helper). [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L2-L2]
8. Invoke the domain factory method or state transition action (e.g. Claim.Declare, Upload, StartReview, Approve). [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L11-L11]
9. For success scenarios, assert IsSuccess is true, unwrap Value, and verify updated aggregate properties. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L13-L13]
10. Verify raised DomainEvents match expected event types and payload properties. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L28-L28]
11. For failure scenarios, assert IsFailure is true and check expected error codes or types. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L50-L50]
12. Verify multi-error validations return all expected domain errors simultaneously using ignoreOrder: true. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L53-L53]

## Checklist

- [ ] Does the test use fixed Actor instances (Handler, Manager) and deterministic DateTimeOffset timestamps? [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L9-L9]
- [ ] Are validation errors checked against domain constants (ClaimErrors, DocumentErrors) using ignoreOrder: true when testing multiple invalid inputs? [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L52-L52]
- [ ] Are domain events cleared with ClearDomainEvents() before asserting transitions if intermediate steps emit events? [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L15-L15]
- [ ] Are aggregate AllowedActions and History collections verified alongside Status changes? [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L20-L20]
- [ ] Do forbidden transitions return an error with ErrorType.Conflict and preserve existing aggregate state? [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimWorkflowTests.cs:L62-L62]
- [ ] Failure assertions check specific domain error codes (e.g. ClaimErrors, DocumentErrors). [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L52-L52]
- [ ] Multi-error assertions specify ignoreOrder: true to avoid ordering dependencies. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L53-L53]
- [ ] State transitions verify resulting Status, History entries, and AllowedActions. [tests/ClaimFlow.Claims.Domain.UnitTests/ClaimDeclarationTests.cs:L20-L20]
