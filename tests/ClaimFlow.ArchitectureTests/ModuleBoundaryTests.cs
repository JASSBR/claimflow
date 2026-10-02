using System.Reflection;
using ClaimFlow.Claims;
using ClaimFlow.Claims.Domain;
using ClaimFlow.SharedKernel;
using NetArchTest.Rules;

namespace ClaimFlow.ArchitectureTests;

/// <summary>
/// The architecture is enforced, not just drawn: these rules fail the build the day someone takes a shortcut.
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly string[] InfrastructureNamespaces =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Npgsql",
        "ClaimFlow.BuildingBlocks",
    ];

    private static readonly Assembly SharedKernel = typeof(Result).Assembly;
    private static readonly Assembly ClaimsDomain = typeof(Claim).Assembly;
    private static readonly Assembly ClaimsModuleAssembly = typeof(ClaimsModule).Assembly;
    private static readonly Assembly ApiHost = Assembly.Load("ClaimFlow.Api");
    private static readonly Assembly DocumentsDomain = Assembly.Load("ClaimFlow.Documents.Domain");
    private static readonly Assembly DocumentsModuleAssembly = Assembly.Load("ClaimFlow.Documents");

    /// <summary>Everything in the Claims module except its published contract.</summary>
    private static readonly string[] ClaimsInternals =
    [
        "ClaimFlow.Claims.Domain",
        "ClaimFlow.Claims.Features",
        "ClaimFlow.Claims.Persistence",
        "ClaimFlow.Claims.Realtime",
        "ClaimFlow.Claims.Security",
    ];

    [Fact]
    public void SharedKernel_HasNoInfrastructureDependency() =>
        AssertNoViolation(Types.InAssembly(SharedKernel).ShouldNot().HaveDependencyOnAny(InfrastructureNamespaces).GetResult());

    [Fact]
    public void ClaimsDomain_HasNoInfrastructureDependency() =>
        AssertNoViolation(Types.InAssembly(ClaimsDomain).ShouldNot().HaveDependencyOnAny(InfrastructureNamespaces).GetResult());

    [Fact]
    public void ClaimsModule_ExposesOnlyItsEntryPointAndContracts() =>
        AssertNoViolation(Types.InAssembly(ClaimsModuleAssembly)
            .That().ArePublic()
            .And().DoNotResideInNamespace("ClaimFlow.Claims.Contracts")
            // EF Core generates migrations as public classes; they are not part of the module's API.
            .And().DoNotResideInNamespace("ClaimFlow.Claims.Persistence.Migrations")
            .Should().HaveName(nameof(ClaimsModule))
            .GetResult());

    [Fact]
    public void ApiHost_NeverReachesIntoModuleInternals() =>
        AssertNoViolation(Types.InAssembly(ApiHost)
            .ShouldNot().HaveDependencyOnAny(
                [.. ClaimsInternals,
                "ClaimFlow.Documents.Domain",
                "ClaimFlow.Documents.Analysis",
                "ClaimFlow.Documents.Features",
                "ClaimFlow.Documents.Persistence",
                "Microsoft.EntityFrameworkCore"])
            .GetResult());

    [Fact]
    public void DocumentsDomain_HasNoInfrastructureDependency() =>
        AssertNoViolation(Types.InAssembly(DocumentsDomain).ShouldNot().HaveDependencyOnAny(InfrastructureNamespaces).GetResult());

    [Fact]
    public void DocumentsModule_TalksToClaims_OnlyThroughItsContract() =>
        AssertNoViolation(Types.InAssembly(DocumentsModuleAssembly).ShouldNot().HaveDependencyOnAny(ClaimsInternals).GetResult());

    [Fact]
    public void DocumentsModule_ExposesOnlyItsEntryPointAndContracts() =>
        AssertNoViolation(Types.InAssembly(DocumentsModuleAssembly)
            .That().ArePublic()
            .And().DoNotResideInNamespace("ClaimFlow.Documents.Persistence.Migrations")
            .Should().ResideInNamespaceMatching(@"^ClaimFlow\.Documents$")
            .GetResult());

    [Fact]
    public void DomainAggregates_AreSealed() =>
        AssertNoViolation(Types.InAssembly(ClaimsDomain)
            .That().Inherit(typeof(AggregateRoot<>))
            .Should().BeSealed()
            .GetResult());

    private static void AssertNoViolation(NetArchTest.Rules.TestResult result) =>
        result.IsSuccessful.ShouldBeTrue($"Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
