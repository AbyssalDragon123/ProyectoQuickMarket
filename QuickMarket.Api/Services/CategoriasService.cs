using System.Linq; // Where/Select/Skip/Take
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Services
{
    public interface ICategoriasService
    {
        Task<CategoriaDto> CreateAsync(CreateCategoriaDto dto, CancellationToken ct = default);
        Task<CategoriaDto?> GetAsync(int id, CancellationToken ct = default);
        Task<PagedResult<CategoriaDto>> ListAsync(int page, int pageSize, string? q, CancellationToken ct = default);
        Task<CategoriaDto> UpdateAsync(int id, UpdateCategoriaDto dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }

    public class CategoriasService : ICategoriasService
    {
        private readonly QuickMarketContext _db;
        public CategoriasService(QuickMarketContext db) { _db = db; }

        private static string CI(string s) => s.Trim().ToUpperInvariant();
        private static string N(string s) => s.Trim();

        public async Task<CategoriaDto> CreateAsync(CreateCategoriaDto dto, CancellationToken ct = default)
        {
            var c = new Categoria
            {
                Nombre = N(dto.Nombre),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion!.Trim()
            };

            _db.Categorias.Add(c);
            await _db.SaveChangesAsync(ct);

            return new CategoriaDto(c.IdCategoria, c.Nombre, c.Descripcion, c.CreadoEn, c.ActualizadoEn);
        }

        public async Task<CategoriaDto?> GetAsync(int id, CancellationToken ct = default)
        {
            var c = await _db.Categorias.FirstOrDefaultAsync(x => x.IdCategoria == id, ct);
            return c is null ? null : new CategoriaDto(c.IdCategoria, c.Nombre, c.Descripcion, c.CreadoEn, c.ActualizadoEn);
        }

        public async Task<PagedResult<CategoriaDto>> ListAsync(int page, int pageSize, string? q, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Categorias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var cq = CI(q);
                query = query.Where(x =>
                    x.Nombre.ToUpper().Contains(cq) ||
                    (x.Descripcion ?? string.Empty).ToUpper().Contains(cq)
                );
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoriaDto(
                    c.IdCategoria, c.Nombre, c.Descripcion, c.CreadoEn, c.ActualizadoEn
                ))
                .ToListAsync(ct);

            return new PagedResult<CategoriaDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<CategoriaDto> UpdateAsync(int id, UpdateCategoriaDto dto, CancellationToken ct = default)
        {
            var c = await _db.Categorias.FirstOrDefaultAsync(x => x.IdCategoria == id, ct)
                ?? throw new KeyNotFoundException("Categoría no encontrada.");

            if (!string.IsNullOrWhiteSpace(dto.Nombre)) c.Nombre = N(dto.Nombre!);
            if (dto.Descripcion is not null) c.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();

            await _db.SaveChangesAsync(ct);

            return new CategoriaDto(c.IdCategoria, c.Nombre, c.Descripcion, c.CreadoEn, c.ActualizadoEn);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var c = await _db.Categorias.FirstOrDefaultAsync(x => x.IdCategoria == id, ct)
                ?? throw new KeyNotFoundException("Categoría no encontrada.");

            _db.Categorias.Remove(c);
            await _db.SaveChangesAsync(ct);
        }
    }
}
