using System.ComponentModel.DataAnnotations;

namespace APIGym.DTOs
{
    public class SuscripcionSimpleDto
    {
        public int IdPago { get; set; }
        public string NombrePago { get; set; }
        public decimal Monto { get; set; }
        public decimal MontoDinamico { get; set; }
        public string? Frecuencia { get; set; }
        public bool EsPlazoForzoso { get; set; }
        public DateTime? DiaInicio { get; set; }
        public int DiasRecordatorio { get; set; }
    }
}