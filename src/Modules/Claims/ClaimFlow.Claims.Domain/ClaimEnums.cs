namespace ClaimFlow.Claims.Domain;

public enum ClaimType
{
    Auto,
    Home,
    Liability,
}

public enum ClaimStatus
{
    Declared,
    UnderReview,
    InformationRequested,
    Approved,
    Rejected,
    Settled,
}

public enum ClaimAction
{
    StartReview,
    RequestInformation,
    ResumeReview,
    Approve,
    Reject,
    Settle,
}
