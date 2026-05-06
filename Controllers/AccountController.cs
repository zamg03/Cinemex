using Cinemex.Models;
using Cinemex.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;

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
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            try
            {
                var user = _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Password == password);
                if (user != null)
                {
                    string primerNombre = user.Nombres.Trim().Split(' ')[0];
                    var claims = new List<Claim> { new Claim(ClaimTypes.Name, primerNombre), new Claim(ClaimTypes.Email, user.Email) };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception) { }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public JsonResult EnviarCodigoConfirmacion(string correo)
        {
            try
            {
                if (_context.Usuarios.Any(u => u.Email == correo))
                {
                    return Json(new { exito = false, error = "Este correo ya se encuentra registrado previamente." });
                }
            }
            catch (Exception dbEx)
            {
                return Json(new { exito = false, error = "Error interno de Base de Datos: " + dbEx.Message });
            }

            try
            {
                Random rnd = new Random();
                string codigo = rnd.Next(100000, 999999).ToString();
                string miCorreo = "danielfzg21@gmail.com";
                string miContrasenaApp = "mgbearathouprdwp";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(miCorreo, miContrasenaApp)
                };

                using (var message = new MailMessage(new MailAddress(miCorreo, "Cinemex Loop"), new MailAddress(correo)))
                {
                    message.Subject = "Código de confirmación - Cinemex Loop";
                    message.IsBodyHtml = true;
                    message.Body = $"<div style='font-family: Arial; padding: 20px; border: 1px solid #ddd; max-width: 500px;'><h2 style='color: #E50914;'>Confirmación de Registro</h2><p>Hola,</p><p>Estás a un paso de ser parte de Cinemex Loop. Tu código de verificación es:</p><h1 style='background-color: #f1f1f1; padding: 10px; text-align: center; letter-spacing: 5px;'>{codigo}</h1><p>Regresa a la página e ingresa este número para continuar.</p></div>";
                    smtp.Send(message);
                }

                return Json(new { exito = true, codigoEnviado = codigo });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, error = "Fallo al enviar el correo: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario model)
        {
            try
            {
                if (_context.Usuarios.Any(u => u.Email == model.Email))
                {
                    ViewBag.Error = "El correo ya está registrado previamente.";
                    return View(model);
                }
            }
            catch (Exception) { }

            List<string> erroresPwd = new List<string>();

            if (model.Password.Length < 8) erroresPwd.Add("Tiene que contener mínimo 8 caracteres.");
            if (model.Password.Length > 20) erroresPwd.Add("Límite máximo de 20 caracteres.");
            if (model.Password.Contains(" ")) erroresPwd.Add("No debe contener espacios.");
            if (!Regex.IsMatch(model.Password, @"[A-Z]")) erroresPwd.Add("Falta mayúscula.");
            if (!Regex.IsMatch(model.Password, @"[a-z]")) erroresPwd.Add("Tiene que contener al menos una minúscula.");
            if (!Regex.IsMatch(model.Password, @"\d")) erroresPwd.Add("Falta número.");
            if (!Regex.IsMatch(model.Password, @"[^a-zA-Z0-9\s]")) erroresPwd.Add("Tiene que contener al menos un símbolo.");

            if (erroresPwd.Any())
            {
                ViewBag.Error = string.Join("<br>", erroresPwd);
                return View(model);
            }

            if (string.IsNullOrEmpty(model.ApellidoMaterno)) model.ApellidoMaterno = "";

            _context.Usuarios.Add(model);
            _context.SaveChanges();

            string primerNombre = model.Nombres.Trim().Split(' ')[0];
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, primerNombre), new Claim(ClaimTypes.Email, model.Email) };
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