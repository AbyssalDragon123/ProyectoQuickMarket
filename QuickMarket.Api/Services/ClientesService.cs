using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Services
{
    // Helper seguro para Oracle: evita AnyAsync => usa COUNT(*) > 0
    internal static class OracleSafeLinq
    {
        public static async Task<bool> ExistsAsync<T>(
            this IQueryable<T> query, CancellationToken ct = default)
            => (await query.Select(_ => 1).CountAsync(ct)) > 0;
    }

    public interface IClientesService
    {
        Task<ClienteDto> CreateAsync(CreateClienteDto dto, CancellationToken ct = default);
        Task<ClienteDto?> GetAsync(int idCliente, CancellationToken ct = default);
        Task<ClienteDto?> GetByUsuarioAsync(int idUsuario, CancellationToken ct = default);
        Task<PagedResult<ClienteDto>> ListAsync(int page, int pageSize, string? q, CancellationToken ct = default);
        Task<ClienteDto> UpdateAsync(int idCliente, UpdateClienteDto dto, CancellationToken ct = default);
        Task DeleteAsync(int idCliente, CancellationToken ct = default);
    }

    public class ClientesService : IClientesService
    {
        private readonly QuickMarketContext _db;
        public ClientesService(QuickMarketContext db) { _db = db; }

        private static string CI(string s) => s.Trim().ToUpperInvariant();
        private static string N(string? s) => s?.Trim() ?? string.Empty;

        public async Task<ClienteDto> CreateAsync(CreateClienteDto dto, CancellationToken ct = default)
        {
            if (dto.IdUsuario is int uid)
            {
                // Existe usuario?
                var userExists = await _db.Usuarios
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(u => u.IdUsuario == uid)
                    .ExistsAsync(ct);

                if (!userExists)
                    throw new InvalidOperationException("El usuario asociado no existe.");

                // Ya tiene perfil?
                var yaTienePerfil = await _db.Clientes
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(c => c.IdUsuario == uid)
                    .ExistsAsync(ct);

                if (yaTienePerfil)
                    throw new InvalidOperationException("Ese usuario ya tiene un perfil de cliente.");
            }

            var c = new Cliente
            {
                IdUsuario = dto.IdUsuario,
                Nombre = N(dto.Nombre),
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono!.Trim(),
                Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion!.Trim(),
                Departamento = string.IsNullOrWhiteSpace(dto.Departamento) ? null : dto.Departamento!.Trim(),
                Municipio = string.IsNullOrWhiteSpace(dto.Municipio) ? null : dto.Municipio!.Trim(),
                Referencia = string.IsNullOrWhiteSpace(dto.Referencia) ? null : dto.Referencia!.Trim()
            };

            _db.Clientes.Add(c);
            await _db.SaveChangesAsync(ct);

            return new ClienteDto(
                c.IdCliente, c.IdUsuario, c.Nombre, c.Telefono, c.Direccion, c.Departamento,
                c.Municipio, c.Referencia, c.CreadoEn, c.ActualizadoEn
            );
        }

        public async Task<ClienteDto?> GetAsync(int idCliente, CancellationToken ct = default)
        {
            var c = await _db.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdCliente == idCliente, ct);

            return c is null ? null : new ClienteDto(
                c.IdCliente, c.IdUsuario, c.Nombre, c.Telefono, c.Direccion, c.Departamento,
                c.Municipio, c.Referencia, c.CreadoEn, c.ActualizadoEn
            );
        }

        public async Task<ClienteDto?> GetByUsuarioAsync(int idUsuario, CancellationToken ct = default)
        {
            var c = await _db.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdUsuario == idUsuario, ct);

            return c is null ? null : new ClienteDto(
                c.IdCliente, c.IdUsuario, c.Nombre, c.Telefono, c.Direccion, c.Departamento,
                c.Municipio, c.Referencia, c.CreadoEn, c.ActualizadoEn
            );
        }

        public async Task<PagedResult<ClienteDto>> ListAsync(int page, int pageSize, string? q, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Clientes.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var cq = CI(q);
                query = query.Where(x =>
                    x.Nombre.ToUpper().Contains(cq) ||
                    (x.Telefono ?? string.Empty).ToUpper().Contains(cq) ||
                    (x.Direccion ?? string.Empty).ToUpper().Contains(cq) ||
                    (x.Departamento ?? string.Empty).ToUpper().Contains(cq) ||
                    (x.Municipio ?? string.Empty).ToUpper().Contains(cq) ||
                    (x.Referencia ?? string.Empty).ToUpper().Contains(cq)
                );
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.IdCliente)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClienteDto(
                    c.IdCliente, c.IdUsuario, c.Nombre, c.Telefono, c.Direccion, c.Departamento,
                    c.Municipio, c.Referencia, c.CreadoEn, c.ActualizadoEn
                ))
                .ToListAsync(ct);

            return new PagedResult<ClienteDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<ClienteDto> UpdateAsync(int idCliente, UpdateClienteDto dto, CancellationToken ct = default)
        {
            var c = await _db.Clientes.FirstOrDefaultAsync(x => x.IdCliente == idCliente, ct)
                ?? throw new KeyNotFoundException("Cliente no encontrado.");

            if (dto.IdUsuario.HasValue)
            {
                var uid = dto.IdUsuario.Value;

                if (uid == 0)
                {
                    c.IdUsuario = null;
                }
                else
                {
                    // Existe usuario?
                    var userExists = await _db.Usuarios
                        .AsNoTracking()
                        .IgnoreQueryFilters()
                        .Where(u => u.IdUsuario == uid)
                        .ExistsAsync(ct);

                    if (!userExists)
                        throw new InvalidOperationException("El usuario asociado no existe.");

                    // Ya está vinculado a otro cliente?
                    var yaTienePerfil = await _db.Clientes
                        .AsNoTracking()
                        .IgnoreQueryFilters()
                        .Where(x => x.IdUsuario == uid && x.IdCliente != idCliente)
                        .ExistsAsync(ct);

                    if (yaTienePerfil)
                        throw new InvalidOperationException("Ese usuario ya tiene un perfil de cliente.");

                    c.IdUsuario = uid;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Nombre)) c.Nombre = dto.Nombre!.Trim();
            if (dto.Telefono is not null) c.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            if (dto.Direccion is not null) c.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
            if (dto.Departamento is not null) c.Departamento = string.IsNullOrWhiteSpace(dto.Departamento) ? null : dto.Departamento.Trim();
            if (dto.Municipio is not null) c.Municipio = string.IsNullOrWhiteSpace(dto.Municipio) ? null : dto.Municipio.Trim();
            if (dto.Referencia is not null) c.Referencia = string.IsNullOrWhiteSpace(dto.Referencia) ? null : dto.Referencia.Trim();

            await _db.SaveChangesAsync(ct);

            return new ClienteDto(
                c.IdCliente, c.IdUsuario, c.Nombre, c.Telefono, c.Direccion, c.Departamento,
                c.Municipio, c.Referencia, c.CreadoEn, c.ActualizadoEn
            );
        }

        public async Task DeleteAsync(int idCliente, CancellationToken ct = default)
        {
            var c = await _db.Clientes.FirstOrDefaultAsync(x => x.IdCliente == idCliente, ct)
                ?? throw new KeyNotFoundException("Cliente no encontrado.");

            _db.Clientes.Remove(c);
            await _db.SaveChangesAsync(ct);
        }
    }
}
