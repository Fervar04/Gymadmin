// GymAdminApp.DTOs.GimnasioUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace APIGym.DTOs
{
    public class GimnasioUpdateDto
    {
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Ciudad { get; set; }
        [Required]
        public string Estado { get; set; }
        [Required]
        public string Pais { get; set; }
        [Required]
        public string CodigoPostal { get; set; }
        public string FotoPerfil { get; set; }
        [Required]
        public int NumeroAdmins { get; set; }
        [Required]
        public int NumeroClientes { get; set; }
        [Required]
        public int NumeroVigilantes { get; set; }
    }
}