using APIGym.Data;
using APIGym.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using APIGym.DTOs;

[Route("api/[controller]")]
[ApiController]
public class SuscripcionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public SuscripcionesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    // GET: api/Suscripciones
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetSuscripciones(int? gimnasioId)
    {
        var usuarioActual = User.Identity.Name;
        var esSuperAdmin = User.IsInRole("SuperAdmin");

        List<SuscripcionSimpleDto> suscripcionesDto;

        if (esSuperAdmin)
        {
            // Si es SuperAdmin, puede ver todas las suscripciones del gimnasio especificado
            suscripcionesDto = await _context.Suscripciones
                .Where(s => s.IdGimnasio == gimnasioId)
                .Select(s => new SuscripcionSimpleDto
                {
                    IdPago = s.IdPago,
                    NombrePago = s.NombrePago,
                    Monto = s.Monto,
                    MontoDinamico = s.MontoDinamico,
                    Frecuencia = s.Frecuencia,
                    EsPlazoForzoso = s.EsPlazoForzoso,
                    DiaInicio = s.DiaInicio,
                    DiasRecordatorio = s.DiasRecordatorio
                })
                .ToListAsync();
        }
        else
        {
            // Si es Admin, solo puede ver las suscripciones de su gimnasio asignado
            suscripcionesDto = await _context.AsingGyms
                .Where(a => a.AdminId == usuarioActual && a.IdGimnasio == gimnasioId)
                .SelectMany(a => _context.Suscripciones.Where(s => s.IdGimnasio == a.IdGimnasio))
                .Select(s => new SuscripcionSimpleDto
                {
                    IdPago = s.IdPago,
                    NombrePago = s.NombrePago,
                    Monto = s.Monto,
                    MontoDinamico = s.MontoDinamico,
                    Frecuencia = s.Frecuencia,
                    EsPlazoForzoso = s.EsPlazoForzoso,
                    DiaInicio = s.DiaInicio,
                    DiasRecordatorio = s.DiasRecordatorio
                })
                .ToListAsync();
        }

        return Ok(suscripcionesDto);
    }

    // GET: api/Suscripciones/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetSuscripcion(int id)
    {
        var suscripcion = await _context.Suscripciones
            .Include(s => s.Gimnasio)
            .FirstOrDefaultAsync(s => s.IdPago == id);

        if (suscripcion == null)
        {
            return NotFound("Suscripción no encontrada.");
        }

        var usuarioActual = User.Identity.Name;
        var esSuperAdmin = User.IsInRole("SuperAdmin");

        if (esSuperAdmin || await _context.AsingGyms.AnyAsync(a => a.AdminId == usuarioActual && a.IdGimnasio == suscripcion.IdGimnasio))
        {
            return Ok(suscripcion);
        }
        return Unauthorized("No tienes permiso para ver esta suscripción.");
    }

   [HttpPost]
[Authorize(Roles = "SuperAdmin,Admin")]
public async Task<IActionResult> CreateSuscripcion([FromBody] SuscripcionCreateDto suscripcionDto)
{
    // Verificar si el gimnasio existe
    var gimnasio = await _context.Gimnasios.FindAsync(suscripcionDto.IdGimnasio);
    if (gimnasio == null)
    {
        return NotFound("Gimnasio no encontrado.");
    }

    // Crear la entidad de Suscripcion con los datos del DTO
    var suscripcion = new Suscripcion
    {
        IdGimnasio = suscripcionDto.IdGimnasio,
        NombrePago = suscripcionDto.NombrePago,
        Monto = suscripcionDto.Monto,
        MontoDinamico = suscripcionDto.MontoDinamico,
        Frecuencia = suscripcionDto.Frecuencia,
        EsPlazoForzoso = suscripcionDto.EsPlazoForzoso,
        DiaInicio = suscripcionDto.DiaInicio,
        DiasRecordatorio = suscripcionDto.DiasRecordatorio
    };

    _context.Suscripciones.Add(suscripcion);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetSuscripcion), new { id = suscripcion.IdPago }, suscripcion);
}
    // PUT: api/Suscripciones/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateSuscripcion(int id, [FromBody] SuscripcionDto suscripcionDto)
    {
        var suscripcionExistente = await _context.Suscripciones.FindAsync(id);
        if (suscripcionExistente == null)
        {
            return NotFound("Suscripción no encontrada.");
        }

        var usuarioActual = User.Identity.Name;
        var esSuperAdmin = User.IsInRole("SuperAdmin");

        if (esSuperAdmin || await _context.AsingGyms.AnyAsync(a => a.AdminId == usuarioActual && a.IdGimnasio == suscripcionDto.IdGimnasio))
        {
            // Registrar el historial antes de actualizar
            var historial = new HistorialSuscripcion
            {
                IdPago = suscripcionExistente.IdPago,
                Mes = DateTime.Now.Month,
                Año = DateTime.Now.Year,
                NuevoMonto = suscripcionExistente.Monto
            };
            _context.HistorialSuscripciones.Add(historial);

            // Actualizar la suscripción con los datos del DTO
            suscripcionExistente.Monto = suscripcionDto.Monto;
            suscripcionExistente.Frecuencia = suscripcionDto.Frecuencia;
            suscripcionExistente.EsPlazoForzoso = suscripcionDto.EsPlazoForzoso;
            suscripcionExistente.DiaInicio = suscripcionDto.DiaInicio;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        return Unauthorized("No tienes permiso para actualizar esta suscripción.");
    }

    // DELETE: api/Suscripciones/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteSuscripcion(int id)
    {
        var suscripcion = await _context.Suscripciones.FindAsync(id);
        if (suscripcion == null)
        {
            return NotFound("Suscripción no encontrada.");
        }

        _context.Suscripciones.Remove(suscripcion);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpPost("AsignarSuscripcionDirecta")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AsignarSuscripcionDirecta(string userId, int idGimnasio, int idPago)
    {
        // Verificar si el gimnasio existe
        var gimnasio = await _context.Gimnasios.FindAsync(idGimnasio);
        if (gimnasio == null)
        {
            return BadRequest("El gimnasio especificado no existe.");
        }

        // Crear la relación ClienteSuscripcion con los valores directamente proporcionados
        var clienteSuscripcion = new ClienteSuscripcion
        {
            UserId = userId,
            IdGimnasio = idGimnasio,
            IdPago = idPago,
            EstadoSuscripcion = "Activa",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddMonths(1)
        };

        try
        {
            _context.ClienteSuscripciones.Add(clienteSuscripcion);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"Error al guardar la suscripción. Detalles: {ex.InnerException?.Message ?? ex.Message}");
        }

        return Ok("Suscripción asignada al cliente exitosamente.");
    }
   
    private DateTime CalcularFechaFin(string frecuencia, DateTime fechaInicio, bool esPlazoForzoso = false, DateTime? diaInicio = null)
    {
        if (esPlazoForzoso && diaInicio.HasValue)
        {
            // Si es plazo forzoso, calcula el vencimiento a partir de la fecha de inicio específica
            return diaInicio.Value.AddMonths(1);  // En este ejemplo, considera el plazo forzoso como mensual
        }

        // Si no es plazo forzoso, calcula basado en la frecuencia
        return frecuencia switch
        {
            "Diario" => fechaInicio.AddDays(1),
            "Semanal" => fechaInicio.AddDays(7),
            "Mensual" => fechaInicio.AddMonths(1),
            "Anual" => fechaInicio.AddYears(1),
            _ => fechaInicio  // Si no coincide ninguna frecuencia, devuelve la fecha de inicio sin cambios
        };
    }
    // GymAdminApp.Controllers.SuscripcionesController.cs
    private async Task<bool> VerificarDeuda(string userId, string frecuencia, DateTime diaInicio)
    {
        DateTime fechaInicioPeriodo;
        DateTime fechaFinPeriodo;

        if (frecuencia == "Mensual")
        {
            fechaInicioPeriodo = new DateTime(diaInicio.Year, diaInicio.Month, 1);
            fechaFinPeriodo = fechaInicioPeriodo.AddMonths(1).AddDays(-1);
        }
        else if (frecuencia == "Anual")
        {
            fechaInicioPeriodo = new DateTime(diaInicio.Year, 1, 1);
            fechaFinPeriodo = fechaInicioPeriodo.AddYears(1).AddDays(-1);
        }
        else
        {
            throw new InvalidOperationException("Frecuencia no válida.");
        }

        // Consulta si existe un pago registrado dentro del periodo actual.
        return !await _context.HistorialPagosClientes.AnyAsync(h =>
            h.ClienteSuscripcion.UserId == userId &&
            h.FechaPago >= fechaInicioPeriodo &&
            h.FechaPago <= fechaFinPeriodo &&
            h.Estado == "Aprobado");
    }

}