using System.Security.Claims;
using HospitalAppointment.Application.DTOs.Auth;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Infrastructure.Identity;
using HospitalAppointment.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Web.Controllers;

// Hesap yönetimi kontrolcüsü — giriş, kayıt, çıkış
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    // ─── Giriş ───────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            // Identity SignInManager ile giriş — cookie auth kullanır
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Kullanıcı giriş yaptı: {Email}", model.Email);
                TempData["Success"] = "Başarıyla giriş yaptınız.";

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş hatası: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Giriş sırasında bir hata oluştu.");
        }

        return View(model);
    }

    // ─── Kayıt ───────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            // Hasta kaydı servisi üzerinden
            var dto = new RegisterDto(
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                model.PhoneNumber,
                model.DateOfBirth,
                model.Gender,
                model.BloodType);

            await _authService.RegisterPatientAsync(dto);

            // Kayıt sonrası otomatik giriş
            await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            _logger.LogInformation("Yeni hasta kaydı: {Email}", model.Email);
            TempData["Success"] = "Hesabınız başarıyla oluşturuldu. Hoş geldiniz!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt hatası: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(model);
    }

    // ─── Çıkış ───────────────────────────────────────────────────────────────

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Kullanıcı çıkış yaptı.");
        TempData["Success"] = "Başarıyla çıkış yaptınız.";
        return RedirectToAction("Index", "Home");
    }

    // ─── Erişim Reddedildi ────────────────────────────────────────────────────

    public IActionResult AccessDenied()
    {
        return View();
    }
}
