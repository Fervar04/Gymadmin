using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace APIGym.Models
{
    public class ClienteSuscripcion
    {
        public ClienteSuscripcion()
        {
            FechaInicio = DateTime.UtcNow;
            FechaFin = DateTime.UtcNow.AddMonths(1); // Asignación inicial para FechaFin
            EstadoSuscripcion = "Activa";
        }

        [Key]
        public int IdClienteSuscripcion { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; } // No inicializamos aquí

        [Required]
        public int IdGimnasio { get; set; }

        [ForeignKey("IdGimnasio")]
        public Gimnasio Gimnasio { get; set; } // No inicializamos aquí

        [Required]
        public int IdPago { get; set; }

        [ForeignKey("IdPago")]
        public Suscripcion Suscripcion { get; set; } // No inicializamos aquí

        [Required]
        public string EstadoSuscripcion { get; set; } = "Activa";

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }
    }
}