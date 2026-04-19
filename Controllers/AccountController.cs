using Microsoft.AspNetCore.Mvc;
using Cinemex.Models;
using Cinemex.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Email) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Home");
            }

            // Si llega aquí, es porque falló el login
            ViewBag.ModalError = "Usuario y/o contraseña inválidos.";
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password)
        {
            // Verificamos si el correo ya existe en la base de datos
            if (_context.Usuarios.Any(u => u.Email == email))
            {
                ViewBag.ModalError = "El correo electrónico ya se encuentra registrado.";
                ViewBag.ShowRegister = true; // Mantiene la vista en "Registro"
                return View("Login");
            }

            // Si no existe, lo creamos
            _context.Usuarios.Add(new Usuario { Email = email, Password = password });
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}