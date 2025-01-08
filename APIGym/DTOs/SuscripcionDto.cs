using System.ComponentModel.DataAnnotations;

namespace APIGym.DTOs
{
    public class SuscripcionDto
    {
        public int IdPago { get; set; }
        public int IdGimnasio { get; set; }  // Clave foránea
        public string NombrePago { get; set; } = string.Empty;  // Evita nulos
        public decimal Monto { get; set; }
        public decimal MontoDinamico { get; set; }
        public string Frecuencia { get; set; } = string.Empty;  // Evita nulos
        public bool EsPlazoForzoso { get; set; }
        public DateTime DiaInicio { get; set; }  // No anulable para consistencia
        public int DiasRecordatorio { get; set; }
        public bool TieneDeuda { get; set; }  // Campo para indicar deuda
    }
}