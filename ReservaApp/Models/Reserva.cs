using System;
using System.Collections.Generic;

namespace ReservaApp.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdUsuario { get; set; }

    public int IdEvento { get; set; }

    public int CantidadCupos { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaReserva { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
