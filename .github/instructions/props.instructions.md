---
applyTo: '**/*.props'
---

# Rules for .props files

Each rule cites the code it rests on. A rule marked not yet reviewed is the estate's best reading, not a decision.

- [InformalDecision] C# nullable reference type checking is enabled across projects. [Directory.Build.props:L4-L4]
- [InformalDecision] Compiler warnings are treated as errors ('TreatWarningsAsErrors' set to true) so that any warning causes the build to fail. [Directory.Build.props:L7-L8]
- [InformalDecision] Implicit global using directives are enabled across projects. [Directory.Build.props:L5-L5]
- [InformalDecision] Meziantou.Analyzer is included across all projects as a private build/analyzer asset. [Directory.Build.props:L16-L16]
- [InformalDecision] Code style enforcement during build is enabled by setting EnforceCodeStyleInBuild to true. [Directory.Build.props:L10-L10]
- [InformalDecision] NuGet Central Package Management is enabled project-wide by setting ManagePackageVersionsCentrally to true. [Directory.Packages.props:L3-L3]
- [InformalDecision] The target framework across projects governed by Directory.Build.props is configured to net10.0. [Directory.Build.props:L3-L3]
- [InformalDecision] XML documentation file generation during build is disabled by setting GenerateDocumentationFile to false. [Directory.Build.props:L12-L12]
- [InformalDecision] Transitive package version pinning is enabled across the repository by setting CentralPackageTransitivePinningEnabled to true. [Directory.Packages.props:L4-L4]
- [InformalDecision] Deterministic compilation is enabled by setting Deterministic to true. [Directory.Build.props:L11-L11]
- [InformalDecision] Code analysis is set to 'latest-recommended' rules across projects. [Directory.Build.props:L9-L9]
- [InformalDecision] The C# language version is set to 'latest' across projects. [Directory.Build.props:L6-L6]
- [InformalDecision] Marks projects in the tests directory with IsTestProject set to true and IsPackable set to false to prevent test assemblies from being packaged into NuGet artifacts. [tests/Directory.Build.props:L6-L7]
- [InformalDecision] Configures OutputType as Exe for test projects because xUnit v3 test projects operate as self-hosting executables. [tests/Directory.Build.props:L4-L5]
- [InformalDecision] Enables UseMicrosoftTestingPlatformRunner to execute tests using Microsoft.Testing.Platform rather than VSTest. [tests/Directory.Build.props:L8-L9]
