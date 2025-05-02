using EnviarEmail.AppWebMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnviarEmail.AppWebMVC.Controllers
{
    public class EmailController : Controller
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnviarCorreo(string destino, string asunto, string mensaje, List<IFormFile> archivos)
        {
            try
            {
                await _emailService.SendEmailAsync(destino, asunto, mensaje, archivos);
                TempData["MensajeExito"] = "Correo enviado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al enviar correo: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

    }
}
