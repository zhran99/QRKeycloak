using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRSwitch.Models;
using QRSwitch.Services;

namespace QRSwitch.Controllers
{
    [ApiController]
    [Route("api/authz")]
    public class AuthorizationController : ControllerBase
    {
        private readonly KeycloakAuthorizationService _authzService;
        private readonly IConfiguration _config;

        public AuthorizationController(KeycloakAuthorizationService authzService, IConfiguration config)
        {
            _authzService = authzService;
            _config = config;
        }

        [HttpPost("scope")]
        public async Task<IActionResult> CreateScope([FromBody] CreateScopeRequest req)
        {
            var realm = _config["Keycloak:Realm"];
            var clientIdFromUi = _config["Keycloak:ClientId"]; // ده الاسم في UI
            var result = await _authzService.CreateScopeAsync(realm, clientIdFromUi, req);
            return result.Success ? Ok(result.RawResponse) : BadRequest(result.ErrorMessage);
        }

        [HttpPost("resource")]
        public async Task<IActionResult> CreateResource([FromBody] CreateResourceRequest req)
        {
            var realm = _config["Keycloak:Realm"];
            var clientId = _config["Keycloak:ClientUuid"];
            var result = await _authzService.CreateResourceAsync(realm, clientId, req);
            return result.Success ? Ok(result.RawResponse) : BadRequest(result.ErrorMessage);
        }

        [HttpPost("policy")]
        public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyRequest req)
        {
            var realm = _config["Keycloak:Realm"];
            var clientId = _config["Keycloak:ClientUuid"];
            var result = await _authzService.CreatePolicyAsync(realm, clientId, req);
            return result.Success ? Ok(result.RawResponse) : BadRequest(result.ErrorMessage);
        }

        [HttpPost("permission")]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest req)
        {
            var realm = _config["Keycloak:Realm"];
            var clientId = _config["Keycloak:ClientUuid"];
            var result = await _authzService.CreatePermissionAsync(realm, clientId, req);
            return result.Success ? Ok(result.RawResponse) : BadRequest(result.ErrorMessage);
        }
    }


}
