// GymAdminApp.DTOs.GimnasioCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace APIGym.DTOs
{
    public class GimnasioCreateDto
    {
        public string Nombre { get; set; }
        public string Ciudad { get; set; }
        public string Estado { get; set; }
        public string Pais { get; set; }
        public string CodigoPostal { get; set; }
        public string FotoPerfil { get; set; }
        public int NumeroAdmins { get; set; }
        public int NumeroClientes { get; set; }
        public int NumeroVigilantes { get; set; }
    }
}