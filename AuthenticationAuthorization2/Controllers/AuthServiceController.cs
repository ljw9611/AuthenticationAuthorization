using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AuthenticationAuthorization2.Program;

namespace AuthenticationAuthorization2.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthServiceController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IEnumerable<ClaimDto> Get() =>
            HttpContext.User.Claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value });
    }
}
