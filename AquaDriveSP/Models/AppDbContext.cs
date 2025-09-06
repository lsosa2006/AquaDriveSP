using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace AquaDriveSP.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=PostgresConnection") { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Sede> Sedes { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<TipoServicio> TiposServicio { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Horario> Horarios { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Resena> Resenas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relaciones Usuario -> Cliente / Empleado / Administrador
            modelBuilder.Entity<Cliente>().HasRequired(c => c.Usuario).WithOptional(u => u.Cliente).WillCascadeOnDelete(true);
            modelBuilder.Entity<Empleado>().HasRequired(e => e.Usuario).WithOptional(u => u.Empleado).WillCascadeOnDelete(true);
            modelBuilder.Entity<Administrador>().HasRequired(a => a.Usuario).WithOptional(u => u.Administrador).WillCascadeOnDelete(true);

            // Empleado -> Sede
            modelBuilder.Entity<Empleado>().HasRequired(e => e.Sede).WithMany(s => s.Empleados).HasForeignKey(e => e.SedeId);

            // Cita -> Cliente / Empleado / TipoServicio / Sede
            modelBuilder.Entity<Cita>().HasRequired(c => c.Cliente).WithMany(c => c.Citas).HasForeignKey(c => c.ClienteId).WillCascadeOnDelete(true);
            modelBuilder.Entity<Cita>().HasOptional(c => c.Empleado).WithMany(e => e.Citas).HasForeignKey(c => c.EmpleadoId);
            modelBuilder.Entity<Cita>().HasRequired(c => c.TipoServicio).WithMany(t => t.Citas).HasForeignKey(c => c.TipoServicioId);
            modelBuilder.Entity<Cita>().HasRequired(c => c.Sede).WithMany(s => s.Citas).HasForeignKey(c => c.SedeId);

            // Resena -> Cita / Empleado / Cliente
            modelBuilder.Entity<Resena>().HasRequired(r => r.Cita).WithOptional(c => c.Resena).WillCascadeOnDelete(true);
            modelBuilder.Entity<Resena>().HasRequired(r => r.Empleado).WithMany(e => e.Resenas).HasForeignKey(r => r.EmpleadoId);
            modelBuilder.Entity<Resena>().HasRequired(r => r.Cliente).WithMany(c => c.Resenas).HasForeignKey(r => r.ClienteId);

            // Vehiculo -> Cliente
            modelBuilder.Entity<Vehiculo>().HasRequired(v => v.Cliente).WithMany(c => c.Vehiculos).HasForeignKey(v => v.ClienteId);

            // Horario -> Empleado
            modelBuilder.Entity<Horario>().HasRequired(h => h.Empleado).WithMany(e => e.Horarios).HasForeignKey(h => h.EmpleadoId);
        }
    }
}