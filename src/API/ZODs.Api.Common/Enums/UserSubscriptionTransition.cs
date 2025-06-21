namespace ZODs.Api.Common.Enums;

public enum UserSubscriptionTransition
{
    ExpiredToActive,
    ToPastDue,
    ToUnpaid,
    ToCancelled,
    ToExpired,
}