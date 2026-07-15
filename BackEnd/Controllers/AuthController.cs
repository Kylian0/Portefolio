using BackEnd.Dtos;
using BackEnd.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(UserManager<AdminUser> userManager, SignInManager<AdminUser> signInManager) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(AdminLoginDto request)
    {
        var user = await userManager.FindByNameAsync(request.Username.Trim());
        if (user is null) return UnauthorizedProblem();
        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut) return Problem(statusCode: 423, title: "Account locked", detail: "Trop de tentatives. Réessayez dans quelques minutes.");
        if (!result.Succeeded) return UnauthorizedProblem();
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        return SignIn(principal, new AuthenticationProperties { IsPersistent = true }, IdentityConstants.BearerScheme);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { userName = User.Identity?.Name });

    private ObjectResult UnauthorizedProblem() => Problem(statusCode: 401, title: "Invalid credentials", detail: "L'identifiant ou le mot de passe est incorrect.");
}
