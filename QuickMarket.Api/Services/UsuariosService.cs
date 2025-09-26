using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace QuickMarket.Api.Services
{
    public interface IUsuariosService
    {
        Task<UserDto> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default);
        Task<string> LoginAsync(LoginDto dto, CancellationToken ct = default);
        Task<PagedResult<UserDto>> ListAsync(int page, int pageSize, string? q, CancellationToken ct = default);
        Task<UserDto?> GetAsync(int id, CancellationToken ct = default);
        Task<UserDto> UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task RequestPasswordResetAsync(ResetRequestDto dto, CancellationToken ct = default);
        Task CompletePasswordResetAsync(ResetPasswordDto dto, CancellationToken ct = default);
    }

    public class UsuariosService : IUsuariosService
    {
        private readonly QuickMarketContext _db;
        private readonly IJwtTokenService _jwt;
        private readonly IEmailService _email;

        public UsuariosService(QuickMarketContext db, IJwtTokenService jwt, IEmailService email)
        {
            _db = db; _jwt = jwt; _email = email;
        }

        private static string CI(string s) => s.Trim().ToUpperInvariant();
        private static string N(string s) => s.Trim();
        private static string Hash(string pwd) => BCrypt.Net.BCrypt.HashPassword(pwd);
        private static bool Verify(string pwd, string hash) => BCrypt.Net.BCrypt.Verify(pwd, hash);

        // ====================
        // Registro de usuario
        // ====================
        public async Task<UserDto> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
        {
            var u = new Usuario
            {
                Username = N(dto.Username),
                Email = N(dto.Email),
                PasswordHash = Hash(dto.Password),
                Rol = "cliente",
                Estado = "activo"
            };

            _db.Usuarios.Add(u);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is OracleException ox && ox.Number == 1 /* ORA-00001 unique constraint */)
            {
                var m = ox.Message?.ToUpperInvariant() ?? string.Empty;
                if (m.Contains("UQ_USUARIOS_USERNAME_CI") || m.Contains("IX_USUARIOS_USERNAME"))
                    throw new InvalidOperationException("El username ya existe.");
                if (m.Contains("UQ_USUARIOS_EMAIL_CI") || m.Contains("IX_USUARIOS_EMAIL"))
                    throw new InvalidOperationException("El email ya existe.");
                throw new InvalidOperationException("Ya existe un usuario con ese username o email.");
            }

            // Email de bienvenida (best-effort)
            try
            {
                var subject = "¡Bienvenido/a a la familia QuickMarket!";
                var html = $@"
                    <div style='font-family:Arial,sans-serif;line-height:1.45'>
                      <h2 style='margin:0 0 8px'>¡Hola, {System.Net.WebUtility.HtmlEncode(u.Username)}!</h2>
                      <p>Gracias por registrarte en <b>QuickMarket</b> 🛒.</p>
                      <ul>
                        <li>Ofertas y promociones semanales</li>
                        <li>Historial de pedidos y seguimiento</li>
                        <li>Soporte cuando lo necesites</li>
                      </ul>
                      <p style='margin:12px 0 0'>¡Nos alegra tenerte con nosotros! 💚</p>
                    </div>";
                await _email.SendAsync(u.Email, subject, html);
            }
            catch { /* opcional log */ }

            return new UserDto(u.IdUsuario, u.Username, u.Email, u.Rol, u.Estado, u.CreadoEn, u.ActualizadoEn);
        }

        // ====================
        // Login
        // ====================
        public async Task<string> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            var key = CI(dto.UsernameOrEmail);
            var user = await _db.Usuarios
                .FirstOrDefaultAsync(x => x.Username.ToUpper() == key || x.Email.ToUpper() == key, ct);

            if (user is null) throw new UnauthorizedAccessException("Credenciales inválidas.");
            if (!Verify(dto.Password, user.PasswordHash)) throw new UnauthorizedAccessException("Credenciales inválidas.");
            if (!string.Equals(user.Estado, "activo", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("La cuenta está inactiva.");

            return _jwt.CreateToken(user.Username, user.Email, user.IdUsuario.ToString(), user.Rol);
        }

        // ====================
        // Listado paginado
        // ====================
        public async Task<PagedResult<UserDto>> ListAsync(int page, int pageSize, string? q, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var cq = CI(q);
                query = query.Where(x =>
                    x.Username.ToUpper().Contains(cq) ||
                    x.Email.ToUpper().Contains(cq) ||
                    x.Rol.ToUpper().Contains(cq) ||
                    x.Estado.ToUpper().Contains(cq)
                );
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.IdUsuario)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserDto(
                    x.IdUsuario,
                    x.Username,
                    x.Email,
                    x.Rol,
                    x.Estado,
                    x.CreadoEn,
                    x.ActualizadoEn
                ))
                .ToListAsync(ct);

            return new PagedResult<UserDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<UserDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var u = await _db.Usuarios.FindAsync(new object?[] { id }, ct);
            return u is null ? null : new UserDto(u.IdUsuario, u.Username, u.Email, u.Rol, u.Estado, u.CreadoEn, u.ActualizadoEn);
        }

        public async Task<UserDto> UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct = default)
        {
            var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id, ct)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Username))
                u.Username = N(dto.Username);

            if (!string.IsNullOrWhiteSpace(dto.Email))
                u.Email = N(dto.Email);

            if (!string.IsNullOrWhiteSpace(dto.Rol))
                u.Rol = N(dto.Rol!);

            if (!string.IsNullOrWhiteSpace(dto.Estado))
                u.Estado = N(dto.Estado!);

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                u.PasswordHash = Hash(dto.NewPassword!);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is OracleException ox && ox.Number == 1 /* ORA-00001 */)
            {
                var m = ox.Message?.ToUpperInvariant() ?? string.Empty;
                if (m.Contains("UQ_USUARIOS_USERNAME_CI") || m.Contains("IX_USUARIOS_USERNAME"))
                    throw new InvalidOperationException("El username ya existe.");
                if (m.Contains("UQ_USUARIOS_EMAIL_CI") || m.Contains("IX_USUARIOS_EMAIL"))
                    throw new InvalidOperationException("El email ya existe.");
                throw new InvalidOperationException("Ya existe un usuario con ese username o email.");
            }

            return new UserDto(u.IdUsuario, u.Username, u.Email, u.Rol, u.Estado, u.CreadoEn, u.ActualizadoEn);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id, ct)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");
            _db.Usuarios.Remove(u);
            await _db.SaveChangesAsync(ct);
        }

        // ==========================
        // Password Reset (5 dígitos)
        // ==========================
        public async Task RequestPasswordResetAsync(ResetRequestDto dto, CancellationToken ct = default)
        {
            var emailKey = CI(dto.Email);
            var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Email.ToUpper() == emailKey, ct);
            if (u is null) return; // no revelar existencia

            u.ResetToken = Generate5DigitCode();
            u.ResetExpira = DateTime.UtcNow.AddMinutes(15);
            await _db.SaveChangesAsync(ct);

            var html = $@"
                <div style='font-family:Arial,sans-serif;'>
                  <h2>Recuperación de contraseña</h2>
                  <p>Usa este código para restablecer tu contraseña:</p>
                  <div style='font-size:28px;font-weight:bold;letter-spacing:4px;'>{u.ResetToken}</div>
                  <p>El código expira en <b>15 minutos</b>.</p>
                </div>";
            try { await _email.SendAsync(u.Email, "Código de verificación (5 dígitos)", html); }
            catch { /* log opcional */ }
        }

        public async Task CompletePasswordResetAsync(ResetPasswordDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                throw new InvalidOperationException("Código requerido.");

            var code = new string(dto.Token.Where(char.IsDigit).ToArray());
            if (code.Length != 5)
                throw new InvalidOperationException("Código inválido.");

            var now = DateTime.UtcNow;

            var u = await _db.Usuarios.FirstOrDefaultAsync(x =>
                x.ResetToken == code &&
                x.ResetExpira != null &&
                x.ResetExpira > now, ct);

            if (u is null)
                throw new InvalidOperationException("Código inválido o expirado.");

            u.PasswordHash = Hash(dto.NewPassword);
            u.ResetToken = null;
            u.ResetExpira = null;

            await _db.SaveChangesAsync(ct);

            var html = @"
                <div style='font-family:Arial,sans-serif;'>
                  <h2>Contraseña restablecida</h2>
                  <p>Tu contraseña se ha restablecido correctamente.</p>
                  <p>Si no fuiste tú, por favor contáctanos de inmediato.</p>
                </div>";
            try { await _email.SendAsync(u.Email, "Contraseña restablecida", html); }
            catch { /* log opcional */ }
        }

        // ===== Helpers =====
        private static string Generate5DigitCode()
        {
            var value = RandomNumberGenerator.GetInt32(10000, 100000);
            return value.ToString();
        }
    }
}
