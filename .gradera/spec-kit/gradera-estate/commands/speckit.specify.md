## Estate grounding (Gradera)

The estate knowledge of `claimflow` is held by the Gradera Context Engine and reached through the `gradera` MCP server this repository's kit configures. Read it before you write, and keep to it:

- Knowledge in state `approved` or `validated` is a constraint. Write nothing that contradicts it. When the request does, say which object it contradicts and stop for a decision.
- Knowledge in state `inferred` is the estate's best reading, not a decision. Use it as a question to confirm, never as a fact.
- Cite every estate fact you use with its object id and state, as `(gradera:<object id>, approved)`.
- When the `gradera` server cannot be reached, say so where the estate section belongs and carry on. Never write an estate fact the server did not return.

### Before you write the specification

1. Call `gradera/search_context` with the feature description, and read each hit you rely on with `gradera/get_context_object`.
2. Name the domain's concepts, actors and systems the way the estate names them.
3. Fill the specification's "Estate Context" section: the approved and validated rules that bind this feature, and every inferred fact it depends on as a `[NEEDS CLARIFICATION: ...]` item.
