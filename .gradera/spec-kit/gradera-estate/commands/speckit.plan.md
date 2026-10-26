## Estate grounding (Gradera)

The estate knowledge of `claimflow` is held by the Gradera Context Engine and reached through the `gradera` MCP server this repository's kit configures. Read it before you write, and keep to it:

- Knowledge in state `approved` or `validated` is a constraint. Write nothing that contradicts it. When the request does, say which object it contradicts and stop for a decision.
- Knowledge in state `inferred` is the estate's best reading, not a decision. Use it as a question to confirm, never as a fact.
- Cite every estate fact you use with its object id and state, as `(gradera:<object id>, approved)`.
- When the `gradera` server cannot be reached, say so where the estate section belongs and carry on. Never write an estate fact the server did not return.

### Before you write the plan

1. For each component, file or symbol the plan changes, call `gradera/get_facts_for_symbol` and `gradera/get_impact` with it as the anchor.
2. Fill the plan's "Estate Impact" section: what the change reaches and how strongly, the rules that apply, and every unknown the engine reported.
3. Prefer the patterns the kit's skills (`.github/skills/`) describe over new ones, and name the skill each design choice follows.
