using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;
using QuickMarket.Api.Services;
using QuickMarket.Api.Utils;
using System.IdentityModel.Tokens.Jwt;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly QuickMarketContext _db;
        private readonly IJwtTokenService _jwt;
        private readonly IEmailService _email;

        public AuthController(QuickMarketContext db, IJwtTokenService jwt, IEmailService email)
        {
            _db = db;
            _jwt = jwt;
            _email = email;
        }

        // Helpers de normalización (case-insensitive consistente)
        private static string NormUsername(string s) => s.Trim().ToUpperInvariant();
        private static string NormGmail(string s) => s.Trim().ToUpperInvariant();

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var username = NormUsername(dto.Username);
            var gmail = NormGmail(dto.Gmail);

            if (!gmail.EndsWith("@GMAIL.COM", StringComparison.Ordinal))
                return BadRequest("Solo se permiten cuentas @gmail.com.");

            // --- Evitar AnyAsync con Oracle: usar SELECT 1 + FirstOrDefaultAsync
            var userNameExists = await _db.LOGIN.AsNoTracking()
                .Where(x => x.USERNAME == username)
                .Select(_ => 1)
                .FirstOrDefaultAsync() == 1;

            if (userNameExists)
                return Conflict("El username ya existe.");

            var gmailExists = await _db.LOGIN.AsNoTracking()
                .Where(x => x.GMAIL == gmail)
                .Select(_ => 1)
                .FirstOrDefaultAsync() == 1;

            if (gmailExists)
                return Conflict("El gmail ya está en uso.");

            var empleadoExiste = await _db.EMPLEADOS.AsNoTracking()
                .Where(e => e.ID_EMPLEADO == dto.IdEmpleado)
                .Select(_ => 1)
                .FirstOrDefaultAsync() == 1;

            if (!empleadoExiste)
                return BadRequest("El empleado no existe.");
            // --- Fin reemplazos

            var entity = new LOGIN
            {
                USERNAME = username,
                GMAIL = gmail,
                PASSWORD_HASH = SecurityUtils.HashPassword(dto.Password),
                ID_EMPLEADO = dto.IdEmpleado,
                ESTADO = "activo",
                CREADO_EN = DateTime.UtcNow
            };

            _db.LOGIN.Add(entity);
            await _db.SaveChangesAsync();

            return Created($"api/auth/users/{entity.ID_LOGIN}", new
            {
                entity.ID_LOGIN,
                entity.USERNAME,
                gmail = entity.GMAIL,
                entity.ID_EMPLEADO
            });
        }


        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var username = NormUsername(dto.Username);

            var user = await _db.LOGIN.FirstOrDefaultAsync(x => x.USERNAME == username && x.ESTADO == "activo");

            if (user is null || !SecurityUtils.VerifyPassword(dto.Password, user.PASSWORD_HASH))
                return Unauthorized("Credenciales inválidas.");

            var token = _jwt.CreateToken(user.USERNAME, user.GMAIL, user.ID_LOGIN.ToString(), "empleado");

            return Ok(new
            {
                message = "Login OK",
                token,
                user = new { user.ID_LOGIN, user.USERNAME, gmail = user.GMAIL, user.ID_EMPLEADO }
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
            var user = await _db.LOGIN.AsNoTracking().FirstOrDefaultAsync(x => x.USERNAME == norm);
            if (user is null) return NotFound();

            return Ok(new
            {
                user.ID_LOGIN,
                user.USERNAME,
                gmail = user.GMAIL,
                user.ID_EMPLEADO,
                user.ESTADO,
                user.CREADO_EN,
                user.ACTUALIZADO_EN
            });
        }

        // POST: api/auth/request-reset  (envía código 5 dígitos que expira en 10 minutos)
        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestReset(RequestResetDto dto)
        {
            var gmail = NormGmail(dto.Gmail);

            // Privacidad: no revelar existencia
            var user = await _db.LOGIN.FirstOrDefaultAsync(x => x.GMAIL == gmail && x.ESTADO == "activo");
            if (user is null)
            {
                await Task.Delay(100);
                return Ok(new { message = "Si la cuenta existe, se enviará un código al correo." });
            }

            var code = SecurityUtils.Generate5DigitCode();
            user.RESET_TOKEN = code;
            user.RESET_TOKEN_EXP = DateTime.UtcNow.AddMinutes(10);
            user.ACTUALIZADO_EN = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var html = $@"
                <h2>Recuperación de contraseña</h2>
                <p>Tu código de verificación es:</p>
                <h1 style=""letter-spacing:3px"">{code}</h1>
                <p>Este código expira en <strong>10 minutos</strong>.</p>
                <p>Si no solicitaste este código, ignora este mensaje.</p>";

            try
            {
                await _email.SendAsync(user.GMAIL, "Código de verificación - QuickMarket", html);
            }
            catch
            {
                user.RESET_TOKEN = null;
                user.RESET_TOKEN_EXP = null;
                user.ACTUALIZADO_EN = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return StatusCode(503, "No se pudo enviar el correo. Intenta nuevamente en unos minutos.");
            }

            return Ok(new { message = "Si la cuenta existe, se enviará un código al correo." });
        }

        // POST: api/auth/verify-code
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode(VerifyCodeDto dto)
        {
            var gmail = NormGmail(dto.Gmail);
            var user = await _db.LOGIN.FirstOrDefaultAsync(x => x.GMAIL == gmail);

            if (user is null || user.RESET_TOKEN is null || user.RESET_TOKEN_EXP is null)
                return BadRequest("Código inválido o no solicitado.");

            if (user.RESET_TOKEN_EXP <= DateTimeOffset.UtcNow)
                return BadRequest("El código ha expirado.");

            if (!string.Equals(user.RESET_TOKEN, dto.Code, StringComparison.Ordinal))
                return BadRequest("Código inválido.");

            return Ok(new { message = "Código válido." });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var gmail = NormGmail(dto.Gmail);
            var user = await _db.LOGIN.FirstOrDefaultAsync(x => x.GMAIL == gmail);
            if (user is null)
                return BadRequest("Código inválido o no solicitado.");

            if (user.RESET_TOKEN is null || user.RESET_TOKEN_EXP is null)
                return BadRequest("Código inválido o no solicitado.");

            if (user.RESET_TOKEN_EXP <= DateTimeOffset.UtcNow)
                return BadRequest("El código ha expirado.");

            if (!string.Equals(user.RESET_TOKEN, dto.Code, StringComparison.Ordinal))
                return BadRequest("Código inválido.");

            // Política de contraseña: aquí podrías validar fuerza adicional con Regex
            user.PASSWORD_HASH = SecurityUtils.HashPassword(dto.NewPassword);
            user.RESET_TOKEN = null;
            user.RESET_TOKEN_EXP = null;
            user.ACTUALIZADO_EN = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Contraseña actualizada." });
        }
    }
}