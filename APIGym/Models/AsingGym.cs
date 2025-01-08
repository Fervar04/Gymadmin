using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace APIGym.Models
{
    public class AsingGym
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdGimnasio { get; set; }

        [Required]
        public Gimnasio Gimnasio { get; set; } = new Gimnasio(); // Inicialización para evitar advertencias de nulabilidad

        [Required]
        public string AdminId { get; set; } = string.Empty; // Inicialización para evitar advertencias de nulabilidad

        [Required]
        public IdentityUser Admin { get; set; } = new IdentityUser(); // Inicialización para evitar advertencias de nulabilidad
    }
}