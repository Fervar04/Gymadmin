using System;
using System.Linq;
using System.Threading.Tasks;
using APIGym.Data;
using APIGym.Services;
using Microsoft.EntityFrameworkCore;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public NotificationService(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task EnviarRecordatoriosDePago()
{
    var hoy = DateTime.UtcNow;
    var suscripciones = await _context.ClienteSuscripciones
        .Include(cs => cs.Suscripcion)
        .Where(cs => cs.Suscripcion.EsPlazoForzoso &&
                     cs.FechaFin.AddDays(-cs.Suscripcion.DiasRecordatorio) <= hoy &&
                     cs.FechaFin > hoy)
        .ToListAsync();

    foreach (var suscripcion in suscripciones)
    {
        var email = await _context.Users
            .Where(u => u.Id == suscripcion.UserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync();

        var mensaje = $"Tu suscripción vence el {suscripcion.FechaFin.ToString("dd MMM yyyy")}. " +
                      $"Por favor realiza el pago antes de esta fecha para evitar interrupciones en el servicio.";
        await _emailService.SendEmailAsync(email, "Recordatorio de Vencimiento de Suscripción", mensaje);
    }
}
}