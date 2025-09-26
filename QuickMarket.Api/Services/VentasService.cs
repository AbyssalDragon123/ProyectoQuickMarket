using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;

namespace QuickMarket.Api.Services
{
    public interface IVentasService
    {
        Task<int> CrearVentaAsync(CrearOrdenDto orden, CancellationToken ct = default);
        Task<VentaConDetalleDto?> ObtenerVentaAsync(int idVenta, CancellationToken ct = default);
    }

    public class VentasService : IVentasService
    {
        private readonly QuickMarketContext _db;
        public VentasService(QuickMarketContext db) { _db = db; }

        public async Task<int> CrearVentaAsync(CrearOrdenDto orden, CancellationToken ct = default)
        {
            if (orden is null) throw new ArgumentNullException(nameof(orden));
            if (orden.Items is null || orden.Items.Count == 0)
                throw new InvalidOperationException("La orden no tiene items.");
            if (orden.IdCliente <= 0)
                throw new InvalidOperationException("IdCliente inválido.");

            // 👇 Evitar THEN True/False en Oracle: usar COUNT(*) > 0 en vez de Any()
            var clienteExiste = (await _db.Clientes
                .AsNoTracking()
                .Where(c => c.IdCliente == orden.IdCliente)
                .CountAsync(ct)) > 0;

            if (!clienteExiste)
                throw new InvalidOperationException("El cliente no existe.");

            using var trx = await _db.Database.BeginTransactionAsync(ct);

            // 1) Cabecera
            var v = new Venta
            {
                IdCliente = orden.IdCliente,
                Fecha = DateTime.Now,
                TotalBruto = 0,
                TotalImpuestos = 0,
                TotalNeto = 0
            };
            _db.Ventas.Add(v);
            await _db.SaveChangesAsync(ct); // ID_VENTA por secuencia/trigger

            decimal totalBruto = 0m, totalImpuestos = 0m;

            // 2) Detalles
            foreach (var it in orden.Items)
            {
                if (it.Cantidad <= 0)
                    throw new InvalidOperationException("Cantidad debe ser > 0.");

                var idProd = it.IdProducto;

                var p = await _db.Productos
                    .FirstOrDefaultAsync(x => x.IdProducto == idProd, ct)
                    ?? throw new InvalidOperationException($"Producto {idProd} no existe.");

                var precio = (it.PrecioUnitario is null || it.PrecioUnitario == 0m)
                             ? p.PrecioUnitario
                             : it.PrecioUnitario.Value;

                if (precio < 0)
                    throw new InvalidOperationException("PrecioUnitario no puede ser negativo.");
                if (p.Stock < it.Cantidad)
                    throw new InvalidOperationException($"Stock insuficiente para {p.Nombre}.");

                p.Stock -= it.Cantidad;

                var det = new DetalleVenta
                {
                    IdVenta = v.IdVenta,
                    IdProducto = p.IdProducto,
                    Cantidad = it.Cantidad,
                    PrecioUnitario = precio
                };
                _db.DetalleVentas.Add(det);

                totalBruto += precio * it.Cantidad;
                totalImpuestos += Math.Round(precio * 0.12m, 2) * it.Cantidad; // IVA 12%
            }

            v.TotalBruto = Math.Round(totalBruto, 2);
            v.TotalImpuestos = Math.Round(totalImpuestos, 2);
            v.TotalNeto = v.TotalBruto + v.TotalImpuestos;

            await _db.SaveChangesAsync(ct);
            await trx.CommitAsync(ct);

            return v.IdVenta;
        }

        public async Task<VentaConDetalleDto?> ObtenerVentaAsync(int idVenta, CancellationToken ct = default)
        {
            var v = await _db.Ventas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdVenta == idVenta, ct);
            if (v is null) return null;

            var dets = await _db.DetalleVentas.AsNoTracking()
                .Where(d => d.IdVenta == idVenta)
                .Join(_db.Productos.AsNoTracking(),
                      d => d.IdProducto,
                      p => p.IdProducto,
                      (d, p) => new { d, p })
                .Select(x => new VentaItemDto(
                    x.d.IdDetalle,
                    x.d.IdProducto,
                    x.d.Cantidad,
                    x.d.PrecioUnitario,
                    Math.Round(x.d.Cantidad * x.d.PrecioUnitario, 2),
                    x.p.Nombre
                ))
                .ToListAsync(ct);

            var resumen = new VentaResumenDto(
                v.IdVenta,
                v.Fecha,
                v.IdCliente,
                v.TotalBruto,
                v.TotalImpuestos,
                v.TotalNeto
            );

            return new VentaConDetalleDto(resumen, dets);
        }
    }
}
