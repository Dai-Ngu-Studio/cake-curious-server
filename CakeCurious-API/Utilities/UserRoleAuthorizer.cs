using Repository.Constants.Roles;
using Repository.Interfaces;

namespace CakeCurious_API.Utilities
{
    public static class UserRoleAuthorizer
    {
        public static async Task<bool> AuthorizeUser(RoleEnum[] roles, string uid, IUserRepository userRepository)
        {
            var user = await userRepository.GetDetached(uid);
            if (user != null)
            {
                foreach (var role in roles)
                {
                    var has = user.HasRoles!.Any(x => x.RoleId == (int)role);
                    if (has) return true;
                }
            }
            return false;
        }
    }
}
