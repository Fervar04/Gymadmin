using System;

namespace APIGym.DTOs
{
    public class UsuarioConDetallesDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string FotoPerfil { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; } = DateTime.UtcNow;
        public string Genero { get; set; } = string.Empty;
        public float PesoActual { get; set; } = 0.0f;
        public float PesoObjetivo { get; set; } = 0.0f;
        public float Altura { get; set; } = 0.0f;
        public string Objetivo { get; set; } = string.Empty;
    }
}