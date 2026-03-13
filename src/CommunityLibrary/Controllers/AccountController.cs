using System.ComponentModel.DataAnnotations;
using CommunityLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLibrary.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl ?? "/";
        return View(new LoginViewModel { ReturnUrl = returnUrl ?? "/" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        returnUrl ??= "/";
        ViewData["ReturnUrl"] = returnUrl;
        model.ReturnUrl = returnUrl;

        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(string.Empty, "Email and Password are required.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in.", model.Email);
            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid login. Use admin@community.local / Admin123!");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return Redirect(returnUrl ?? "/");
    }
}
