using System;
using System.Collections.Generic;

namespace ReservaApp.Models;

public partial class Evento
{
    public int IdEvento { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime Fecha { get; set; }

    public string Lugar { get; set; } = null!;

    public int Capacidad { get; set; }

    public int CuposDisponibles { get; set; }

    public int IdOrganizador { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuario IdOrganizadorNavigation { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
