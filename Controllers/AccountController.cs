using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using prgmlab3.Models;

namespace prgmlab3.Controllers
{
    public class AccountController : Controller
    {
        // ---------------- LOGIN ----------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["Error"] = "E-posta ve şifre zorunludur.";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var user = UserModel.Login(email, password);
            if (user == null)
            {
                ViewData["Error"] = "Geçersiz e-posta veya şifre.";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            // Cookie içine yazılacak claim'ler
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Mail),
                new Claim(ClaimTypes.Role, user.Role == UserModel.RoleAdmin ? "Admin" : "Customer"),
                new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                "CookieAuth",
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // ADMIN ise admin paneline
            if (user.Role == UserModel.RoleAdmin)
            {
                return RedirectToAction("Index", "Admin");
            }

            // Normal kullanıcı: varsa geçerli returnUrl'e, yoksa Booking/Index'e
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Booking");
        }

        // ---------------- REGISTER ----------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(
            string tcNo,
            string firstName,
            string lastName,
            DateTime? birthDate,
            string email,
            string phone,
            string password)
        {
            // Boş alan kontrolü
            if (string.IsNullOrWhiteSpace(tcNo) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password) ||
                !birthDate.HasValue)
            {
                ViewData["Error"] = "Tüm alanları doldurun.";
                return View();
            }

            // TC kimlik kontrolü 
            if (tcNo.Length != 11)
            {
                ViewData["Error"] = "TC Kimlik Numarası 11 haneli olmalıdır.";
                return View();
            }

            // 18 yaş kontrolü
            var today = DateTime.Today;
            var age = today.Year - birthDate.Value.Year;
            if (birthDate.Value.Date > today.AddYears(-age))
            {
                age--;
            }

            if (age < 18)
            {
                ViewData["Error"] = "Sisteme kayıt olabilmek için en az 18 yaşında olmalısınız.";
                return View();
            }

            bool success = UserModel.Register(
                tcNo,
                firstName,
                lastName,
                birthDate.Value,
                email,
                phone,
                password);

            if (success)
            {
                TempData["Success"] = "Kayıt başarılı. Lütfen giriş yapın.";
                return RedirectToAction("Login");
            }

            ViewData["Error"] = "Kayıt sırasında bir hata oluştu veya bu e-posta zaten kayıtlı.";
            return View();
        }

        // ---------------- LOGOUT ----------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // ---------------- PROFILE ----------------
        [Authorize]
        public IActionResult Profile()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login");
            }

            var user = UserModel.GetById(userId);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        // ---------------- ACCESS DENIED ----------------

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}