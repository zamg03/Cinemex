using Cinemex.Models;
using Cinemex.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Cinemex.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                string primerNombre = user.Nombres.Trim().Split(' ')[0];

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, primerNombre),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario model)
        {
            if (_context.Usuarios.Any(u => u.Email == model.Email))
            {
                ViewBag.Error = "El correo ya está registrado.";
                return View(model);
            }

            // VALIDACIÓN DETALLADA DE CONTRASEÑA EN EL SERVIDOR
            List<string> erroresPwd = new List<string>();

            if (model.Password.Length < 6) erroresPwd.Add("Tiene que contener mínimo 6 caracteres.");
            if (model.Password.Length > 20) erroresPwd.Add("Límite máximo de 20 caracteres.");
            if (model.Password.Contains(" ")) erroresPwd.Add("No debe contener espacios.");
            if (!Regex.IsMatch(model.Password, @"[A-Z]")) erroresPwd.Add("Falta mayúscula.");
            if (!Regex.IsMatch(model.Password, @"[a-z]")) erroresPwd.Add("Tiene que contener al menos una minúscula.");
            if (!Regex.IsMatch(model.Password, @"\d")) erroresPwd.Add("Falta número.");
            if (!Regex.IsMatch(model.Password, @"[^a-zA-Z0-9\s]")) erroresPwd.Add("Tiene que contener al menos un símbolo.");

            if (erroresPwd.Any())
            {
                // Unimos todos los errores detectados para mandarlos a la pantalla
                ViewBag.Error = string.Join("<br>", erroresPwd);
                return View(model);
            }

            if (string.IsNullOrEmpty(model.ApellidoMaterno))
            {
                model.ApellidoMaterno = "";
            }

            _context.Usuarios.Add(model);
            _context.SaveChanges();

            string primerNombre = model.Nombres.Trim().Split(' ')[0];
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, primerNombre),
                new Claim(ClaimTypes.Email, model.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}