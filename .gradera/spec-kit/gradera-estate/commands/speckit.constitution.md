## Estate grounding (Gradera)

The estate knowledge of `claimflow` is held by the Gradera Context Engine and reached through the `gradera` MCP server this repository's kit configures. Read it before you write, and keep to it:

- Knowledge in state `approved` or `validated` is a constraint. Write nothing that contradicts it. When the request does, say which object it contradicts and stop for a decision.
- Knowledge in state `inferred` is the estate's best reading, not a decision. Use it as a question to confirm, never as a fact.
- Cite every estate fact you use with its object id and state, as `(gradera:<object id>, approved)`.
- When the `gradera` server cannot be reached, say so where the estate section belongs and carry on. Never write an estate fact the server did not return.

### Before you change the constitution

1. Read the repository's rules: `.github/copilot-instructions.md` and the scoped instructions beside it hold the repository's rules.
2. Call `gradera/search_context` for the estate's standing rules (security, data handling, testing, naming, dependencies), keeping the `approved` and `validated` ones.
3. Propose principles from what comes back, each citing the objects it rests on. The constitution is the team's: add or refine principles, never remove one the team wrote, and leave the decision on every proposal to them.
