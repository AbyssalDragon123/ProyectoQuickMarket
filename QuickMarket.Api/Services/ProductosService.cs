// QuickMarket.Api/Services/ProductosService.cs
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Services
{
    public interface IProductosService
    {
        Task<ProductoDto> CreateAsync(CreateProductoDto dto, CancellationToken ct = default);
        Task<ProductoDto?> GetAsync(int id, CancellationToken ct = default);
        Task<PagedResult<ProductoDto>> ListAsync(int page, int pageSize, string? q, int? idCategoria, CancellationToken ct = default);
        Task<ProductoDto> UpdateAsync(int id, UpdateProductoDto dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }

    public class ProductosService : IProductosService
    {
        private readonly QuickMarketContext _db;
        public ProductosService(QuickMarketContext db) { _db = db; }

        private static string CI(string s) => s.Trim().ToUpperInvariant();
        private static string N(string s) => s.Trim();

        public async Task<ProductoDto> CreateAsync(CreateProductoDto dto, CancellationToken ct = default)
        {
            // Validaciones numéricas
            if (dto.PrecioUnitario < 0) throw new InvalidOperationException("El precio no puede ser negativo.");
            if (dto.Stock < 0) throw new InvalidOperationException("El stock no puede ser negativo.");

            // Validación de categoría (si viene y no es 0)
            if (dto.IdCategoria.HasValue && dto.IdCategoria.Value != 0)
            {
                var catOk = (await _db.Categorias
                    .AsNoTracking()
                    .Where(c => c.IdCategoria == dto.IdCategoria.Value)
                    .Select(_ => 1)
                    .CountAsync(ct)) > 0;

                if (!catOk) throw new InvalidOperationException("La categoría especificada no existe.");
            }

            // (Opcional) Evitar duplicado de nombre (case-insensitive)
            var nombre = N(dto.Nombre);
            var duplicado = (await _db.Productos
                .AsNoTracking()
                .Where(p => p.Nombre.ToUpper() == nombre.ToUpper())
                .Select(_ => 1)
                .CountAsync(ct)) > 0;

            if (duplicado) throw new InvalidOperationException("Ya existe un producto con ese nombre.");

            var p = new Producto
            {
                Nombre = nombre,
                Descripcion = N(dto.Descripcion),
                PrecioUnitario = dto.PrecioUnitario,
                Stock = dto.Stock,
                IdCategoria = dto.IdCategoria == 0 ? null : dto.IdCategoria
            };

            _db.Productos.Add(p);
            await _db.SaveChangesAsync(ct);

            return new ProductoDto(
                p.IdProducto, p.Nombre, p.Descripcion, p.PrecioUnitario, p.Stock,
                p.IdCategoria, p.IvaUnitario, p.PrecioConIva, p.CreadoEn, p.ActualizadoEn
            );
        }

        public async Task<ProductoDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var p = await _db.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdProducto == id, ct);

            return p is null ? null : new ProductoDto(
                p.IdProducto, p.Nombre, p.Descripcion, p.PrecioUnitario, p.Stock,
                p.IdCategoria, p.IvaUnitario, p.PrecioConIva, p.CreadoEn, p.ActualizadoEn
            );
        }

        public async Task<PagedResult<ProductoDto>> ListAsync(int page, int pageSize, string? q, int? idCategoria, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Productos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var cq = CI(q);
                query = query.Where(x =>
                    x.Nombre.ToUpper().Contains(cq) ||
                    x.Descripcion.ToUpper().Contains(cq)
                );
            }

            if (idCategoria.HasValue)
            {
                if (idCategoria.Value == 0) query = query.Where(x => x.IdCategoria == null);
                else query = query.Where(x => x.IdCategoria == idCategoria.Value);
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductoDto(
                    p.IdProducto, p.Nombre, p.Descripcion, p.PrecioUnitario, p.Stock,
                    p.IdCategoria, p.IvaUnitario, p.PrecioConIva, p.CreadoEn, p.ActualizadoEn
                ))
                .ToListAsync(ct);

            return new PagedResult<ProductoDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<ProductoDto> UpdateAsync(int id, UpdateProductoDto dto, CancellationToken ct = default)
        {
            var p = await _db.Productos.FirstOrDefaultAsync(x => x.IdProducto == id, ct)
                ?? throw new InvalidOperationException("Producto no encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Nombre))
            {
                var nuevoNombre = N(dto.Nombre!);

                var duplicado = (await _db.Productos
                    .AsNoTracking()
                    .Where(x => x.IdProducto != id && x.Nombre.ToUpper() == nuevoNombre.ToUpper())
                    .Select(_ => 1)
                    .CountAsync(ct)) > 0;

                if (duplicado) throw new InvalidOperationException("Ya existe un producto con ese nombre.");

                p.Nombre = nuevoNombre;
            }

            if (!string.IsNullOrWhiteSpace(dto.Descripcion))
                p.Descripcion = N(dto.Descripcion!);

            if (dto.PrecioUnitario.HasValue)
            {
                if (dto.PrecioUnitario.Value < 0) throw new InvalidOperationException("El precio no puede ser negativo.");
                p.PrecioUnitario = dto.PrecioUnitario.Value;
            }

            if (dto.Stock.HasValue)
            {
                if (dto.Stock.Value < 0) throw new InvalidOperationException("El stock no puede ser negativo.");
                p.Stock = dto.Stock.Value;
            }

            if (dto.IdCategoria.HasValue)
            {
                if (dto.IdCategoria.Value == 0)
                {
                    p.IdCategoria = null;
                }
                else
                {
                    var catOk = (await _db.Categorias
                        .AsNoTracking()
                        .Where(c => c.IdCategoria == dto.IdCategoria.Value)
                        .Select(_ => 1)
                        .CountAsync(ct)) > 0;

                    if (!catOk) throw new InvalidOperationException("La categoría especificada no existe.");
                    p.IdCategoria = dto.IdCategoria.Value;
                }
            }

            await _db.SaveChangesAsync(ct);

            return new ProductoDto(
                p.IdProducto, p.Nombre, p.Descripcion, p.PrecioUnitario, p.Stock,
                p.IdCategoria, p.IvaUnitario, p.PrecioConIva, p.CreadoEn, p.ActualizadoEn
            );
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var p = await _db.Productos.FirstOrDefaultAsync(x => x.IdProducto == id, ct)
                ?? throw new InvalidOperationException("Producto no encontrado.");

            _db.Productos.Remove(p);
            await _db.SaveChangesAsync(ct);
        }
    }
}
