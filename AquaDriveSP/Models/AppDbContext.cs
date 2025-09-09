using System.Data.Entity;

namespace AquaDriveSP.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=PostgresConnection") { }

        public DbSet<Usuario> usuario { get; set; }
        public DbSet<Cliente> cliente { get; set; }
        public DbSet<Empleado> empleado { get; set; }
        public DbSet<Administrador> administrador { get; set; }
        public DbSet<Sede> sede { get; set; }
        public DbSet<TipoServicio> tiposervicio { get; set; }
        public DbSet<Vehiculo> vehiculo { get; set; }
        public DbSet<Horario> horario { get; set; }
        public DbSet<Cita> cita { get; set; }
        public DbSet<Resena> resena { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --------------------
            // Usuario -> Cliente / Empleado / Administrador
            // --------------------
            modelBuilder.Entity<Cliente>()
                .HasRequired(c => c.usuario)
                .WithMany(u => u.clientes)
                .HasForeignKey(c => c.usuarioid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Empleado>()
                .HasRequired(e => e.usuario)
                .WithMany(u => u.empleados)
                .HasForeignKey(e => e.usuarioid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Administrador>()
                .HasRequired(a => a.usuario)
                .WithMany(u => u.administradores)
                .HasForeignKey(a => a.usuarioid)
                .WillCascadeOnDelete(true);

            // --------------------
            // Empleado -> Sede
            // --------------------
            modelBuilder.Entity<Empleado>()
                .HasRequired(e => e.sede)
                .WithMany(s => s.empleados)
                .HasForeignKey(e => e.sedeid);

            // --------------------
            // Cita -> Cliente / Empleado / TipoServicio / Sede
            // --------------------
            modelBuilder.Entity<Cita>()
                .HasRequired(c => c.cliente)
                .WithMany(cl => cl.citas)
                .HasForeignKey(c => c.clienteid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Cita>()
                .HasOptional(c => c.empleado)
                .WithMany(e => e.citas)
                .HasForeignKey(c => c.empleadoid);

            modelBuilder.Entity<Cita>()
                .HasRequired(c => c.tiposervicio)
                .WithMany(t => t.citas)
                .HasForeignKey(c => c.tipoServicioid);

            modelBuilder.Entity<Cita>()
                .HasRequired(c => c.sede)
                .WithMany(s => s.citas)
                .HasForeignKey(c => c.sedeid);

            // --------------------
            // Resena -> Cita / Empleado / Cliente
            // --------------------
            modelBuilder.Entity<Resena>()
                .HasRequired(r => r.cita)
                .WithMany(c => c.resenas)
                .HasForeignKey(r => r.citaid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Resena>()
                .HasRequired(r => r.empleado)
                .WithMany(e => e.resenas)
                .HasForeignKey(r => r.empleadoid);

            modelBuilder.Entity<Resena>()
                .HasRequired(r => r.cliente)
                .WithMany(c => c.resenas)
                .HasForeignKey(r => r.clienteid);

            // --------------------
            // Vehiculo -> Cliente
            // --------------------
            modelBuilder.Entity<Vehiculo>()
                .HasRequired(v => v.cliente)
                .WithMany(c => c.vehiculos)
                .HasForeignKey(v => v.clienteid);

            // --------------------
            // Horario -> Empleado
            // --------------------
            modelBuilder.Entity<Horario>()
                .HasRequired(h => h.empleado)
                .WithMany(e => e.horarios)
                .HasForeignKey(h => h.empleadoid);
        }
    }
}
