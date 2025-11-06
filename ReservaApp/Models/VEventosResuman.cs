using System;
using System.Collections.Generic;

namespace ReservaApp.Models;

public partial class VEventosResuman
{
    public int IdEvento { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime Fecha { get; set; }

    public string Lugar { get; set; } = null!;

    public int Capacidad { get; set; }

    public int CuposDisponibles { get; set; }

    public int? CuposReservados { get; set; }

    public double? PorcentajeOcupacion { get; set; }

    public string Organizador { get; set; } = null!;

    public string EmailOrganizador { get; set; } = null!;

    public bool Activo { get; set; }

    public int? TotalReservas { get; set; }

    public int? UsuariosRegistrados { get; set; }
}
