# Research: Reopen Rejected Claims with New Evidence

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | `/specs/20261022-040010-reopen-rejected-claims-evidence/`

Each entry is a decision, the reason for it and what else was considered, in the shape `/speckit.plan` writes in Phase 0. An entry the estate cannot decide stays NEEDS CLARIFICATION; nothing here is guessed.

## Decided by the estate

### Successfully reopen a Rejected claim within the 30-day window

- **Decision**: build to this criterion as written (gradera:73f51895-eb9a-429c-8dbc-079b30084dc5, approved)
- **Rationale**: it is approved knowledge of the estate, cited by 1 source(s), for story "Reopen a Rejected claim within 30 days"
- **Alternatives considered**: none; changing it is a change to governed knowledge, made in the engine and not in this specification

### Settled claims cannot be reopened

- **Decision**: build to this criterion as written (gradera:ecd47946-8c89-4dd4-8141-83c2a6891408, approved)
- **Rationale**: it is approved knowledge of the estate, cited by 1 source(s), for story "Reopen a Rejected claim within 30 days"
- **Alternatives considered**: none; changing it is a change to governed knowledge, made in the engine and not in this specification

## To decide before implementation

### Language, version and dependencies

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: read from the estate by `/speckit.plan`
- **Alternatives considered**: to be recorded when it is decided

### Storage and testing approach

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: read from the estate by `/speckit.plan`
- **Alternatives considered**: to be recorded when it is decided

### no impacted code is recorded for the epic, so the plan cannot name the modules it changes

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: no impact
- **Alternatives considered**: to be recorded when it is decided

### Conflict: CH-1 allows a claim handler to reopen a Rejected claim back into UnderReview, whereas rul_8b9d specifies that Rejected is a terminal status in the adjudication engine. (acknowledged)

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: a rule the request collides with
- **Alternatives considered**: to be recorded when it is decided

### Conflict: CH-1 permits reopening a Rejected claim into UnderReview, whereas rul_b31a defines the allowed actions per status and does not permit any actions from Rejected. (acknowledged)

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: a rule the request collides with
- **Alternatives considered**: to be recorded when it is decided

### Conflict: AC-1.1 specifies that a claim with status Rejected can perform a reopen action transitioning it to UnderReview, whereas rul_b31a defines the allowed actions governed by claim status and permits no actions for a claim with status Rejected. (acknowledged)

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: a rule the request collides with
- **Alternatives considered**: to be recorded when it is decided
