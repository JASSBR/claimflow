# Quickstart: Reopen Rejected Claims with New Evidence

**Feature**: [spec.md](./spec.md) | `/specs/20261022-040010-reopen-rejected-claims-evidence/`

Run these after the tasks are done. Each scenario is a criterion of spec.md, so a failed step names the requirement it breaks.

## User Story 1: Reopen a Rejected claim within 30 days

1. *Successfully reopen a Rejected claim within the 30-day window*: **Given** a claim in the claims portal with a status of Rejected where the rejection timestamp in the history is 30 days or less in the past, **When** the claims handler attaches a document through the documents panel, provides a reopen reason, and submits the reopen action, **Then** the claim status transitions to UnderReview (gradera:73f51895-eb9a-429c-8dbc-079b30084dc5, approved)
2. *Settled claims cannot be reopened*: **Given** a claim in the claims portal with a status of Settled, **When** the claims handler views the claim, **Then** the reopen option is not available (gradera:ecd47946-8c89-4dd4-8141-83c2a6891408, approved)
