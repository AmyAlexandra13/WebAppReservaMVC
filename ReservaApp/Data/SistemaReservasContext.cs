using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ReservaApp.Models;

namespace ReservaApp.Data;

public partial class SistemaReservasContext : DbContext
{
    public SistemaReservasContext()
    {
    }

    public SistemaReservasContext(DbContextOptions<SistemaReservasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VEventosResuman> VEventosResumen { get; set; }

    public virtual DbSet<VReservasUsuario> VReservasUsuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=SistemaReservasEventos;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.IdEvento).HasName("PK__Evento__AF150CA5A5858F1C");

            entity.ToTable("Evento", tb => tb.HasTrigger("trg_insert_evento"));

            entity.HasIndex(e => e.Activo, "idx_activo");

            entity.HasIndex(e => e.Fecha, "idx_fecha");

            entity.HasIndex(e => e.IdOrganizador, "idx_organizador");

            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Capacidad).HasColumnName("capacidad");
            entity.Property(e => e.CuposDisponibles).HasColumnName("cupos_disponibles");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdOrganizador).HasColumnName("id_organizador");
            entity.Property(e => e.Lugar)
                .HasMaxLength(200)
                .HasColumnName("lugar");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdOrganizadorNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdOrganizador)
                .HasConstraintName("FK_Evento_Usuario");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reserva__423CBE5D66D953BC");

            entity.ToTable("Reserva", tb =>
                {
                    tb.HasTrigger("trg_before_insert_reserva");
                    tb.HasTrigger("trg_delete_reserva");
                    tb.HasTrigger("trg_update_reserva");
                });

            entity.HasIndex(e => e.Estado, "idx_estado");

            entity.HasIndex(e => e.IdEvento, "idx_evento");

            entity.HasIndex(e => e.FechaReserva, "idx_fecha_reserva");

            entity.HasIndex(e => e.IdUsuario, "idx_usuario");

            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.CantidadCupos)
                .HasDefaultValue(1)
                .HasColumnName("cantidad_cupos");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaReserva)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_reserva");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Evento");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__4E3E04ADC7432EBB");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Email, "UQ__Usuario__AB6E61643A23025B").IsUnique();

            entity.HasIndex(e => e.Email, "idx_email");

            entity.HasIndex(e => e.Rol, "idx_rol");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .HasDefaultValue("usuario")
                .HasColumnName("rol");
        });

        modelBuilder.Entity<VEventosResuman>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_eventos_resumen");

            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.Capacidad).HasColumnName("capacidad");
            entity.Property(e => e.CuposDisponibles).HasColumnName("cupos_disponibles");
            entity.Property(e => e.CuposReservados).HasColumnName("cupos_reservados");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EmailOrganizador)
                .HasMaxLength(150)
                .HasColumnName("email_organizador");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.Lugar)
                .HasMaxLength(200)
                .HasColumnName("lugar");
            entity.Property(e => e.Organizador)
                .HasMaxLength(100)
                .HasColumnName("organizador");
            entity.Property(e => e.PorcentajeOcupacion).HasColumnName("porcentaje_ocupacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");
            entity.Property(e => e.TotalReservas).HasColumnName("total_reservas");
            entity.Property(e => e.UsuariosRegistrados).HasColumnName("usuarios_registrados");
        });

        modelBuilder.Entity<VReservasUsuario>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_reservas_usuario");

            entity.Property(e => e.CantidadCupos).HasColumnName("cantidad_cupos");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasColumnName("estado");
            entity.Property(e => e.FechaEvento).HasColumnName("fecha_evento");
            entity.Property(e => e.FechaReserva).HasColumnName("fecha_reserva");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Lugar)
                .HasMaxLength(200)
                .HasColumnName("lugar");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(100)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.TituloEvento)
                .HasMaxLength(150)
                .HasColumnName("titulo_evento");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
