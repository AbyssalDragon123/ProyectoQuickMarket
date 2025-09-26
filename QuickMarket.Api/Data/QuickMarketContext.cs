using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Data
{
    public class QuickMarketContext : DbContext
    {
        public QuickMarketContext(DbContextOptions<QuickMarketContext> options) : base(options) { }

        // ===== DbSets =====
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // =========================
            // USUARIOS
            // =========================
            b.Entity<Usuario>(e =>
            {
                e.ToTable("USUARIOS");
                e.HasKey(x => x.IdUsuario).HasName("PK_USUARIOS");

                e.Property(x => x.IdUsuario)
                    .HasColumnName("ID_USUARIO")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SQ_USUARIOS.NEXTVAL"); // si usas trigger+secuencia

                e.Property(x => x.Username).HasColumnName("USERNAME").HasMaxLength(50).IsRequired();
                e.Property(x => x.Email).HasColumnName("EMAIL").HasMaxLength(150).IsRequired();
                e.Property(x => x.PasswordHash).HasColumnName("PASSWORD_HASH").HasMaxLength(255).IsRequired();
                e.Property(x => x.Rol).HasColumnName("ROL").HasMaxLength(20).HasDefaultValue("cliente").IsRequired();
                e.Property(x => x.Estado).HasColumnName("ESTADO").HasMaxLength(20).HasDefaultValue("activo").IsRequired();

                e.Property(x => x.ResetToken).HasColumnName("RESET_TOKEN").HasMaxLength(255);
                e.Property(x => x.ResetExpira).HasColumnName("RESET_EXPIRA");

                e.Property(x => x.CreadoEn).HasColumnName("CREADO_EN");
                e.Property(x => x.ActualizadoEn).HasColumnName("ACTUALIZADO_EN");

                e.HasIndex(x => x.Username).HasDatabaseName("IX_USUARIOS_USERNAME");
                e.HasIndex(x => x.Email).HasDatabaseName("IX_USUARIOS_EMAIL");
            });

            // =========================
            // CLIENTES (1:1 opcional con USUARIOS)
            // =========================
            b.Entity<Cliente>(e =>
            {
                e.ToTable("CLIENTES");
                e.HasKey(x => x.IdCliente).HasName("PK_CLIENTES");

                e.Property(x => x.IdCliente)
                    .HasColumnName("ID_CLIENTE")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SQ_CLIENTES.NEXTVAL");

                e.Property(x => x.IdUsuario).HasColumnName("ID_USUARIO");
                e.Property(x => x.Nombre).HasColumnName("NOMBRE").HasMaxLength(150).IsRequired();
                e.Property(x => x.Telefono).HasColumnName("TELEFONO").HasMaxLength(30);
                e.Property(x => x.Direccion).HasColumnName("DIRECCION").HasMaxLength(200);
                e.Property(x => x.Departamento).HasColumnName("DEPARTAMENTO").HasMaxLength(80);
                e.Property(x => x.Municipio).HasColumnName("MUNICIPIO").HasMaxLength(80);
                e.Property(x => x.Referencia).HasColumnName("REFERENCIA").HasMaxLength(200);
                e.Property(x => x.CreadoEn).HasColumnName("CREADO_EN");
                e.Property(x => x.ActualizadoEn).HasColumnName("ACTUALIZADO_EN");

                e.HasIndex(x => x.IdUsuario).IsUnique().HasDatabaseName("UQ_CLIENTES_ID_USUARIO");

                e.HasOne(x => x.Usuario)
                    .WithOne()
                    .HasForeignKey<Cliente>(x => x.IdUsuario)
                    .HasConstraintName("FK_CLIENTE_USUARIO")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // CATEGORIAS
            // =========================
            b.Entity<Categoria>(e =>
            {
                e.ToTable("CATEGORIAS");
                e.HasKey(x => x.IdCategoria).HasName("PK_CATEGORIAS");

                e.Property(x => x.IdCategoria)
                    .HasColumnName("ID_CATEGORIA")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SQ_CATEGORIAS.NEXTVAL");

                e.Property(x => x.Nombre).HasColumnName("NOMBRE").HasMaxLength(100).IsRequired();
                e.Property(x => x.Descripcion).HasColumnName("DESCRIPCION").HasMaxLength(255);
                e.Property(x => x.CreadoEn).HasColumnName("CREADO_EN");
                e.Property(x => x.ActualizadoEn).HasColumnName("ACTUALIZADO_EN");

                e.HasIndex(x => x.Nombre).HasDatabaseName("IX_CATEGORIAS_NOMBRE");
            });

            // =========================
            // PRODUCTOS
            // =========================
            b.Entity<Producto>(e =>
            {
                e.ToTable("PRODUCTOS");
                e.HasKey(x => x.IdProducto).HasName("PK_PRODUCTOS");

                e.Property(x => x.IdProducto)
                    .HasColumnName("ID_PRODUCTO")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SQ_PRODUCTOS.NEXTVAL");

                e.Property(x => x.Nombre).HasColumnName("NOMBRE").HasMaxLength(120).IsRequired();
                e.Property(x => x.Descripcion).HasColumnName("DESCRIPCION").HasMaxLength(120).IsRequired();

                e.Property(x => x.PrecioUnitario)
                    .HasColumnName("PRECIO_UNITARIO")
                    .HasPrecision(12, 2)
                    .IsRequired();

                e.Property(x => x.Stock)
                    .HasColumnName("STOCK")
                    .HasPrecision(12, 3)
                    .HasDefaultValue(0)
                    .IsRequired();

                e.Property(x => x.IdCategoria).HasColumnName("ID_CATEGORIA");

                e.Property(x => x.IvaUnitario)
                    .HasColumnName("IVA_UNITARIO")
                    .HasPrecision(12, 2)
                    .ValueGeneratedOnAddOrUpdate();

                e.Property(x => x.PrecioConIva)
                    .HasColumnName("PRECIO_CON_IVA")
                    .HasPrecision(12, 2)
                    .ValueGeneratedOnAddOrUpdate();

                e.Property(x => x.CreadoEn).HasColumnName("CREADO_EN");
                e.Property(x => x.ActualizadoEn).HasColumnName("ACTUALIZADO_EN");

                e.HasIndex(x => x.IdCategoria).HasDatabaseName("IX_PRODUCTOS_CATEGORIA");
                e.HasIndex(x => x.Nombre).HasDatabaseName("IX_PRODUCTOS_NOMBRE");

                e.HasOne(x => x.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(x => x.IdCategoria)
                    .HasConstraintName("FK_PRODUCTO_CATEGORIA")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // VENTAS
            // =========================
            b.Entity<Venta>(e =>
            {
                e.ToTable("VENTAS");
                e.HasKey(x => x.IdVenta).HasName("PK_VENTAS");

                e.Property(x => x.IdVenta)
                    .HasColumnName("ID_VENTA")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SQ_VENTAS.NEXTVAL");

                e.Property(x => x.Fecha).HasColumnName("FECHA"); // en DB: DATE default SYSDATE
                e.Property(x => x.IdCliente).HasColumnName("ID_CLIENTE");

                e.Property(x => x.TotalBruto).HasColumnName("TOTAL_BRUTO").HasPrecision(12, 2).HasDefaultValue(0).IsRequired();
                e.Property(x => x.TotalImpuestos).HasColumnName("TOTAL_IMPUESTOS").HasPrecision(12, 2).HasDefaultValue(0).IsRequired();
                e.Property(x => x.TotalNeto).HasColumnName("TOTAL_NETO").HasPrecision(12, 2).HasDefaultValue(0).IsRequired();

                e.Property(x => x.CreadoEn).HasColumnName("CREADO_EN");
                e.Property(x => x.ActualizadoEn).HasColumnName("ACTUALIZADO_EN");

                e.HasIndex(x => x.IdCliente).HasDatabaseName("IX_VENTAS_CLIENTE");

                e.HasOne(x => x.Cliente)
                    .WithMany()
                    .HasForeignKey(x => x.IdCliente)
                    .HasConstraintName("FK_VENTA_CLIENTE")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // DETALLE_VENTAS
            // =========================
            b.Entity<DetalleVenta>(e =>
            {
                e.ToTable("DETALLE_VENTAS");
                e.HasKey(x => x.IdDetalle).HasName("PK_DETALLE_VENTAS");

                e.Property(x => x.IdDetalle)
                    .HasColumnName("ID_DETALLE")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SQ_DETALLE_VENTAS.NEXTVAL");

                e.Property(x => x.IdVenta).HasColumnName("ID_VENTA").IsRequired();
                e.Property(x => x.IdProducto).HasColumnName("ID_PRODUCTO").IsRequired();

                e.Property(x => x.Cantidad)
                    .HasColumnName("CANTIDAD")
                    .HasPrecision(12, 3)
                    .IsRequired();

                e.Property(x => x.PrecioUnitario)
                    .HasColumnName("PRECIO_UNITARIO")
                    .HasPrecision(12, 2)
                    .IsRequired();

                e.Property(x => x.Subtotal)
                    .HasColumnName("SUBTOTAL")
                    .HasPrecision(12, 2)
                    .ValueGeneratedOnAddOrUpdate();

                e.Property(x => x.CreadoEn).HasColumnName("CREADO_EN");
                e.Property(x => x.ActualizadoEn).HasColumnName("ACTUALIZADO_EN");

                e.HasIndex(x => x.IdVenta).HasDatabaseName("IX_DETALLE_VENTAS_VENTA");
                e.HasIndex(x => x.IdProducto).HasDatabaseName("IX_DETALLE_VENTAS_PRODUCTO");
                e.HasIndex(x => new { x.IdVenta, x.IdProducto }).IsUnique().HasDatabaseName("UQ_DET_VENTA_PRODUCTO");

                e.HasOne(x => x.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(x => x.IdVenta)
                    .HasConstraintName("FK_DET_VENTA")
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Producto)
                    .WithMany()
                    .HasForeignKey(x => x.IdProducto)
                    .HasConstraintName("FK_DET_PRODUCTO")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================================
            // CONVERSORES GLOBALES (aplican a TODAS las entidades)
            // - bool/bool? -> NUMBER(1) (0/1) — evita ... = FALSE/TRUE
            // - DateTime/DateTime? -> fuerza Kind=Utc al leer/escribir
            // ==========================================================
            var boolToNumber = new ValueConverter<bool, int>(v => v ? 1 : 0, v => v == 1);
            var boolToNumberN = new ValueConverter<bool?, int?>(
                v => v.HasValue ? (v.Value ? 1 : 0) : (int?)null,
                v => v.HasValue ? v.Value == 1 : (bool?)null
            );
            var toUtc = new ValueConverter<DateTime, DateTime>(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );
            var toUtcN = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
            );

            foreach (var et in b.Model.GetEntityTypes())
                foreach (var p in et.GetProperties())
                {
                    if (p.ClrType == typeof(bool))
                    {
                        p.SetValueConverter(boolToNumber);
                        p.SetColumnType("NUMBER(1)");
                        if (p.GetDefaultValue() is null) p.SetDefaultValue(0);
                    }
                    else if (p.ClrType == typeof(bool?))
                    {
                        p.SetValueConverter(boolToNumberN);
                        p.SetColumnType("NUMBER(1)");
                    }
                    else if (p.ClrType == typeof(DateTime))
                    {
                        p.SetValueConverter(toUtc);
                    }
                    else if (p.ClrType == typeof(DateTime?))
                    {
                        p.SetValueConverter(toUtcN);
                    }
                }
        }
    }
}
