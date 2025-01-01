using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TechAssessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecureController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost("admin-endpoint")]
        public IActionResult AdminEndpoint()
        {
            return Ok("This is an Admin-only endpoint.");
        }

        [Authorize(Roles = "User")]
        [HttpGet("user-endpoint")]
        public IActionResult UserEndpoint()
        {
            return Ok("This is a User-only endpoint.");
        }

    }
}
