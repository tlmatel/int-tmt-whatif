using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth0.AspNetCore.Authentication;

namespace CoreWhatIf.Controllers;

public class AccountController : Controller
{
    [AllowAnonymous]
    public IActionResult Welcome()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard", "WhatIf");

        return View();
    }

    public async Task Login(string returnUrl = "/")
    {
        var authProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .Build();

        await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authProperties);
    }

    [Authorize]
    public async Task Logout()
    {
        var authProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri(Url.Action("Welcome", "Account") ?? "/")
            .Build();

        await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authProperties);
        await HttpContext.SignOutAsync();
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [AllowAnonymous]
    public IActionResult AuthError(string? message)
    {
        ViewBag.ErrorMessage = message ?? "Se produjo un error durante la autenticación.";
        return View();
    }
}
