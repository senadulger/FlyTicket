using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using prgmlab3.Models;
using System.Security.Claims;

namespace prgmlab3.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var user = UserModel.Login(email, password);
            if (user == null)
            {
                ViewData["Error"] = "Geçersiz e-posta veya şifre.";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Mail),
                new Claim(ClaimTypes.Role, user.Role == 1 ? "Admin" : "Customer"),
                new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role == 1)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Booking");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["Error"] = "Tüm alanları doldurun.";
                return View();
            }

            if (UserModel.Register(username, email, password))
            {
                TempData["Success"] = "Kayıt başarılı. Lütfen giriş yapın.";
                return RedirectToAction("Login");
            }
            else
            {
                ViewData["Error"] = "Bu e-posta adresi zaten kayıtlı.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
