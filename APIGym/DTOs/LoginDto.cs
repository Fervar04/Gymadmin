namespace APIGym.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty; // Inicialización para evitar advertencias de nulabilidad
        public string Password { get; set; } = string.Empty; // Inicialización para evitar advertencias de nulabilidad
    }
}