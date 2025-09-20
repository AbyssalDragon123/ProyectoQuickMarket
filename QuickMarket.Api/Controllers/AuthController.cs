// Controllers/AuthController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;
using QuickMarket.Api.Services;
using QuickMarket.Api.Utils;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly QuickMarketContext _db;
        private readonly IJwtTokenService _jwt;
        private readonly IEmailService _email;
        private readonly ILogger<AuthController> _log;

        private const int ResetCodeMinutes = 5;      // ⬅️ antes era 1
        private const int ClockSkewSeconds = 10;     // ⬅️ tolerancia mínima

        public AuthController(QuickMarketContext db, IJwtTokenService jwt, IEmailService email, ILogger<AuthController> log)
        {
            _db = db;
            _jwt = jwt;
            _email = email;
            _log = log;
        }

        private static string NormUsername(string s) => s.Trim().ToUpperInvariant();
        private static string NormEmail(string s) => s.Trim().ToUpperInvariant();

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var username = NormUsername(dto.Username);
            var email = NormEmail(dto.Email);

            // Si quieres limitar temporalmente a Gmail, deja esto:
            if (!email.EndsWith("@GMAIL.COM", StringComparison.Ordinal))
                return BadRequest("Por ahora solo se permiten cuentas @gmail.com.");

            var userNameExists = await _db.USUARIOS.AsNoTracking()
                .Where(x => x.USERNAME == username)
                .Select(_ => 1).FirstOrDefaultAsync() == 1;

            if (userNameExists) return Conflict("El username ya existe.");

            var emailExists = await _db.USUARIOS.AsNoTracking()
                .Where(x => x.EMAIL == email)
                .Select(_ => 1).FirstOrDefaultAsync() == 1;

            if (emailExists) return Conflict("El email ya está en uso.");

            var entity = new USUARIOS
            {
                USERNAME = username,
                EMAIL = email,
                PASSWORD_HASH = SecurityUtils.HashPassword(dto.Password),
                ROL = "cliente",
                ESTADO = "activo",
                CREADO_EN = DateTime.UtcNow
            };

            _db.USUARIOS.Add(entity);
            await _db.SaveChangesAsync();

            return Created($"api/auth/users/{entity.ID_USUARIO}", new
            {
                entity.ID_USUARIO,
                entity.USERNAME,
                email = entity.EMAIL,
                entity.ROL
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var username = NormUsername(dto.Username);
            var user = await _db.USUARIOS.FirstOrDefaultAsync(x => x.USERNAME == username && x.ESTADO == "activo");

            if (user is null || !SecurityUtils.VerifyPassword(dto.Password, user.PASSWORD_HASH))
                return Unauthorized("Credenciales inválidas.");

            var token = _jwt.CreateToken(user.USERNAME, user.EMAIL, user.ID_USUARIO.ToString(), user.ROL);

            return Ok(new
            {
                message = "Login OK",
                token,
                user = new { user.ID_USUARIO, user.USERNAME, email = user.EMAIL, user.ROL }
            });
        }

        // GET: api/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var username = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ?? User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized("Token sin nombre de usuario.");

            var norm = NormUsername(username);
            var user = await _db.USUARIOS.AsNoTracking().FirstOrDefaultAsync(x => x.USERNAME == norm);
            if (user is null) return NotFound();

            return Ok(new
            {
                user.ID_USUARIO,
                user.USERNAME,
                email = user.EMAIL,
                user.ROL,
                user.ESTADO,
                user.CREADO_EN,
                user.ACTUALIZADO_EN
            });
        }

        // POST: api/auth/request-reset  (código 5 dígitos, expira en 5 minutos)
        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestReset(RequestResetDto dto)
        {
            var email = NormEmail(dto.Email);

            // No revelar existencia
            var user = await _db.USUARIOS.FirstOrDefaultAsync(x => x.EMAIL == email && x.ESTADO == "activo");
            if (user is null)
            {
                await Task.Delay(100);
                return Ok(new { message = "Se enviará un código si la cuenta existe." });
            }

            var code = SecurityUtils.Generate5DigitCode();

            user.RESET_TOKEN = code;
            user.RESET_EXPIRA = DateTime.UtcNow.AddMinutes(ResetCodeMinutes);
            user.ACTUALIZADO_EN = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var html = $@"
                <h2>Recuperación de contraseña</h2>
                <p>Tu código de verificación es:</p>
                <h1 style=""letter-spacing:3px"">{code}</h1>
                <p>Este código expira en <strong>{ResetCodeMinutes} minutos</strong>.</p>
                <p>Si solicitas otro código, este quedará invalidado.</p>
                <p>Si no solicitaste este código, ignora este mensaje.</p>";

            try
            {
                await _email.SendAsync(user.EMAIL, "Código de verificación - QuickMarket", html);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Fallo al enviar correo de reset, invalidando código.");
                user.RESET_TOKEN = null;
                user.RESET_EXPIRA = null;
                user.ACTUALIZADO_EN = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return StatusCode(503, "No se pudo enviar el correo. Intenta nuevamente en unos minutos.");
            }

            return Ok(new { message = "Código enviado si la cuenta existe." });
        }

        // POST: api/auth/verify-code
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode(VerifyCodeDto dto)
        {
            var email = NormEmail(dto.Email);
            var user = await _db.USUARIOS.FirstOrDefaultAsync(x => x.EMAIL == email);

            if (user is null || user.RESET_TOKEN is null || user.RESET_EXPIRA is null)
                return BadRequest("No hay un código activo. Solicítalo de nuevo.");

            // tolerancia de reloj
            var now = DateTime.UtcNow.AddSeconds(-ClockSkewSeconds);

            if (user.RESET_EXPIRA <= now)
                return BadRequest("El código ha expirado.");

            if (!string.Equals(user.RESET_TOKEN, dto.Code, StringComparison.Ordinal))
                return BadRequest("Código inválido.");

            return Ok(new { message = "Código válido." });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var email = NormEmail(dto.Email);
            var user = await _db.USUARIOS.FirstOrDefaultAsync(x => x.EMAIL == email);
            if (user is null || user.RESET_TOKEN is null || user.RESET_EXPIRA is null)
                return BadRequest("No hay un código activo. Solicítalo de nuevo.");

            var now = DateTime.UtcNow.AddSeconds(-ClockSkewSeconds);

            if (user.RESET_EXPIRA <= now)
                return BadRequest("El código ha expirado.");

            if (!string.Equals(user.RESET_TOKEN, dto.Code, StringComparison.Ordinal))
                return BadRequest("Código inválido.");

            user.PASSWORD_HASH = SecurityUtils.HashPassword(dto.NewPassword);
            user.RESET_TOKEN = null;
            user.RESET_EXPIRA = null;
            user.ACTUALIZADO_EN = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Aviso por email (no bloquear flujo si falla)
            try
            {
                await _email.SendAsync(
                    user.EMAIL,
                    "Tu contraseña fue cambiada - QuickMarket",
                    $@"<p>Hola {user.USERNAME},</p>
                       <p>Tu contraseña se cambió correctamente el {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC.</p>
                       <p>Si no fuiste tú, restablece tu clave y contáctanos.</p>"
                );
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "No se pudo enviar email de confirmación de cambio de contraseña.");
            }

            return Ok(new { message = "Contraseña actualizada." });
        }
    }
}
