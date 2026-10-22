# Feature Specification: Reopen Rejected Claims with New Evidence

**Feature Branch**: `20261022-040010-reopen-rejected-claims-evidence`

**Created**: 2026-10-22

**Status**: Draft

**Input**: Backlog epic "Reopen Rejected Claims with New Evidence" (gradera:4fd985f5-932a-4a1c-81c0-4d1f2dadd6ec, approved), drafted by the Gradera Context Engine from its stories, acceptance criteria and the estate of `git:claimflow`

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reopen a Rejected claim within 30 days (Priority: P1)

As a claims handler, I want to reopen a Rejected claim into UnderReview within 30 days of its rejection when I attach a document and provide a reason, so that the claim can be re-evaluated based on the new evidence. (gradera:3c915558-cda4-4dc5-97d3-d4a0c4bf981f, approved)

**Why this priority**: Ordered as the backlog ranks it.

**Independent Test**: Can be fully tested by the acceptance scenarios below, each of which cites the estate it was drawn from.

**Acceptance Scenarios**:

1. *Successfully reopen a Rejected claim within the 30-day window*: **Given** a claim in the claims portal with a status of Rejected where the rejection timestamp in the history is 30 days or less in the past, **When** the claims handler attaches a document through the documents panel, provides a reopen reason, and submits the reopen action, **Then** the claim status transitions to UnderReview (gradera:73f51895-eb9a-429c-8dbc-079b30084dc5, approved)
2. *Settled claims cannot be reopened*: **Given** a claim in the claims portal with a status of Settled, **When** the claims handler views the claim, **Then** the reopen option is not available (gradera:ecd47946-8c89-4dd4-8141-83c2a6891408, approved)

---

### Edge Cases

- [NEEDS CLARIFICATION: the backlog records no edge case; run `/speckit.clarify`]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST meet this criterion: successfully reopen a Rejected claim within the 30-day window (User Story 1; gradera:73f51895-eb9a-429c-8dbc-079b30084dc5, approved)
- **FR-002**: System MUST meet this criterion: settled claims cannot be reopened (User Story 1; gradera:ecd47946-8c89-4dd4-8141-83c2a6891408, approved)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every acceptance scenario of User Story 1 passes in the repository's own test suite.

## Assumptions

- Decided: Acknowledged: Curated statuses
- Decided: Acknowledged: Test behavior from code 1
- Decided: Acknowledged: Test behavior from code 1
- Scope is the backlog epic as the engine holds it; a story or criterion rejected or superseded there is not in this specification.

## Estate Context *(Gradera)*

### Constraints that bind this feature

- Conflict: CH-1 allows a claim handler to reopen a Rejected claim back into UnderReview, whereas rul_8b9d specifies that Rejected is a terminal status in the adjudication engine. (acknowledged)
- Conflict: CH-1 permits reopening a Rejected claim into UnderReview, whereas rul_b31a defines the allowed actions per status and does not permit any actions from Rejected. (acknowledged)
- Conflict: AC-1.1 specifies that a claim with status Rejected can perform a reopen action transitioning it to UnderReview, whereas rul_b31a defines the allowed actions governed by claim status and permits no actions for a claim with status Rejected. (acknowledged)

### Estate facts to confirm

- None: every item and criterion here is approved or validated.
