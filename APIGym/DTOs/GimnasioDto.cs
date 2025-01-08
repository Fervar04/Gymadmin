using System.ComponentModel.DataAnnotations;

namespace APIGym.DTOs
{
    public class GimnasioDto
    {
        public int IdGimnasio { get; set; }
        public string Nombre { get; set; }
        public string Ciudad { get; set; }
        public string Estado { get; set; }
        public string Pais { get; set; }
        public string CodigoPostal { get; set; }
        public string FotoPerfil { get; set; }
    }
}