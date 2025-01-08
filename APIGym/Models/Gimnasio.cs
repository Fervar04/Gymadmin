using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APIGym.Models
{
    public class Gimnasio
    {
        [Key]
        public int IdGimnasio { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Ciudad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string FotoPerfil { get; set; } = string.Empty;
        public int NumeroAdmins { get; set; }
        public int NumeroClientes { get; set; }
        public int NumeroVigilantes { get; set; }

        public ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
        public ICollection<ClienteGimnasio> ClienteGimnasios { get; set; } = new List<ClienteGimnasio>();
        public ICollection<CodigoAsignacion> CodigosAsignacion { get; set; } = new List<CodigoAsignacion>();
        public ICollection<ClienteSuscripcion> ClienteSuscripciones { get; set; } = new List<ClienteSuscripcion>();
    }
}