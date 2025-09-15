using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Data
{
    public class QuickMarketContext : DbContext
    {
        public QuickMarketContext(DbContextOptions<QuickMarketContext> options) : base(options) { }

        public DbSet<EMPLEADOS> EMPLEADOS { get; set; }
        public DbSet<CLIENTES> CLIENTES { get; set; }
        public DbSet<CATEGORIAS> CATEGORIAS { get; set; }
        public DbSet<PRODUCTOS> PRODUCTOS { get; set; }
        public DbSet<VENTAS> VENTAS { get; set; }
        public DbSet<DETALLE_VENTAS> DETALLE_VENTAS { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Nombres de tabla = tal cual
            modelBuilder.Entity<EMPLEADOS>().ToTable("EMPLEADOS");
            modelBuilder.Entity<CLIENTES>().ToTable("CLIENTES");
            modelBuilder.Entity<CATEGORIAS>().ToTable("CATEGORIAS");
            modelBuilder.Entity<PRODUCTOS>().ToTable("PRODUCTOS");
            modelBuilder.Entity<VENTAS>().ToTable("VENTAS");
            modelBuilder.Entity<DETALLE_VENTAS>().ToTable("DETALLE_VENTAS");

            // PKs (Identity se respeta automáticamente)
            modelBuilder.Entity<EMPLEADOS>().HasKey(x => x.ID_EMPLEADO);
            modelBuilder.Entity<CLIENTES>().HasKey(x => x.ID_CLIENTE);
            modelBuilder.Entity<CATEGORIAS>().HasKey(x => x.ID_CATEGORIA);
            modelBuilder.Entity<PRODUCTOS>().HasKey(x => x.ID_PRODUCTO);
            modelBuilder.Entity<VENTAS>().HasKey(x => x.ID_VENTA);
            modelBuilder.Entity<DETALLE_VENTAS>().HasKey(x => x.ID_DETALLE);

            // Relaciones (FK)
            modelBuilder.Entity<CLIENTES>()
                .HasOne<EMPLEADOS>()
                .WithMany()
                .HasForeignKey(c => c.ID_EMPLEADO)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PRODUCTOS>()
                .HasOne<CATEGORIAS>()
                .WithMany()
                .HasForeignKey(p => p.ID_CATEGORIA)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VENTAS>()
                .HasOne<CLIENTES>()
                .WithMany()
                .HasForeignKey(v => v.ID_CLIENTE)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VENTAS>()
                .HasOne<EMPLEADOS>()
                .WithMany()
                .HasForeignKey(v => v.ID_EMPLEADO)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DETALLE_VENTAS>()
                .HasOne<VENTAS>()
                .WithMany()
                .HasForeignKey(d => d.ID_VENTA)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DETALLE_VENTAS>()
                .HasOne<PRODUCTOS>()
                .WithMany()
                .HasForeignKey(d => d.ID_PRODUCTO);

            // Único compuesto en detalle (ID_VENTA, ID_PRODUCTO)
            modelBuilder.Entity<DETALLE_VENTAS>()
                .HasIndex(d => new { d.ID_VENTA, d.ID_PRODUCTO })
                .IsUnique();

            // Columnas computadas (no escribir desde EF)
            modelBuilder.Entity<PRODUCTOS>()
                .Property(p => p.IVA_UNITARIO)
                .HasComputedColumnSql(null, stored: false);

            modelBuilder.Entity<PRODUCTOS>()
                .Property(p => p.PRECIO_CON_IVA)
                .HasComputedColumnSql(null, stored: false);

            modelBuilder.Entity<DETALLE_VENTAS>()
                .Property(d => d.SUBTOTAL)
                .HasComputedColumnSql(null, stored: false);
        }
    }
}
