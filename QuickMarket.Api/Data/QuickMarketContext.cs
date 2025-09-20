// Data/QuickMarketContext.cs
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Models;
using System.Threading;

namespace QuickMarket.Api.Data
{
    public class QuickMarketContext : DbContext
    {
        public QuickMarketContext(DbContextOptions<QuickMarketContext> options) : base(options) { }

        public DbSet<USUARIOS> USUARIOS { get; set; }
        public DbSet<CLIENTES> CLIENTES { get; set; }
        public DbSet<CATEGORIAS> CATEGORIAS { get; set; }
        public DbSet<PRODUCTOS> PRODUCTOS { get; set; }
        public DbSet<VENTAS> VENTAS { get; set; }
        public DbSet<DETALLE_VENTAS> DETALLE_VENTAS { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<USUARIOS>().ToTable("USUARIOS");
            modelBuilder.Entity<CLIENTES>().ToTable("CLIENTES");
            modelBuilder.Entity<CATEGORIAS>().ToTable("CATEGORIAS");
            modelBuilder.Entity<PRODUCTOS>().ToTable("PRODUCTOS");
            modelBuilder.Entity<VENTAS>().ToTable("VENTAS");
            modelBuilder.Entity<DETALLE_VENTAS>().ToTable("DETALLE_VENTAS");

            // ===== USUARIOS =====
            modelBuilder.Entity<USUARIOS>(e =>
            {
                e.HasKey(x => x.ID_USUARIO);
                e.Property(x => x.USERNAME).HasColumnType("VARCHAR2(80)").IsRequired();
                e.Property(x => x.EMAIL).HasColumnType("VARCHAR2(150)").IsRequired();
                e.Property(x => x.PASSWORD_HASH).HasColumnType("VARCHAR2(255)").IsRequired();
                e.Property(x => x.ROL).HasColumnType("VARCHAR2(20)").HasDefaultValue("cliente");
                e.Property(x => x.ESTADO).HasColumnType("VARCHAR2(20)").HasDefaultValue("activo");
                e.Property(x => x.RESET_TOKEN).HasColumnType("VARCHAR2(255)");
                e.Property(x => x.RESET_EXPIRA).HasColumnType("TIMESTAMP");
                e.Property(x => x.CREADO_EN).HasColumnType("TIMESTAMP").HasDefaultValueSql("SYSTIMESTAMP");
                e.Property(x => x.ACTUALIZADO_EN).HasColumnType("TIMESTAMP");
            });

            // ===== CATEGORIAS =====
            modelBuilder.Entity<CATEGORIAS>(e =>
            {
                e.HasKey(x => x.ID_CATEGORIA);

                e.Property(x => x.NOMBRE).HasColumnType("VARCHAR2(120)").IsRequired();
                e.Property(x => x.DESCRIPCION).HasColumnType("VARCHAR2(500)");

                e.Property(x => x.CREADO_EN).HasColumnType("TIMESTAMP").HasDefaultValueSql("SYSTIMESTAMP");
                e.Property(x => x.ACTUALIZADO_EN).HasColumnType("TIMESTAMP");

                // Índice único case-insensitive UPPER(NOMBRE) se crea por SQL (Oracle) fuera de EF.
            });

            // ===== PRODUCTOS =====
            modelBuilder.Entity<PRODUCTOS>(e =>
            {
                e.HasKey(x => x.ID_PRODUCTO);

                e.Property(x => x.NOMBRE).HasColumnType("VARCHAR2(150)").IsRequired();
                e.Property(x => x.DESCRIPCION).HasColumnType("VARCHAR2(1000)").IsRequired();

                // Precio/base y stock
                e.Property(x => x.PRECIO_UNITARIO).HasColumnType("NUMBER(18,2)").IsRequired();
                e.Property(x => x.STOCK).HasColumnType("NUMBER(18,3)").HasDefaultValue(0);

                // FK categoría (opcional)
                e.Property(x => x.ID_CATEGORIA).HasColumnType("NUMBER");

                // Calculados por la BD (triggers/paquete) -> EF no los setea
                e.Property(x => x.IVA_UNITARIO)
                 .HasColumnType("NUMBER(18,2)")
                 .ValueGeneratedOnAddOrUpdate();

                e.Property(x => x.PRECIO_CON_IVA)
                 .HasColumnType("NUMBER(18,2)")
                 .ValueGeneratedOnAddOrUpdate();

                e.Property(x => x.CREADO_EN).HasColumnType("TIMESTAMP").HasDefaultValueSql("SYSTIMESTAMP");
                e.Property(x => x.ACTUALIZADO_EN).HasColumnType("TIMESTAMP");

                e.HasOne<CATEGORIAS>()
                 .WithMany()
                 .HasForeignKey(p => p.ID_CATEGORIA)
                 .OnDelete(DeleteBehavior.SetNull);

                // Sugerencia: índice único case-insensitive por (ID_CATEGORIA, UPPER(NOMBRE)) crear vía SQL.
                // e.HasIndex(p => new { p.ID_CATEGORIA, p.NOMBRE }).IsUnique().HasDatabaseName("UQ_PROD_CAT_NOMBRE"); // (no CI)
            });

            // ===== CLIENTES (1:1 con USUARIOS) =====
            modelBuilder.Entity<CLIENTES>(e =>
            {
                e.HasKey(x => x.ID_CLIENTE);
                e.Property(x => x.ID_USUARIO).HasColumnType("NUMBER");
                e.Property(x => x.NOMBRE).HasColumnType("VARCHAR2(150)").IsRequired();
                e.Property(x => x.EMAIL).HasColumnType("VARCHAR2(150)");
                e.Property(x => x.TELEFONO).HasColumnType("VARCHAR2(30)");
                e.Property(x => x.DIRECCION).HasColumnType("VARCHAR2(200)");
                e.Property(x => x.DEPARTAMENTO).HasColumnType("VARCHAR2(80)");
                e.Property(x => x.MUNICIPIO).HasColumnType("VARCHAR2(80)");
                e.Property(x => x.REFERENCIA).HasColumnType("VARCHAR2(200)");
                e.Property(x => x.CREADO_EN).HasColumnType("TIMESTAMP").HasDefaultValueSql("SYSTIMESTAMP");
                e.Property(x => x.ACTUALIZADO_EN).HasColumnType("TIMESTAMP");

                e.HasIndex(x => x.ID_USUARIO).IsUnique().HasDatabaseName("UQ_CLIENTES_USUARIO");

                e.HasOne<USUARIOS>()
                 .WithOne()
                 .HasForeignKey<CLIENTES>(c => c.ID_USUARIO)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== VENTAS =====
            modelBuilder.Entity<VENTAS>(e =>
            {
                e.HasKey(x => x.ID_VENTA);
                e.Property(x => x.FECHA).HasColumnType("TIMESTAMP").IsRequired();
                e.Property(x => x.ID_CLIENTE).HasColumnType("NUMBER");

                e.Property(x => x.TOTAL_BRUTO).HasColumnType("NUMBER(18,2)").HasDefaultValue(0);
                e.Property(x => x.TOTAL_IMPUESTOS).HasColumnType("NUMBER(18,2)").HasDefaultValue(0);
                e.Property(x => x.TOTAL_NETO).HasColumnType("NUMBER(18,2)").HasDefaultValue(0);

                e.Property(x => x.CREADO_EN).HasColumnType("TIMESTAMP").HasDefaultValueSql("SYSTIMESTAMP");
                e.Property(x => x.ACTUALIZADO_EN).HasColumnType("TIMESTAMP");

                e.HasOne<CLIENTES>()
                 .WithMany()
                 .HasForeignKey(v => v.ID_CLIENTE)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== DETALLE_VENTAS =====
            modelBuilder.Entity<DETALLE_VENTAS>(e =>
            {
                e.HasKey(x => x.ID_DETALLE);

                e.Property(x => x.ID_VENTA).HasColumnType("NUMBER").IsRequired();
                e.Property(x => x.ID_PRODUCTO).HasColumnType("NUMBER").IsRequired();

                e.Property(x => x.CANTIDAD).HasColumnType("NUMBER(18,3)").IsRequired();
                e.Property(x => x.PRECIO_UNITARIO).HasColumnType("NUMBER(18,2)").IsRequired();

                // SUBTOTAL lo calcula la BD
                e.Property(x => x.SUBTOTAL)
                 .HasColumnType("NUMBER(18,2)")
                 .ValueGeneratedOnAddOrUpdate();

                e.Property(x => x.CREADO_EN).HasColumnType("TIMESTAMP").HasDefaultValueSql("SYSTIMESTAMP");
                e.Property(x => x.ACTUALIZADO_EN).HasColumnType("TIMESTAMP");

                e.HasOne<VENTAS>()
                 .WithMany()
                 .HasForeignKey(d => d.ID_VENTA)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne<PRODUCTOS>()
                 .WithMany()
                 .HasForeignKey(d => d.ID_PRODUCTO);

                e.HasIndex(d => new { d.ID_VENTA, d.ID_PRODUCTO }).IsUnique();
            });
        }

        // Normaliza cadenas clave antes de guardar
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizeStrings();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            NormalizeStrings();
            return base.SaveChanges();
        }

        private void NormalizeStrings()
        {
            foreach (var e in ChangeTracker.Entries<USUARIOS>())
            {
                if (e.State is EntityState.Added or EntityState.Modified)
                {
                    if (!string.IsNullOrWhiteSpace(e.Entity.USERNAME))
                        e.Entity.USERNAME = e.Entity.USERNAME.Trim().ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(e.Entity.EMAIL))
                        e.Entity.EMAIL = e.Entity.EMAIL.Trim().ToUpperInvariant();
                }
            }

            foreach (var e in ChangeTracker.Entries<CATEGORIAS>())
            {
                if (e.State is EntityState.Added or EntityState.Modified)
                {
                    if (!string.IsNullOrWhiteSpace(e.Entity.NOMBRE))
                        e.Entity.NOMBRE = e.Entity.NOMBRE.Trim().ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(e.Entity.DESCRIPCION))
                        e.Entity.DESCRIPCION = e.Entity.DESCRIPCION.Trim();
                }
            }

            foreach (var e in ChangeTracker.Entries<PRODUCTOS>())
            {
                if (e.State is EntityState.Added or EntityState.Modified)
                {
                    if (!string.IsNullOrWhiteSpace(e.Entity.NOMBRE))
                        e.Entity.NOMBRE = e.Entity.NOMBRE.Trim().ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(e.Entity.DESCRIPCION))
                        e.Entity.DESCRIPCION = e.Entity.DESCRIPCION.Trim();
                }
            }
        }
    }
}
