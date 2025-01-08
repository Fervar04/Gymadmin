using APIGym.Data;
using APIGym.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class PagosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PagosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/Pagos/PagoForzoso
    [HttpPost("PagoForzoso")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> PagoForzoso(string userId, int idPago, decimal monto, string metodoPago)
    {
        // Buscar la suscripción de cliente usando userId e idPago
        var clienteSuscripcion = await _context.ClienteSuscripciones
            .Include(cs => cs.Suscripcion)
            .FirstOrDefaultAsync(cs => cs.UserId == userId && cs.IdPago == idPago);

        if (clienteSuscripcion == null)
        {
            return NotFound("Suscripción de cliente no encontrada.");
        }

        // Verificar la existencia del gimnasio
        var gimnasio = await _context.Gimnasios.FindAsync(clienteSuscripcion.IdGimnasio);
        if (gimnasio == null)
        {
            return NotFound("Gimnasio no encontrado. Asegúrate de que el gimnasio existe antes de asignar una suscripción.");
        }

        // Actualizar la fecha de vencimiento en base a la frecuencia
        clienteSuscripcion.FechaFin = CalcularFechaFin(
            clienteSuscripcion.Suscripcion.Frecuencia,
            clienteSuscripcion.FechaFin,
            clienteSuscripcion.Suscripcion.EsPlazoForzoso,
            clienteSuscripcion.Suscripcion.DiaInicio
        );

        // Registrar el pago en el historial de pagos
        var historialPago = new HistorialPagosCliente
        {
            IdClienteSuscripcion = clienteSuscripcion.IdClienteSuscripcion,
            UserId = userId,  // Asignar el UserId al historial de pagos
            FechaPago = DateTime.UtcNow,
            MontoPago = monto,
            MetodoPago = metodoPago,
            FechaInicio = clienteSuscripcion.FechaInicio,
            FechaFin = clienteSuscripcion.FechaFin,
            FrecuenciaPago = clienteSuscripcion.Suscripcion.Frecuencia,
            Estado = "Aprobado"
        };

        _context.HistorialPagosClientes.Add(historialPago);
        await _context.SaveChangesAsync();

        return Ok("Pago forzoso registrado exitosamente.");
    }

    // POST: api/Pagos/PagoFlexible
    [HttpPost("PagoFlexible")]
    [Authorize(Roles = "SuperAdmin,Admin,Cliente")]
    public async Task<IActionResult> PagoFlexible(string userId, int idPago, decimal monto, string metodoPago, string frecuencia)
    {
        // Buscar la suscripción del cliente usando userId y idPago
        var clienteSuscripcion = await _context.ClienteSuscripciones
            .Include(cs => cs.Suscripcion)
            .Include(cs => cs.Gimnasio)
            .FirstOrDefaultAsync(cs => cs.UserId == userId && cs.IdPago == idPago);

        if (clienteSuscripcion == null)
        {
            return NotFound("Suscripción o cliente no encontrados.");
        }

        // Verificar la configuración del gimnasio
        var config = await _context.GymConfigs.FirstOrDefaultAsync(c => c.GimnasioId == clienteSuscripcion.IdGimnasio);
        if (config == null)
        {
            return NotFound("Configuración de gimnasio no encontrada.");
        }

        // Determinar el monto requerido en base a la frecuencia
        decimal valorRequerido = frecuencia switch
        {
            "Diario" => config.ValorDiario,
            "Semanal" => config.ValorSemanal,
            "Mensual" => config.ValorMensual,
            "Anual" => config.ValorAnual,
            _ => 0
        };

        if (monto < valorRequerido)
        {
            return BadRequest("Monto insuficiente.");
        }

        // Calcular la fecha de fin en base a la frecuencia flexible
        clienteSuscripcion.FechaFin = CalcularFechaFin(frecuencia, clienteSuscripcion.FechaFin);

        // Registrar el pago en el historial de pagos
        var historialPago = new HistorialPagosCliente
        {
            IdClienteSuscripcion = clienteSuscripcion.IdClienteSuscripcion,
            FechaPago = DateTime.UtcNow,
            MontoPago = monto,
            MetodoPago = metodoPago,
            FechaInicio = clienteSuscripcion.FechaInicio,
            FechaFin = clienteSuscripcion.FechaFin,
            FrecuenciaPago = frecuencia,
            Estado = "Aprobado"
        };

        _context.HistorialPagosClientes.Add(historialPago);
        await _context.SaveChangesAsync();

        return Ok("Pago flexible registrado exitosamente.");
    }

    // POST: api/Pagos/AprobarPago
    [HttpPost("AprobarPago")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AprobarPago(int idHistorialPago)
    {
        var historialPago = await _context.HistorialPagosClientes.FindAsync(idHistorialPago);
        if (historialPago == null || historialPago.Estado != "Pendiente")
        {
            return BadRequest("Pago no encontrado o ya procesado.");
        }

        // Actualizar el estado a Aprobado y extender la fecha de fin
        historialPago.Estado = "Aprobado";
        var clienteSuscripcion = await _context.ClienteSuscripciones
            .Include(cs => cs.Suscripcion)
            .FirstOrDefaultAsync(cs => cs.IdClienteSuscripcion == historialPago.IdClienteSuscripcion);

        clienteSuscripcion.FechaFin = CalcularFechaFin(
            clienteSuscripcion.Suscripcion.Frecuencia,
            clienteSuscripcion.FechaFin == DateTime.MinValue ? DateTime.UtcNow : clienteSuscripcion.FechaFin,
            clienteSuscripcion.Suscripcion.EsPlazoForzoso,
            clienteSuscripcion.Suscripcion.DiaInicio
        );

        await _context.SaveChangesAsync();
        return Ok("Pago aprobado exitosamente.");
    }

    // POST: api/Pagos/RechazarPago
    [HttpPost("RechazarPago")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RechazarPago(int idHistorialPago)
    {
        var historialPago = await _context.HistorialPagosClientes.FindAsync(idHistorialPago);
        if (historialPago == null || historialPago.Estado != "Pendiente")
        {
            return BadRequest("Pago no encontrado o ya procesado.");
        }

        historialPago.Estado = "Rechazado";
        await _context.SaveChangesAsync();
        return Ok("Pago rechazado.");
    }

    // Método para calcular la fecha de fin basado en la frecuencia
    private DateTime CalcularFechaFin(string frecuencia, DateTime fechaVencimientoActual, bool esPlazoForzoso = false, DateTime? diaInicio = null)
    {
        if (esPlazoForzoso && diaInicio.HasValue)
        {
            return diaInicio.Value.AddMonths(1); // Ajustar según el plazo deseado
        }

        return frecuencia switch
        {
            "Diario" => fechaVencimientoActual.AddDays(1),
            "Semanal" => fechaVencimientoActual.AddDays(7),
            "Mensual" => fechaVencimientoActual.AddMonths(1),
            "Anual" => fechaVencimientoActual.AddYears(1),
            _ => fechaVencimientoActual
        };
    }
}