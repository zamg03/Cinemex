using Cinemex.Models;
using Cinemex.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace Cinemex.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context) { _context = context; }

        public IActionResult Index() { return View(); }
        public IActionResult Terminos() { return View(); }
        public IActionResult Loop() { return View(); }

        // NUEVA ACCIÓN PARA LA PÁGINA DE IMAX
        public IActionResult Imax() { return View(); }

        public IActionResult Cartelera(bool soloPreventas = false, string buscar = null)
        {
            ViewBag.SoloPreventas = soloPreventas;
            ViewBag.Busqueda = buscar;
            return View();
        }

        public IActionResult Asientos(string pelicula, string poster, string fecha, string sala, string horario = "14:30", string cine = "Mol Concordia")
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            ViewBag.Pelicula = pelicula;
            ViewBag.Poster = poster;
            ViewBag.Fecha = fecha;
            ViewBag.Sala = sala;
            ViewBag.Horario = horario;
            ViewBag.Cine = cine;

            var asientosOcupados = _context.Reservas
                                           .Where(r => r.Horario == horario && r.Cine == cine && r.Pelicula == pelicula && r.Fecha == fecha && r.Sala == sala)
                                           .Select(r => r.Asiento).ToList();

            ViewBag.Ocupados = asientosOcupados;
            return View();
        }

        [HttpPost]
        public IActionResult Pago(string pelicula, string poster, string fecha, string sala, string horario, string asientos, int cantidadAsientos, string totalPagar, string cine)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            ViewBag.Pelicula = pelicula;
            ViewBag.Poster = poster;
            ViewBag.Fecha = fecha;
            ViewBag.Sala = sala;
            ViewBag.Horario = horario;
            ViewBag.Asientos = asientos;
            ViewBag.Cantidad = cantidadAsientos;
            ViewBag.Total = totalPagar;
            ViewBag.Cine = cine;

            return View();
        }

        [HttpPost]
        public IActionResult Confirmacion(string pelicula, string fecha, string sala, string horario, string asientos, int cantidadAsientos, string totalPagar, string cine)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var emailUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            var listaAsientos = asientos.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var asiento in listaAsientos)
            {
                _context.Reservas.Add(new Reserva { UsuarioEmail = emailUsuario, Pelicula = pelicula, Fecha = fecha, Sala = sala, Horario = horario, Asiento = asiento, Cine = cine });
            }
            _context.SaveChanges();

            string errorDelCorreo = EnviarCorreoRecibo(emailUsuario, pelicula, fecha, sala, horario, asientos, cantidadAsientos, totalPagar, cine);

            ViewBag.Pelicula = pelicula;
            ViewBag.Fecha = fecha;
            ViewBag.Sala = sala;
            ViewBag.Horario = horario;
            ViewBag.Asientos = asientos;
            ViewBag.Cantidad = cantidadAsientos;
            ViewBag.Total = totalPagar;
            ViewBag.Cine = cine;
            ViewBag.ErrorCorreo = errorDelCorreo;

            return View();
        }

        private string EnviarCorreoRecibo(string destinatario, string pelicula, string fecha, string sala, string horario, string asientos, int cantidad, string total, string cine)
        {
            try
            {
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

                using (var message = new MailMessage(new MailAddress(miCorreo, "Cinemex Loop"), new MailAddress(destinatario)))
                {
                    message.Subject = $"¡Tu compra en Cinemex {cine} ha sido exitosa!";
                    message.IsBodyHtml = true;
                    message.Body = $"<div style='font-family: Arial; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; padding: 20px;'><div style='background-color: #E50914; color: white; padding: 15px; text-align: center;'><h2>CINEMEX {cine.ToUpper()}</h2></div><div style='padding: 20px;'><h3>¡Hola {destinatario}!</h3><p>Tu compra para <strong>{pelicula}</strong> está confirmada.</p><hr><p><strong>Fecha:</strong> {fecha}</p><p><strong>Horario:</strong> {horario} hrs</p><p><strong>{sala}</strong></p><p><strong>Asientos:</strong> {asientos}</p><p><strong>Boletos:</strong> {cantidad}</p><h3><strong>Total Pagado: {total}</strong></h3></div></div>";
                    smtp.Send(message);
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public IActionResult Privacy() { return View(); }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    }
}