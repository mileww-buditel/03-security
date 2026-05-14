namespace Demo.Common;

public static class Constants
{
    public static class CorsPolicies
    {
        public const string Broken = "BrokenCorsPolicy";
        public const string Correct = "CorrectCorsPolicy";
    }

    public static class RateLimiterPolicies
    {
        public const string LoginFixedWindow = "LoginFixedWindow";
    }
}
