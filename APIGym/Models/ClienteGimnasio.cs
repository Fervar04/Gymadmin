using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace APIGym.Models
{
    public class ClienteGimnasio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; } // No inicializamos aquí; EF se encarga de la instancia

        [Required]
        public int IdGimnasio { get; set; }

        [ForeignKey(nameof(IdGimnasio))]
        public Gimnasio Gimnasio { get; set; } // No inicializamos aquí; EF se encarga de la instancia
    }
}