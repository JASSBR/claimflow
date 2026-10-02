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
                "ClaimFlow.Claims.Domain",
                "ClaimFlow.Claims.Features",
                "ClaimFlow.Claims.Persistence",
                "Microsoft.EntityFrameworkCore")
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
