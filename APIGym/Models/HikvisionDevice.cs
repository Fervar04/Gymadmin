using System.ComponentModel.DataAnnotations;

namespace APIGym.Models
{
    public class HikvisionDevice
    {
        [Key]
        public int DeviceIndex { get; set; }

        [Required] // Indica que siempre debe existir un Id de Gimnasio
        public int IdGimnasio { get; set; }

        [Required] // Relación con Gimnasio, inicializada para evitar advertencias
        public Gimnasio Gimnasio { get; set; } = new Gimnasio();
    }
}