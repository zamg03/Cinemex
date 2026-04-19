using Cinemex.Models;
using Cinemex.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace Cinemex.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() { return View(); }

        public IActionResult ComprarBoletos()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            return View();
        }

        // Recibe el horario y el cine seleccionado
        public IActionResult Asientos(string horario = "14:30", string cine = "Mol Concordia")
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            ViewBag.Horario = horario;
            ViewBag.Cine = cine;

            // Busca asientos ocupados SOLO en ese cine y en esa hora
            var asientosOcupados = _context.Reservas
                                           .Where(r => r.Horario == horario && r.Cine == cine)
                                           .Select(r => r.Asiento)
                                           .ToList();

            ViewBag.Ocupados = asientosOcupados;
            return View();
        }

        [HttpPost]
        public IActionResult Pago(string horario, string asientos, int cantidadAsientos, string totalPagar, string cine)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            ViewBag.Horario = horario;
            ViewBag.Asientos = asientos;
            ViewBag.Cantidad = cantidadAsientos;
            ViewBag.Total = totalPagar;
            ViewBag.Cine = cine;

            return View();
        }

        [HttpPost]
        public IActionResult Confirmacion(string horario, string asientos, int cantidadAsientos, string totalPagar, string cine)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var emailUsuario = User.Identity.Name;
            var listaAsientos = asientos.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Guarda la reserva con el cine correspondiente
            foreach (var asiento in listaAsientos)
            {
                _context.Reservas.Add(new Reserva { UsuarioEmail = emailUsuario, Horario = horario, Asiento = asiento, Cine = cine });
            }
            _context.SaveChanges();

            EnviarCorreoRecibo(emailUsuario, horario, asientos, cantidadAsientos, totalPagar, cine);

            ViewBag.Horario = horario;
            ViewBag.Asientos = asientos;
            ViewBag.Cantidad = cantidadAsientos;
            ViewBag.Total = totalPagar;
            ViewBag.Cine = cine;

            return View();
        }

        private void EnviarCorreoRecibo(string destinatario, string horario, string asientos, int cantidad, string total, string cine)
        {
            try
            {
                string miCorreo = "TU_CORREO@gmail.com";
                string miContrasenaApp = "TU_CONTRASEÑA_DE_APLICACION";

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
                    message.Body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; padding: 20px;'>
                            <div style='background-color: #E50914; color: white; padding: 15px; text-align: center;'><h2>CINEMEX {cine.ToUpper()}</h2></div>
                            <div style='padding: 20px;'>
                                <h3>¡Hola {destinatario}!</h3>
                                <p>Tu compra para <strong>Michael: La Historia de Michael Jackson</strong> está confirmada.</p><hr>
                                <p><strong>Horario:</strong> {horario} hrs</p>
                                <p><strong>Asientos:</strong> {asientos}</p>
                                <p><strong>Boletos:</strong> {cantidad}</p>
                                <h3><strong>Total Pagado: {total}</strong></h3><hr>
                                <p style='color: #777; font-size: 12px;'>Muestra este correo en la entrada de la sala.</p>
                            </div>
                        </div>";
                    smtp.Send(message);
                }
            }
            catch (Exception ex) { Console.WriteLine("Error enviando correo: " + ex.Message); }
        }

        public IActionResult Privacy() { return View(); }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    }
}