using System.ComponentModel.DataAnnotations;

namespace ReservaApp.Models.ViewModels
{
    public class DashboardReservaViewModel
    {
        public int IdReserva { get; set; }

        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string EmailUsuario { get; set; } = string.Empty;

        [Display(Name = "Evento")]
        public string TituloEvento { get; set; } = string.Empty;

        [Display(Name = "Lugar")]
        public string LugarEvento { get; set; } = string.Empty;

        [Display(Name = "Fecha del Evento")]
        public DateTime FechaEvento { get; set; }

        [Display(Name = "Cupos")]
        public int CantidadCupos { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; } = string.Empty;

        [Display(Name = "Fecha de Reserva")]
        public DateTime FechaReserva { get; set; }

        public int CapacidadEvento { get; set; }
        public int CuposDisponibles { get; set; }
        public bool EventoActivo { get; set; }
    }
}