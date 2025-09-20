// Controllers/VentasController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;
using QuickMarket.Api.Services;
using System.Text;

// Si llamas a un paquete Oracle, añade esta referencia (y el paquete NuGet ODP.NET):
using Oracle.ManagedDataAccess.Client;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly QuickMarketContext _db;
        private readonly IEmailService _email;
        private readonly ILogger<VentasController> _log;

        public VentasController(QuickMarketContext db, IEmailService email, ILogger<VentasController> log)
        {
            _db = db;
            _email = email;
            _log = log;
        }

        private static decimal UserId(ClaimsPrincipal u)
        {
            var sub = u.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedAccessException("Token sin sub.");
            return decimal.Parse(sub);
        }

        // POST api/ventas  -> crear orden
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearOrdenDto dto)
        {
            if (dto.Items is null || dto.Items.Count == 0)
                return BadRequest("La orden no tiene items.");

            var uid = UserId(User);

            // Cliente del usuario
            var cliente = await _db.CLIENTES.FirstOrDefaultAsync(c => c.ID_USUARIO == uid);
            if (cliente is null)
                return BadRequest("Completa tu perfil de cliente antes de comprar.");

            // Crear venta (totales los calculará la BD)
            var venta = new VENTAS
            {
                FECHA = DateTime.UtcNow,
                ID_CLIENTE = cliente.ID_CLIENTE,
                TOTAL_BRUTO = 0,
                TOTAL_IMPUESTOS = 0,
                TOTAL_NETO = 0,
                CREADO_EN = DateTime.UtcNow
            };
            _db.VENTAS.Add(venta);
            await _db.SaveChangesAsync(); // ID_VENTA

            // Insertar detalle
            foreach (var it in dto.Items)
            {
                _db.DETALLE_VENTAS.Add(new DETALLE_VENTAS
                {
                    ID_VENTA = venta.ID_VENTA,
                    ID_PRODUCTO = it.IdProducto,
                    CANTIDAD = it.Cantidad,
                    PRECIO_UNITARIO = it.PrecioUnitario
                });
            }
            await _db.SaveChangesAsync();

            // (Opcional) Llamar paquete para totales. Si usas solo triggers, omite esto.
            try
            {
                await _db.Database.ExecuteSqlRawAsync(
                    "BEGIN PKG_VENTAS.CALCULAR_TOTALES(:p_id_venta); END;",
                    new OracleParameter("p_id_venta", (object)venta.ID_VENTA)
                );
            }
            catch (Exception ex)
            {
                _log.LogInformation(ex, "No se invocó paquete PKG_VENTAS (se asume triggers).");
            }

            // Releer la venta ya calculada
            await _db.Entry(venta).ReloadAsync();

            // Detalle para factura
            var detalle = await _db.DETALLE_VENTAS.AsNoTracking()
                .Where(d => d.ID_VENTA == venta.ID_VENTA)
                .Select(d => new VentaItemDto(
                    d.ID_DETALLE, d.ID_PRODUCTO, d.CANTIDAD, d.PRECIO_UNITARIO, d.CANTIDAD * d.PRECIO_UNITARIO
                ))
                .ToListAsync();

            // Email destino: primero CLIENTES.EMAIL, si no, USUARIOS.EMAIL
            var emailTo = cliente.EMAIL;
            if (string.IsNullOrWhiteSpace(emailTo))
            {
                var usuario = await _db.USUARIOS.AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.ID_USUARIO == uid);
                emailTo = usuario?.EMAIL;
            }

            if (!string.IsNullOrWhiteSpace(emailTo))
            {
                var html = RenderFacturaHtml(venta, detalle, cliente, dto.Notas);
                try
                {
                    await _email.SendAsync(emailTo!, $"Factura #{venta.ID_VENTA} - QuickMarket", html);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "No se pudo enviar la factura por email.");
                }
            }

            return Ok(new
            {
                message = "Orden creada.",
                idVenta = venta.ID_VENTA,
                totals = new { venta.TOTAL_BRUTO, venta.TOTAL_IMPUESTOS, venta.TOTAL_NETO }
            });
        }

        // GET api/ventas/{id} -> una venta del usuario actual
        [HttpGet("{id:decimal}")]
        public async Task<ActionResult<VentaConDetalleDto>> GetById(decimal id)
        {
            var uid = UserId(User);

            var venta = await _db.VENTAS.AsNoTracking()
                .FirstOrDefaultAsync(v => v.ID_VENTA == id);

            if (venta is null) return NotFound();

            var cliente = await _db.CLIENTES.AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID_CLIENTE == venta.ID_CLIENTE);

            if (cliente is null || cliente.ID_USUARIO != uid) return Forbid();

            var detalle = await _db.DETALLE_VENTAS.AsNoTracking()
                .Where(d => d.ID_VENTA == id)
                .Select(d => new VentaItemDto(
                    d.ID_DETALLE, d.ID_PRODUCTO, d.CANTIDAD, d.PRECIO_UNITARIO, d.CANTIDAD * d.PRECIO_UNITARIO
                ))
                .ToListAsync();

            var resumen = new VentaResumenDto(
                venta.ID_VENTA, venta.FECHA, venta.ID_CLIENTE,
                venta.TOTAL_BRUTO, venta.TOTAL_IMPUESTOS, venta.TOTAL_NETO
            );

            return new VentaConDetalleDto(resumen, detalle);
        }

        // POST api/ventas/{id}/reenviar-factura
        [HttpPost("{id:decimal}/reenviar-factura")]
        public async Task<IActionResult> ReenviarFactura(decimal id)
        {
            var uid = UserId(User);

            var venta = await _db.VENTAS.AsNoTracking()
                .FirstOrDefaultAsync(v => v.ID_VENTA == id);

            if (venta is null) return NotFound();

            var cliente = await _db.CLIENTES.AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID_CLIENTE == venta.ID_CLIENTE);

            if (cliente is null || cliente.ID_USUARIO != uid) return Forbid();

            var detalle = await _db.DETALLE_VENTAS.AsNoTracking()
                .Where(d => d.ID_VENTA == id)
                .Select(d => new VentaItemDto(
                    d.ID_DETALLE, d.ID_PRODUCTO, d.CANTIDAD, d.PRECIO_UNITARIO, d.CANTIDAD * d.PRECIO_UNITARIO
                ))
                .ToListAsync();

            var emailTo = cliente.EMAIL;
            if (string.IsNullOrWhiteSpace(emailTo))
            {
                var usuario = await _db.USUARIOS.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.ID_USUARIO == uid);
                emailTo = usuario?.EMAIL;
            }

            if (string.IsNullOrWhiteSpace(emailTo))
                return BadRequest("No hay email registrado para enviar la factura.");

            var html = RenderFacturaHtml(venta, detalle, cliente, null);
            await _email.SendAsync(emailTo!, $"Factura #{venta.ID_VENTA} - QuickMarket", html);

            return Ok(new { message = "Factura reenviada." });
        }

        // ===== Helpers =====
        private static string RenderFacturaHtml(VENTAS v, IEnumerable<VentaItemDto> items, CLIENTES c, string? notas)
        {
            var sb = new StringBuilder();
            sb.Append($@"<h2>Factura #{v.ID_VENTA}</h2>
                <p><strong>Fecha:</strong> {v.FECHA:yyyy-MM-dd HH:mm} UTC</p>
                <p><strong>Cliente:</strong> {c.NOMBRE}</p>");

            if (!string.IsNullOrWhiteSpace(c.DIRECCION))
                sb.Append($"<p><strong>Dirección:</strong> {c.DIRECCION}</p>");
            if (!string.IsNullOrWhiteSpace(c.TELEFONO))
                sb.Append($"<p><strong>Teléfono:</strong> {c.TELEFONO}</p>");
            if (!string.IsNullOrWhiteSpace(notas))
                sb.Append($"<p><strong>Notas:</strong> {System.Net.WebUtility.HtmlEncode(notas)}</p>");

            sb.Append(@"<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse'>
                <thead><tr><th>Producto</th><th>Cant</th><th>P.Unit</th><th>Importe</th></tr></thead><tbody>");

            foreach (var it in items)
            {
                var imp = it.Importe;
                sb.Append($"<tr><td>{it.IdProducto}</td><td>{it.Cantidad}</td><td>{it.PrecioUnitario:F2}</td><td>{imp:F2}</td></tr>");
            }

            sb.Append($@"</tbody></table>
                <p><strong>Total Bruto:</strong> {v.TOTAL_BRUTO:F2}</p>
                <p><strong>Impuestos:</strong> {v.TOTAL_IMPUESTOS:F2}</p>
                <p><strong>Total Neto:</strong> {v.TOTAL_NETO:F2}</p>");

            return sb.ToString();
        }
    }
}
