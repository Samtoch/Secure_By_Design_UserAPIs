namespace Securebydesign.Api.Extensions
{
    /// <summary>Policy names. Endpoints ask for a permission, not a role.</summary>
    public static class Policies
    {
        public const string ViewAllUsers = "ViewAllUsers";   // list and search users
        public const string ViewUser = "ViewUser";           // self, Support or Admin
        public const string UpdateUser = "UpdateUser";       // self or Admin
        public const string DeleteUsers = "DeleteUsers";     // Admin only
        public const string ManageRoles = "ManageRoles";     // Admin only
    }
}
