using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartCertificateSystem.Utilities;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthorizeRoleAttribute(params string[] allowedRoles) : Attribute, IAuthorizationFilter
{
    private readonly HashSet<string> _allowedRoles = allowedRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var role = context.HttpContext.Session.GetString(SessionKeys.Role);
        if (string.IsNullOrWhiteSpace(role) || !_allowedRoles.Contains(role))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
    }
}
