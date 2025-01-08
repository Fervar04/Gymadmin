using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using APIGym.Models;
using Microsoft.AspNetCore.Identity;

namespace APIGym.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<Gimnasio> Gimnasios { get; set; }
        public DbSet<Suscripcion> Suscripciones { get; set; }
        public DbSet<HistorialSuscripcion> HistorialSuscripciones { get; set; }
        public DbSet<ClienteSuscripcion> ClienteSuscripciones { get; set; }
        public DbSet<HistorialPagosCliente> HistorialPagosClientes { get; set; }
        public DbSet<HikvisionDevice> HikvisionDevices { get; set; }
        public DbSet<ClienteGimnasio> ClienteGimnasios { get; set; }
        public DbSet<CodigoAsignacion> CodigosAsignacion { get; set; }
        public DbSet<AsingGym> AsingGyms { get; set; }
        public DbSet<GymConfig> GymConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Suscripcion y relación con Gimnasio
            modelBuilder.Entity<Suscripcion>()
                .HasOne(s => s.Gimnasio)
                .WithMany(g => g.Suscripciones)
                .HasForeignKey(s => s.IdGimnasio)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Evita conflictos de eliminación

            modelBuilder.Entity<Suscripcion>()
                .Property(s => s.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Suscripcion>()
                .Property(s => s.MontoDinamico)
                .HasPrecision(18, 2);

            // Configuración para ClienteSuscripcion y sus relaciones
            modelBuilder.Entity<ClienteSuscripcion>()
                .HasOne(cs => cs.Suscripcion)
                .WithMany() 
                .HasForeignKey(cs => cs.IdPago)
                .OnDelete(DeleteBehavior.Restrict); // Evita conflictos al eliminar suscripciones

            modelBuilder.Entity<ClienteSuscripcion>()
                .HasOne(cs => cs.Gimnasio)
                .WithMany(g => g.ClienteSuscripciones)
                .HasForeignKey(cs => cs.IdGimnasio)
                .OnDelete(DeleteBehavior.Restrict); // Evita conflictos al eliminar gimnasios

            modelBuilder.Entity<ClienteSuscripcion>()
                .HasOne(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Evita conflictos al eliminar usuarios

            // Configuración de HistorialPagosCliente
            modelBuilder.Entity<HistorialPagosCliente>()
                .Property(h => h.MontoPago)
                .HasPrecision(18, 2);

            // Configuración de precisión para GymConfig
            modelBuilder.Entity<GymConfig>().Property(g => g.ValorAnual).HasPrecision(18, 2);
            modelBuilder.Entity<GymConfig>().Property(g => g.ValorDiario).HasPrecision(18, 2);
            modelBuilder.Entity<GymConfig>().Property(g => g.ValorMensual).HasPrecision(18, 2);
            modelBuilder.Entity<GymConfig>().Property(g => g.ValorSemanal).HasPrecision(18, 2);
        }
    }
}