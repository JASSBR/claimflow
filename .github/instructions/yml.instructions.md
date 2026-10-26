---
applyTo: '**/*.yml'
---

# Rules for .yml files

Each rule cites the code it rests on. A rule marked not yet reviewed is the estate's best reading, not a decision.

- [InformalDecision] In the nuget ecosystem, the 'testing' group aggregates updates for packages matching patterns 'xunit*', 'Testcontainers*', 'Microsoft.Testing.*', 'Shouldly', and 'NetArchTest*'. [.github/dependabot.yml:L9-L9]
- [InformalDecision] Semver major updates for 'typescript' in the npm ecosystem are ignored by Dependabot, ensuring TypeScript major version updates align with matching Angular major version releases. [.github/dependabot.yml:L12-L14]
- [InformalDecision] In the npm ecosystem (/web directory), the 'angular' group aggregates package updates matching patterns '@angular/*', '@angular-devkit/*', 'angular-eslint', and 'typescript'. [.github/dependabot.yml:L15-L15]
- [InformalDecision] In the nuget ecosystem, the 'aspire' group aggregates updates for packages matching the pattern 'Aspire.*'. [.github/dependabot.yml:L7-L7]
- [InformalDecision] In the nuget ecosystem, the 'efcore' group aggregates updates for packages matching patterns 'Microsoft.EntityFrameworkCore*' and 'Npgsql*'. [.github/dependabot.yml:L8-L8]
- [InformalDecision] Dependency check schedules are set to weekly for 'nuget' and 'npm' ecosystems, while 'github-actions' is scheduled to run monthly. [.github/dependabot.yml:L16-L18]
