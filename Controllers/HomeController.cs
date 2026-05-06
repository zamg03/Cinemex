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

        // Vistas informativas básicas
        public IActionResult Index() { return View(); }
        public IActionResult Terminos() { return View(); }
        public IActionResult Loop() { return View(); }
        public IActionResult Imax() { return View(); }

        // VISTA CARTELERA: Permite filtrar por preventas o mediante una barra de búsqueda.
        public IActionResult Cartelera(bool soloPreventas = false, string buscar = null)
        {
            // Pasamos los filtros a la vista a través de ViewBag para que la vista decida qué películas mostrar.
            ViewBag.SoloPreventas = soloPreventas;
            ViewBag.Busqueda = buscar;
            return View();
        }

        // PASO 1 DE COMPRA: Selección de asientos
        public IActionResult Asientos(string pelicula, string poster, string fecha, string sala, string horario = "14:30", string cine = "Mol Concordia")
        {
            // Si el usuario no ha iniciado sesión, lo rebotamos a la pantalla de Login y le guardamos la página actual.
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            // Arrastramos toda la información de la película a la vista.
            ViewBag.Pelicula = pelicula;
            ViewBag.Poster = poster;
            ViewBag.Fecha = fecha;
            ViewBag.Sala = sala;
            ViewBag.Horario = horario;
            ViewBag.Cine = cine;

            // Filtro LINQ: Busca en la base de datos únicamente los asientos que coincidan exactamente con
            // la misma película, en la misma sucursal, el mismo día, a la misma hora y en la misma sala.
            var asientosOcupados = _context.Reservas
                                           .Where(r => r.Horario == horario && r.Cine == cine && r.Pelicula == pelicula && r.Fecha == fecha && r.Sala == sala)
                                           .Select(r => r.Asiento).ToList();

            ViewBag.Ocupados = asientosOcupados; // Pasamos la lista de asientos no disponibles para marcarlos en gris.
            return View();
        }

        // PASO 2 DE COMPRA: Formulario de Pago
        [HttpPost]
        public IActionResult Pago(string pelicula, string poster, string fecha, string sala, string horario, string asientos, int cantidadAsientos, string totalPagar, string cine)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            // Solo recibimos la información del paso 1 y la enviamos intacta al paso 2.
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

        // PASO 3 DE COMPRA: Guardar en Base de Datos y Generar Recibo
        [HttpPost]
        public IActionResult Confirmacion(string pelicula, string fecha, string sala, string horario, string asientos, int cantidadAsientos, string totalPagar, string cine)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            // Extraemos el correo real de la sesión activa del usuario.
            var emailUsuario = User.FindFirst(ClaimTypes.Email)?.Value;

            // Dividimos la cadena de texto "G5, G6" en un arreglo para poder guardar cada asiento individualmente.
            var listaAsientos = asientos.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Bucle que crea un registro en la Base de Datos por cada asiento comprado.
            foreach (var asiento in listaAsientos)
            {
                _context.Reservas.Add(new Reserva { UsuarioEmail = emailUsuario, Pelicula = pelicula, Fecha = fecha, Sala = sala, Horario = horario, Asiento = asiento, Cine = cine });
            }
            _context.SaveChanges(); // Guardado definitivo

            // Ejecutamos la función de correo. Si falla, nos devolverá el texto del error.
            string errorDelCorreo = EnviarCorreoRecibo(emailUsuario, pelicula, fecha, sala, horario, asientos, cantidadAsientos, totalPagar, cine);

            // Pasamos los datos finales a la vista para construir el Boleto Virtual visualmente.
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

        // FUNCIÓN PRIVADA: Sistema de envío de correos vía SMTP
        private string EnviarCorreoRecibo(string destinatario, string pelicula, string fecha, string sala, string horario, string asientos, int cantidad, string total, string cine)
        {
            try
            {
                // Credenciales de la cuenta remitente
                string miCorreo = "danielfzg21@gmail.com";
                string miContrasenaApp = "mgbearathouprdwp";

                // Configuración del servidor de Google (SMTP)
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(miCorreo, miContrasenaApp)
                };

                // Construcción del mensaje con código HTML integrado para un diseño profesional
                using (var message = new MailMessage(new MailAddress(miCorreo, "Cinemex Loop"), new MailAddress(destinatario)))
                {
                    message.Subject = $"¡Tu compra en Cinemex {cine} ha sido exitosa!";
                    message.IsBodyHtml = true;
                    message.Body = $"<div style='font-family: Arial; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; padding: 20px;'><div style='background-color: #E50914; color: white; padding: 15px; text-align: center;'><h2>CINEMEX {cine.ToUpper()}</h2></div><div style='padding: 20px;'><h3>¡Hola {destinatario}!</h3><p>Tu compra para <strong>{pelicula}</strong> está confirmada.</p><hr><p><strong>Fecha:</strong> {fecha}</p><p><strong>Horario:</strong> {horario} hrs</p><p><strong>{sala}</strong></p><p><strong>Asientos:</strong> {asientos}</p><p><strong>Boletos:</strong> {cantidad}</p><h3><strong>Total Pagado: {total}</strong></h3></div></div>";

                    // Enviar el correo
                    smtp.Send(message);
                }
                return null; // Si no hay error, regresamos nulo indicando éxito total
            }
            catch (Exception ex)
            {
                // En caso de que el antivirus, firewall o Google bloqueen la petición, atrapamos el error para no colapsar la página.
                return ex.Message;
            }
        }

        public IActionResult Privacy() { return View(); }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    }
}