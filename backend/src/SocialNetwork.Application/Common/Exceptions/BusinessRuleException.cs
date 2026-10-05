namespace SocialNetwork.Application.Common.Exceptions;

public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string message)
        : base("BUSINESS_RULE_VIOLATION", message) { }
}
