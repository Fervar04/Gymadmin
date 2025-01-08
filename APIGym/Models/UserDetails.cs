using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace APIGym.Models
{
    public class UserDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Configuramos `User` como navegación y evitamos inicializarlo aquí
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? FotoPerfil { get; set; }
        
        public DateTime FechaNacimiento { get; set; } = DateTime.MinValue;

        public string? CorreoElectronico { get; set; }
        public string? Genero { get; set; }
        
        public float PesoActual { get; set; } = 0.0f;
        public float PesoObjetivo { get; set; } = 0.0f;
        public float Altura { get; set; } = 0.0f;

        public string? Objetivo { get; set; }
    }
}