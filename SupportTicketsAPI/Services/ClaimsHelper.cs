using System.Security.Claims;

namespace SupportTicketsAPI.Services
{
    public static class ClaimsHelper
    {
        public static int? GetUserId(ClaimsPrincipal user)
        {
            var idStr =
                user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue("nameid") ??
                user.FindFirstValue("sub");

            if (int.TryParse(idStr, out var id))
                return id;

            return null;
        }


    }
}
