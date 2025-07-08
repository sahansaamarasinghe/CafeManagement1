namespace WebApplication2.Helpers
{
    public static class RoleConstants
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";

        public static readonly HashSet<string> All =
            new() { Admin, Customer };
    }
}
