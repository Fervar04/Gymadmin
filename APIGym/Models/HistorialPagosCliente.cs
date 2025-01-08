using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIGym.Models
{
    public class HistorialPagosCliente
    {
        [Key]
        public int IdHistorialPago { get; set; }

        [Required]
        public int IdClienteSuscripcion { get; set; } // Relación con la suscripción del cliente

        [ForeignKey("IdClienteSuscripcion")]
        public ClienteSuscripcion ClienteSuscripcion { get; set; }

        [Required]
        public string UserId { get; set; } // Usuario que realiza el pago

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Precision(18, 2)]
        public decimal MontoPago { get; set; }

        [Required]
        [MaxLength(50)]
        public string MetodoPago { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
        public DateTime FechaFin { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string FrecuenciaPago { get; set; } = "Mensual";

        [MaxLength(200)]
        public string ComprobanteUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente";
    }
}