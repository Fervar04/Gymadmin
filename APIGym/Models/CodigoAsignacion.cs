using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIGym.Models
{
    public class CodigoAsignacion
    {
        [Key]
        public int Id { get; set; }

        public string Codigo { get; set; }

        public int IdGimnasio { get; set; }

        [ForeignKey(nameof(IdGimnasio))]
        public Gimnasio Gimnasio { get; set; } // Indica que IdGimnasio es la FK para Gimnasio

        public string RolAsignado { get; set; }
        
        public DateTime FechaCreacion { get; set; }
        
        public bool Usado { get; set; } = false;
    }
}