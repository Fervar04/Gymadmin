using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIGym.Models
{
    public class Suscripcion
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public int IdGimnasio { get; set; } // Clave foránea para el gimnasio

        [ForeignKey("IdGimnasio")]
        public Gimnasio Gimnasio { get; set; } // Relación con el gimnasio

        [Required]
        public string NombrePago { get; set; } = string.Empty;

        [Required]
        public decimal Monto { get; set; }

        public decimal MontoDinamico { get; set; }

        public string? Frecuencia { get; set; }
        
        public bool EsPlazoForzoso { get; set; }

        public DateTime? DiaInicio { get; set; }

        public int DiasRecordatorio { get; set; }
    }
}