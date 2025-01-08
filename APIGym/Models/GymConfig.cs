using System.ComponentModel.DataAnnotations;

namespace APIGym.Models
{
    public class GymConfig
    {
        [Key]
        public int Id { get; set; }
        public int GimnasioId { get; set; }
        public decimal ValorDiario { get; set; }
        public decimal ValorSemanal { get; set; }
        public decimal ValorMensual { get; set; }
        public decimal ValorAnual { get; set; }

        // Relaciones
        public Gimnasio Gimnasio { get; set; }
    }
}