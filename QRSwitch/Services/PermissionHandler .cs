using Microsoft.AspNetCore.Authorization;


public class PermissionAttribute : AuthorizeAttribute
{
    public string PermissionName { get; }
    public PermissionAttribute(string permissionName)
    {
        PermissionName = permissionName;
        Policy = "Permission"; 
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
   
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
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
