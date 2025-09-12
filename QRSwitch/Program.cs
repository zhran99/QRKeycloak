using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using QRSwitch.ErrorHandling;
using QRSwitch.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddHttpClient<KeycloakUserService>();
builder.Services.AddHttpClient<KeycloakRoleService>();
builder.Services.AddHttpClient<KeycloakAuthorizationService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakUrl = builder.Configuration["Keycloak:Url"];
        var realm = builder.Configuration["Keycloak:Realm"];
        options.Authority = $"http://{keycloakUrl}/realms/{realm}";
        options.Audience = builder.Configuration["Keycloak:ClientId"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity == null)
                    return Task.CompletedTask;

                // Extract realm_access.roles
                var realmAccessClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
                if (!string.IsNullOrEmpty(realmAccessClaim))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(realmAccessClaim);
                        if (doc.RootElement.TryGetProperty("roles", out var rolesArray))
                        {
                            foreach (var role in rolesArray.EnumerateArray())
                            {
                                var roleValue = role.GetString();
                                if (!string.IsNullOrEmpty(roleValue))
                                {
                                    // مهم جدًا: استخدام ClaimTypes.Role
                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                    Console.WriteLine($"Added Role: {roleValue}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing roles: {ex.Message}");
                    }
                }

                //  Extract Permissions 
                var authorization = context.Principal.FindFirst("authorization")?.Value;
                if (!string.IsNullOrEmpty(authorization))
                {
                    var json = JObject.Parse(authorization);
                    var permissions = json["permissions"];
                    if (permissions != null)
                    {
                        foreach (var perm in permissions)
                        {
                            var scopes = perm["scopes"];
                            if (scopes != null)
                            {
                                foreach (var scope in scopes)
                                {
                                    identity.AddClaim(new Claim("Permission", scope.ToString()));
                                }
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };


    });

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Permission", policy =>
        policy.Requirements.Add(new PermissionRequirement()));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT Bearer token **_only_**",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
