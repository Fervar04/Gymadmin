using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIGym.Models
{
    public class HistorialSuscripcion
    {
        [Key]
        public int IdHistorial { get; set; }

        [Required] // Se asume que IdPago es obligatorio
        public int IdPago { get; set; }

        [Required] // Se asume que siempre debe existir una suscripción relacionada
        public Suscripcion Suscripcion { get; set; } = new Suscripcion();

        [Range(1, 12)] // Validación para asegurar que el mes esté entre 1 y 12
        public int Mes { get; set; }

        [Range(1, int.MaxValue)] // Validación para asegurar que el año sea positivo
        public int Año { get; set; }

        [Precision(18, 2)]
        public decimal NuevoMonto { get; set; } = 0.0m; // Inicialización en 0.0m para evitar advertencias
    }
}