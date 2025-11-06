using System;
using System.Collections.Generic;

namespace ReservaApp.Models;

public partial class VReservasUsuario
{
    public int IdReserva { get; set; }

    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int IdEvento { get; set; }

    public string TituloEvento { get; set; } = null!;

    public DateTime FechaEvento { get; set; }

    public string Lugar { get; set; } = null!;

    public int CantidadCupos { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaReserva { get; set; }
}
