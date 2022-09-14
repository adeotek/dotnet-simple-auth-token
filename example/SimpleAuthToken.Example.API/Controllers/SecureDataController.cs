using Microsoft.AspNetCore.Mvc;

namespace SimpleAuthToken.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SecureDataController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("The answer is: 42");
    }
}