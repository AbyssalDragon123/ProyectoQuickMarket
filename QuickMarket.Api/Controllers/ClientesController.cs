// Controllers/ClientesController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly QuickMarketContext _db;
        public ClientesController(QuickMarketContext db) => _db = db;

        private static decimal UserId(ClaimsPrincipal u)
        {
            var sub = u.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? throw new UnauthorizedAccessException("Token sin sub.");
            return decimal.Parse(sub);
        }

        // GET: api/clientes/me
        [HttpGet("me")]
        public async Task<ActionResult<ClienteDto>> GetMine()
        {
            var uid = UserId(User);
            var c = await _db.CLIENTES.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.ID_USUARIO == uid);
            if (c is null) return NotFound();

            return new ClienteDto(
                c.ID_CLIENTE, c.ID_USUARIO, c.NOMBRE, c.EMAIL, c.TELEFONO,
                c.DIRECCION, c.DEPARTAMENTO, c.MUNICIPIO, c.REFERENCIA
            );
        }

        // PUT: api/clientes/me  (crea o actualiza el perfil del usuario actual)
        [HttpPut("me")]
        public async Task<IActionResult> UpsertMine([FromBody] ClienteUpsertDto dto)
        {
            var uid = UserId(User);
            var c = await _db.CLIENTES.FirstOrDefaultAsync(x => x.ID_USUARIO == uid);

            if (c is null)
            {
                c = new CLIENTES
                {
                    ID_USUARIO = uid,
                    NOMBRE = dto.Nombre,
                    EMAIL = dto.Email?.Trim(),
                    TELEFONO = dto.Telefono?.Trim(),
                    DIRECCION = dto.Direccion?.Trim(),
                    DEPARTAMENTO = dto.Departamento?.Trim(),
                    MUNICIPIO = dto.Municipio?.Trim(),
                    REFERENCIA = dto.Referencia?.Trim(),
                    CREADO_EN = DateTime.UtcNow
                };
                _db.CLIENTES.Add(c);
            }
            else
            {
                c.NOMBRE = dto.Nombre;
                c.EMAIL = dto.Email?.Trim();
                c.TELEFONO = dto.Telefono?.Trim();
                c.DIRECCION = dto.Direccion?.Trim();
                c.DEPARTAMENTO = dto.Departamento?.Trim();
                c.MUNICIPIO = dto.Municipio?.Trim();
                c.REFERENCIA = dto.Referencia?.Trim();
                c.ACTUALIZADO_EN = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Perfil de cliente guardado.", idCliente = c.ID_CLIENTE });
        }
    }
}
