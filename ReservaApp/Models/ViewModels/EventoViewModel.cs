using System.ComponentModel.DataAnnotations;

namespace ReservaApp.Models.ViewModels
{
    public class EventoViewModel
    {
        public int IdEvento { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(150, ErrorMessage = "El título no puede exceder 150 caracteres")]
        [Display(Name = "Título del Evento")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        [Display(Name = "Fecha del Evento")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El lugar es requerido")]
        [StringLength(200, ErrorMessage = "El lugar no puede exceder 200 caracteres")]
        [Display(Name = "Lugar")]
        public string Lugar { get; set; } = string.Empty;

        [Required(ErrorMessage = "La capacidad es requerida")]
        [Range(1, 10000, ErrorMessage = "La capacidad debe estar entre 1 y 10000")]
        [Display(Name = "Capacidad")]
        public int Capacidad { get; set; }

        [Display(Name = "Cupos Disponibles")]
        public int CuposDisponibles { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Organizador")]
        public string? NombreOrganizador { get; set; }

        [Display(Name = "Email Organizador")]
        public string? EmailOrganizador { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime? FechaCreacion { get; set; }
    }
}