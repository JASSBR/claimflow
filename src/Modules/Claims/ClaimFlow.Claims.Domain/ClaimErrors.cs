using ClaimFlow.SharedKernel;

namespace ClaimFlow.Claims.Domain;

public static class ClaimErrors
{
    public static readonly Error PolicyNumberInvalid =
        Error.Validation("claim.policy_number_invalid", "Policy number must match POL-000000.");

    public static readonly Error TypeInvalid =
        Error.Validation("claim.type_invalid", "Unknown claim type.");

    public static readonly Error IncidentDateInFuture =
        Error.Validation("claim.incident_date_in_future", "The incident cannot be declared before it happens.");

    public static readonly Error IncidentTimeBarred =
        Error.Validation("claim.incident_time_barred", "Claims are time-barred two years after the incident (Code des assurances, L114-1).");

    public static readonly Error DescriptionLength =
        Error.Validation("claim.description_length", $"Description must be between {Claim.DescriptionMinLength} and {Claim.DescriptionMaxLength} characters.");

    public static readonly Error ClaimedAmountOutOfRange =
        Error.Validation("claim.claimed_amount_out_of_range", $"Claimed amount must be greater than 0 and at most {Claim.MaxClaimedAmount:N0} EUR.");

    public static readonly Error ReasonRequired =
        Error.Validation("claim.reason_required", "A reason is required for this decision.");

    public static readonly Error ApprovedAmountOutOfRange =
        Error.Validation("claim.approved_amount_out_of_range", "Approved amount must be greater than 0 and cannot exceed the claimed amount.");

    public static readonly Error NotFound =
        Error.NotFound("claim.not_found", "No claim exists with this id.");

    public static readonly Error ConcurrentUpdate =
        Error.Conflict("claim.concurrent_update", "The claim was modified by someone else. Reload it and try again.");

    public static Error TransitionNotAllowed(ClaimAction action, ClaimStatus status) =>
        Error.Conflict("claim.transition_not_allowed", $"Action '{action}' is not allowed while the claim is '{status}'.");
}
