using System.ComponentModel.DataAnnotations;

namespace ReservaApp.Models.ViewModels
{
    public class ReservaViewModel
    {
        public int IdReserva { get; set; }

        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string EmailUsuario { get; set; } = string.Empty;

        [Display(Name = "Evento")]
        public string TituloEvento { get; set; } = string.Empty;

        [Display(Name = "Fecha del Evento")]
        public DateTime FechaEvento { get; set; }

        [Display(Name = "Lugar")]
        public string LugarEvento { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad de cupos es requerida")]
        [Range(1, 10, ErrorMessage = "Puedes reservar entre 1 y 10 cupos")]
        [Display(Name = "Cantidad de Cupos")]
        public int CantidadCupos { get; set; } = 1;

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "pendiente";

        [Display(Name = "Fecha de Reserva")]
        public DateTime FechaReserva { get; set; }

        [Display(Name = "Fecha de Actualización")]
        public DateTime FechaActualizacion { get; set; }

        public int IdEvento { get; set; }
        public int IdUsuario { get; set; }

        // Propiedades adicionales del evento para mostrar
        public int CuposDisponibles { get; set; }
        public int CapacidadTotal { get; set; }
        public bool EventoActivo { get; set; }
    }

    public class EventoDisponibleViewModel
    {
        public int IdEvento { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string Lugar { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public int CuposDisponibles { get; set; }
        public string NombreOrganizador { get; set; } = string.Empty;
        public string EmailOrganizador { get; set; } = string.Empty;
        public bool TieneReserva { get; set; }
        public int? CuposReservados { get; set; }
    }
}