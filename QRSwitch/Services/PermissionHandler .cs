using Microsoft.AspNetCore.Authorization;


public class PermissionAttribute : AuthorizeAttribute
{
    public string PermissionName { get; }
    public PermissionAttribute(string permissionName)
    {
        PermissionName = permissionName;
        Policy = "Permission"; // نفس policy اللي فوق
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    // ممكن تضيف أي properties لو احتجت
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            // خذ اسم الـ permission المطلوب من Endpoint attribute
            var endpoint = httpContext.GetEndpoint();
            var requiredPermission = endpoint?.Metadata
                .GetMetadata<PermissionAttribute>()?.PermissionName;

            if (!string.IsNullOrEmpty(requiredPermission))
            {
                if (context.User.HasClaim(c => c.Type == "Permission" && c.Value == requiredPermission))
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}
