namespace BN.PROJECT.IdentityService
{
    internal class MinimumAgeRequirement : IAuthorizationRequirement
    {
        public int Age { get; private set; }

        public MinimumAgeRequirement(int age) { Age = age; }
    }
}