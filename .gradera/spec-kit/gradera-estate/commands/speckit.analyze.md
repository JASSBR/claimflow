## Estate grounding (Gradera)

The estate knowledge of `claimflow` is held by the Gradera Context Engine and reached through the `gradera` MCP server this repository's kit configures. Read it before you write, and keep to it:

- Knowledge in state `approved` or `validated` is a constraint. Write nothing that contradicts it. When the request does, say which object it contradicts and stop for a decision.
- Knowledge in state `inferred` is the estate's best reading, not a decision. Use it as a question to confirm, never as a fact.
- Cite every estate fact you use with its object id and state, as `(gradera:<object id>, approved)`.
- When the `gradera` server cannot be reached, say so where the estate section belongs and carry on. Never write an estate fact the server did not return.

### Also check against the estate

Report each of these as a finding of its own: a requirement or task that contradicts an approved or validated estate rule, a component the change reaches that the plan's "Estate Impact" leaves out, and an inferred estate fact used as if it were decided.
