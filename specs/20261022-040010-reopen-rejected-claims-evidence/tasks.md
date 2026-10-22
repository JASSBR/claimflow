---

description: "Task list for Reopen Rejected Claims with New Evidence, drafted from the backlog"
---

# Tasks: Reopen Rejected Claims with New Evidence

**Input**: Design documents from `/specs/20261022-040010-reopen-rejected-claims-evidence/`

**Prerequisites**: plan.md (required), spec.md (required for user stories)

**Tests**: Included. Every acceptance scenario in spec.md is a test written first.

**Organization**: Tasks are grouped by user story, each naming the promoted kit skill it follows.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, ...)

## Phase 1: Setup (Shared Infrastructure)

- [ ] T001 Confirm every NEEDS CLARIFICATION in spec.md and plan.md with `/speckit.clarify` and `/speckit.plan`
- [ ] T002 [P] Read the impact of each module the plan names with `get_impact` before changing it

---

## Phase 2: User Story 1 - Reopen a Rejected claim within 30 days (Priority: P1) 🎯 MVP

**Goal**: As a claims handler, I want to reopen a Rejected claim into UnderReview within 30 days of its rejection when I attach a document and provide a reason, so that the claim can be re-evaluated based on the new evidence.

**Independent Test**: the acceptance scenarios of User Story 1 in spec.md pass.

### Tests for User Story 1

- [ ] T003 [P] [US1] Write a failing test for: Successfully reopen a Rejected claim within the 30-day window (gradera:73f51895-eb9a-429c-8dbc-079b30084dc5, approved)
- [ ] T004 [P] [US1] Write a failing test for: Settled claims cannot be reopened (gradera:ecd47946-8c89-4dd4-8141-83c2a6891408, approved)

### Implementation for User Story 1

- [ ] T005 [US1] Implement Reopen a Rejected claim within 30 days, following skill `add-claim-action` (`.github/skills/add-claim-action/SKILL.md`)
- [ ] T006 [US1] Make every test of this story pass and keep to the rules the plan's Estate Impact names

**Checkpoint**: User Story 1 works and is testable on its own

---

## Phase 3: Polish & Cross-Cutting Concerns

- [ ] T007 Run `/speckit.analyze` and resolve every finding against the estate
- [ ] T008 [P] Update the documentation the change reaches

## Dependencies & Execution Order

- Setup (Phase 1) first; each user story phase then stands on its own, in priority order.
- Within a story, tests are written and fail before the implementation.
