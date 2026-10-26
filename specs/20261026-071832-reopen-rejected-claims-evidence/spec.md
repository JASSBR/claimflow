# Feature Specification: Reopen Rejected Claims with New Evidence

**Feature Branch**: `20261026-071832-reopen-rejected-claims-evidence`

**Created**: 2026-10-26

**Status**: Draft

**Input**: Backlog epic "Reopen Rejected Claims with New Evidence" (gradera:3764f644-146e-434b-8f4d-ba322fd859df, approved), drafted by the Gradera Context Engine from its stories, acceptance criteria and the estate of `git:claimflow`

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reopen a rejected claim within 30 days (Priority: P1)

As a claims handler, I want to reopen a Rejected claim into UnderReview status within 30 days of its rejection, so that I can process the claim again when new evidence arrives. (gradera:1d0ef4a0-6281-4eb5-80b1-e75b245e8373, approved)

**Why this priority**: Ordered as the backlog ranks it.

**Independent Test**: Can be fully tested by the acceptance scenarios below, each of which cites the estate it was drawn from.

**Acceptance Scenarios**:

1. *Settled claims cannot be reopened*: **Given** a claim in Settled status, **When** the claims handler views the claim, **Then** no action is permitted (gradera:2246c819-8075-4173-a11d-4187dc882147, approved)

---

### Edge Cases

- [NEEDS CLARIFICATION: the backlog records no edge case; run `/speckit.clarify`]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST meet this criterion: settled claims cannot be reopened (User Story 1; gradera:2246c819-8075-4173-a11d-4187dc882147, approved)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every acceptance scenario of User Story 1 passes in the repository's own test suite.

## Assumptions

- What constitutes valid 'new evidence' upon reopening, and how should it be submitted or recorded in the claims portal (e.g., mandatory document upload, free-text justification, or comment)?
- Decided: Acknowledged: Test behavior from code 1
- Decided: Acknowledged: Test behavior from code 1
- Decided: Acknowledged: Curated statuses
- Decided: What constitutes valid 'new evidence' upon reopening, and how should it be submitted or recorded in the claims portal (e.g., mandatory document upload, free-text justification, or comment)? Any document the handler attaches to the claim through the documents panel, as today. No new document type; the reopen reason says what arrived
- Scope is the backlog epic as the engine holds it; a story or criterion rejected or superseded there is not in this specification.

## Estate Context *(Gradera)*

### Constraints that bind this feature

- Conflict: CH-1 allows reopening a Rejected claim into UnderReview, whereas rul_b31a defines the permitted actions for each status in ClaimFlow and specifies no allowed actions for Rejected claims. (acknowledged)
- Conflict: AC-1.1 specifies offering a reopen action on a claim in Rejected status, whereas rul_b31a defines the permitted actions by status and does not permit any actions on Rejected claims. (acknowledged)
- Conflict: AC-1.3 transitions a claim from Rejected status back to UnderReview status, which contradicts rul_8b9d establishing that Rejected is a terminal status. (acknowledged)

### Estate facts to confirm

- None: every item and criterion here is approved or validated.
