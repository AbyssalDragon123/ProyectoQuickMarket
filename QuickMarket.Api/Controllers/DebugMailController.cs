// Controllers/DebugMailController.cs
using Microsoft.AspNetCore.Mvc;
using QuickMarket.Api.Services;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DebugMailController : ControllerBase
    {
        private readonly IEmailService _email;
        private readonly IConfiguration _cfg;

        public DebugMailController(IEmailService email, IConfiguration cfg)
        {
            _email = email;
            _cfg = cfg;
        }

        // GET: /api/debug/email?to=tu_gmail_destino@gmail.com
        [HttpGet("email")]
        public async Task<IActionResult> SendTest([FromQuery] string to)
        {
            var from = _cfg["Email:From"];
            var host = _cfg["Email:SmtpHost"];
            var port = _cfg["Email:SmtpPort"];

            var html = $@"
                <h3>Prueba MailKit</h3>
                <p>Esto es un test desde QuickMarket.Api.</p>
                <ul>
                  <li>From: {from}</li>
                  <li>SmtpHost: {host}</li>
                  <li>SmtpPort: {port}</li>
                </ul>";

            await _email.SendAsync(to, "Prueba de correo - QuickMarket", html);
            return Ok(new { message = "Correo de prueba enviado", to, from, host, port });
        }
    }
}
