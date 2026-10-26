## Estate grounding (Gradera)

The estate knowledge of `claimflow` is held by the Gradera Context Engine and reached through the `gradera` MCP server this repository's kit configures. Read it before you write, and keep to it:

- Knowledge in state `approved` or `validated` is a constraint. Write nothing that contradicts it. When the request does, say which object it contradicts and stop for a decision.
- Knowledge in state `inferred` is the estate's best reading, not a decision. Use it as a question to confirm, never as a fact.
- Cite every estate fact you use with its object id and state, as `(gradera:<object id>, approved)`.
- When the `gradera` server cannot be reached, say so where the estate section belongs and carry on. Never write an estate fact the server did not return.

### Before you change a file

1. Call `gradera/get_impact` for the file, and follow the skill its task names.
2. Keep to the rules: `.github/copilot-instructions.md` and the scoped instructions beside it hold the repository's rules. The `claimflow` agent (`.github/agents/claimflow.agent.md`) works this way.
3. When the code you find contradicts an approved rule, stop and report it with the object id rather than changing either.
