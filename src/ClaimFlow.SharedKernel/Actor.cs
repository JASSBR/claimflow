namespace ClaimFlow.SharedKernel;

/// <summary>Who performed an action. Kept in the domain so audit trails and segregation-of-duties rules are business rules, not HTTP plumbing.</summary>
public sealed record Actor(string Id, string Name)
{
    public static Actor System { get; } = new("system", "ClaimFlow");
}
