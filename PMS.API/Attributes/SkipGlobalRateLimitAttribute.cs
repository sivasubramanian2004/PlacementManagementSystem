namespace PMS.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class SkipGlobalRateLimitAttribute : Attribute
    {
    }
}
