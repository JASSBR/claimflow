---
applyTo: '**/*.py'
---

# Rules for .py files

Each rule cites the code it rests on. A rule marked not yet reviewed is the estate's best reading, not a decision.

- [BusinessRule] At least one Cobertura report matching pattern '**/TestResults/**/*.cobertura.xml' must exist; otherwise the gate aborts CI execution with the message 'No Cobertura report found. Run: dotnet test --coverage --coverage-output-format cobertura'. [eng/coverage-gate.py:L13-L15]
- [BusinessRule] Coverage metrics exclude non-production code: packages containing 'Tests' or 'AppHost' are skipped, and files with paths containing '/Migrations/', '/obj/', or '/tests/' are skipped. [eng/coverage-gate.py:L20-L25]
- [BusinessRule] Production code line coverage must meet or exceed a threshold of 80.0% by default, or the custom percentage value provided as the first command-line argument. [eng/coverage-gate.py:L9-L9]
- [Assumption] The total number of production code lines across all included packages in the reports is assumed to be strictly positive when computing the total coverage percentage. [eng/coverage-gate.py:L39-L41]
- [InformalDecision] When combining multiple Cobertura XML reports across test projects, line hit counts for the same package and file are merged by retaining the maximum hit count across reports for each line number. [eng/coverage-gate.py:L17-L29]
