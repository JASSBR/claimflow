# Research: Reopen Rejected Claims with New Evidence

**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | `/specs/20261026-071832-reopen-rejected-claims-evidence/`

Each entry is a decision, the reason for it and what else was considered, in the shape `/speckit.plan` writes in Phase 0. An entry the estate cannot decide stays NEEDS CLARIFICATION; nothing here is guessed.

## Decided by the estate

### Settled claims cannot be reopened

- **Decision**: build to this criterion as written (gradera:2246c819-8075-4173-a11d-4187dc882147, approved)
- **Rationale**: it is approved knowledge of the estate, cited by 1 source(s), for story "Reopen a rejected claim within 30 days"
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

### Conflict: CH-1 allows reopening a Rejected claim into UnderReview, whereas rul_b31a defines the permitted actions for each status in ClaimFlow and specifies no allowed actions for Rejected claims. (acknowledged)

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: a rule the request collides with
- **Alternatives considered**: to be recorded when it is decided

### Conflict: AC-1.1 specifies offering a reopen action on a claim in Rejected status, whereas rul_b31a defines the permitted actions by status and does not permit any actions on Rejected claims. (acknowledged)

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: a rule the request collides with
- **Alternatives considered**: to be recorded when it is decided

### Conflict: AC-1.3 transitions a claim from Rejected status back to UnderReview status, which contradicts rul_8b9d establishing that Rejected is a terminal status. (acknowledged)

- **Decision**: NEEDS CLARIFICATION
- **Rationale**: a rule the request collides with
- **Alternatives considered**: to be recorded when it is decided
