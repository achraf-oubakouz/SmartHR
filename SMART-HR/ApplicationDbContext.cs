using Microsoft.EntityFrameworkCore;
using SmartHR.Models;

namespace SmartHR
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 🔹 DbSets
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Employe> Employes { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<RessourceHumaine> RessourcesHumaines { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<TypeConge> TypesConges { get; set; }
        public DbSet<DemandeConge> DemandesConges { get; set; }
        public DbSet<Rapport> Rapports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1-1: Utilisateur <-> Admin
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Utilisateur)
                .WithOne(u => u.Administrateur)
                .HasForeignKey<Admin>(a => a.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-1: Utilisateur <-> Manager
            modelBuilder.Entity<Manager>()
                 .HasMany(m => m.Rapports)
                 .WithOne(r => r.Manager)
                 .HasForeignKey(r => r.ManagerId)
                 .OnDelete(DeleteBehavior.SetNull);


            // 1-1: Utilisateur <-> Employe
            modelBuilder.Entity<Employe>()
                .HasOne(e => e.Utilisateur)
                .WithOne(u => u.Employe)
                .HasForeignKey<Employe>(e => e.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-1: Utilisateur <-> RessourceHumaine
            modelBuilder.Entity<RessourceHumaine>()
                .HasOne(rh => rh.Utilisateur)
                .WithOne(u => u.RH)
                .HasForeignKey<RessourceHumaine>(rh => rh.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-N: Manager -> Employes
            modelBuilder.Entity<Manager>()
                .HasMany(m => m.Employes)
                .WithOne(e => e.Manager)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1-N: Manager -> Rapports
            modelBuilder.Entity<Manager>()
                .HasMany(m => m.Rapports)
                .WithOne(r => r.Manager)
                .HasForeignKey(r => r.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1-N: Employe -> DemandesConges
            modelBuilder.Entity<Employe>()
                .HasMany(e => e.DemandesConges)
                .WithOne(dc => dc.Employe)
                .HasForeignKey(dc => dc.EmployeId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-N: TypeConge -> DemandesConges
            modelBuilder.Entity<TypeConge>()
                .HasMany(tc => tc.DemandesConges)
                .WithOne(dc => dc.TypeConge)
                .HasForeignKey(dc => dc.TypeCongeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Pointage entity removed
        }
    }
}
