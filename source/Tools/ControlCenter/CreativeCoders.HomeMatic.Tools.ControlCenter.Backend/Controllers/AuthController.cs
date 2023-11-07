using Microsoft.AspNetCore.Mvc;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginModel loginModel)
    {
        return Ok(new LoginResult());
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        return Ok();
    }
}

public class LoginResult
{
    public bool IsSucceeded { get; set; }
    
    public string? Token { get; set; }
}

public class LoginModel
{
    public string? UserName { get; set; }

    public string? Password { get; set; }
}
