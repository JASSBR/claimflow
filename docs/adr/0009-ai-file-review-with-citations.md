# 0009 — AI file review: long context with citations, no vector database

- **Status:** Accepted · 2026-10-02

## Context

A claim file holds a handful of documents: police report, quotes, invoices, photos. A handler must check that they
agree with the declaration (dates, places, circumstances, amounts). Missing an inconsistency costs money; inventing
one costs a customer. Insurance is a regulated activity and the EU AI Act expects human oversight of such systems.

The reflex answer is "RAG": chunk, embed, store vectors, retrieve. Here it would add an embedding provider, a vector
index and a retrieval step whose recall must be tuned — to search a corpus that fits in a single prompt many times over.

## Decision

- **No retrieval, no vector store.** The whole file (≤ 12 documents, ≤ 24 MB per request) is sent in one call to
  Claude (`claude-opus-5-5`, effort `medium`) through the official Anthropic .NET SDK. Retrieval would only
  add a way to miss the page that matters.
- **PDFs are sent natively with citations enabled.** The answer comes back as text blocks carrying page-level
  citations; `AnalysisComposer` (a pure, unit-tested function) numbers them like footnotes (`[[n]]`). The UI shows
  each citation with its quoted passage and a link to the page of the source document. A claim without a source
  cannot hide in a fluent paragraph.
- **Advisory only.** The assistant never changes a claim: it has no tool, no write access, and its output is shown next
  to — not instead of — the workflow buttons. The prompt states that the decision belongs to the handler.
- **Documents are untrusted input.** The system prompt frames them as evidence, never instructions, and asks the model
  to report any embedded instruction as an anomaly (prompt-injection defence).
- **Stored, not regenerated.** Each review is persisted with who asked, which model, how many documents and the token
  usage: it is part of the audit trail and is not paid for twice.
- **Cost guardrails.** Per-user rate limit (6 reviews/hour), size budget per request, and the feature is off unless an
  API key is provided (`Ai:ApiKey` / `ANTHROPIC_API_KEY`); the UI then explains how to enable it. If a safety
  classifier declines, the server-side fallback (`fallbacks: "default"`) re-serves the request in the same call.
- **Testability.** `IClaimAnalyst` is the seam: integration tests run the whole pipeline (upload → blob → analysis →
  storage → API) against a deterministic fake; CI never calls a paid API.

## Consequences

- ✅ Every statement in a review is verifiable in one click.
- ✅ No embedding pipeline to operate, re-index or evaluate.
- ⚠️ Cost scales with file size (a typical 5-document file is a few cents). Mitigated by the guardrails above.
- 🔁 Revisit if files grow beyond the context window (e.g. medical claims with hundreds of pages): then retrieval
  over a pgvector index in the existing PostgreSQL becomes worth its complexity.
