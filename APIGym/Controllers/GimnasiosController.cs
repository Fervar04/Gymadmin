using APIGym.Data;
using APIGym.DTOs;
using APIGym.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class GimnasiosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GimnasiosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Gimnasios
    // Permite que SuperAdmin vea todos los gimnasios y que Admin vea solo los asignados.
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetGimnasios()
    {
        // Verifica si User.Identity o User.Identity.Name es null
        var usuarioActual = User.Identity?.Name;
        if (usuarioActual == null)
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        // Verificar el rol del usuario actual
        if (User.IsInRole("SuperAdmin"))
        {
            // Si es SuperAdmin, devuelve todos los gimnasios
            var gimnasios = await _context.Gimnasios.ToListAsync();
            return Ok(gimnasios);
        }
        else if (User.IsInRole("Admin"))
        {
            // Si es Admin, devuelve solo los gimnasios asignados a este usuario
            var gimnasios = await _context.AsingGyms
                .Where(a => a.AdminId == usuarioActual)
                .Include(a => a.Gimnasio) // Incluye los detalles del gimnasio
                .Select(a => a.Gimnasio)
                .ToListAsync();
            return Ok(gimnasios);
        }

        return Unauthorized("No tienes permiso para ver los gimnasios.");
    }

    // GET: api/Gimnasios/{id}
    // Permite ver los detalles de un gimnasio específico si el usuario tiene permiso.
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetGimnasio(int id)
    {
        // Verifica si User.Identity o User.Identity.Name es null
        var usuarioActual = User.Identity?.Name;
        if (usuarioActual == null)
        {
            return Unauthorized("No se pudo identificar al usuario.");
        }

        // Buscar el gimnasio en la base de datos
        var gimnasio = await _context.Gimnasios.FindAsync(id);
        if (gimnasio == null)
        {
            return NotFound("Gimnasio no encontrado.");
        }

        // Validar si el usuario tiene permiso para ver el gimnasio
        if (User.IsInRole("SuperAdmin") ||
            await _context.AsingGyms.AnyAsync(a => a.AdminId == usuarioActual && a.IdGimnasio == id))
        {
            return Ok(gimnasio);
        }

        return Unauthorized("No tienes permiso para ver este gimnasio.");
    }

   // POST: api/Gimnasios
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateGimnasio([FromBody] GimnasioCreateDto gimnasioDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Convertir GimnasioCreateDto a Gimnasio
        var gimnasio = new Gimnasio
        {
            Nombre = gimnasioDto.Nombre,
            Ciudad = gimnasioDto.Ciudad,
            Estado = gimnasioDto.Estado,
            Pais = gimnasioDto.Pais,
            CodigoPostal = gimnasioDto.CodigoPostal,
            FotoPerfil = gimnasioDto.FotoPerfil,
            NumeroAdmins = gimnasioDto.NumeroAdmins,
            NumeroClientes = gimnasioDto.NumeroClientes,
            NumeroVigilantes = gimnasioDto.NumeroVigilantes
        };

        _context.Gimnasios.Add(gimnasio);
        await _context.SaveChangesAsync();

        // Retorna el gimnasio creado y su URL en la respuesta
        return CreatedAtAction(nameof(GetGimnasio), new { id = gimnasio.IdGimnasio }, gimnasio);
    }


    // PUT: api/Gimnasios/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateGimnasio(int id, [FromBody] GimnasioUpdateDto gimnasioDto)
    {
        var gimnasio = await _context.Gimnasios.FindAsync(id);
        if (gimnasio == null)
        {
            return NotFound("Gimnasio no encontrado.");
        }

        // Actualizar propiedades de gimnasio con datos del DTO
        gimnasio.Nombre = gimnasioDto.Nombre;
        gimnasio.Ciudad = gimnasioDto.Ciudad;
        gimnasio.Estado = gimnasioDto.Estado;
        gimnasio.Pais = gimnasioDto.Pais;
        gimnasio.CodigoPostal = gimnasioDto.CodigoPostal;
        gimnasio.FotoPerfil = gimnasioDto.FotoPerfil;
        gimnasio.NumeroAdmins = gimnasioDto.NumeroAdmins;
        gimnasio.NumeroClientes = gimnasioDto.NumeroClientes;
        gimnasio.NumeroVigilantes = gimnasioDto.NumeroVigilantes;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Gimnasios.AnyAsync(g => g.IdGimnasio == id))
            {
                return NotFound("Gimnasio no encontrado.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Gimnasios/{id}
    // Elimina un gimnasio existente. Solo SuperAdmin puede acceder.
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteGimnasio(int id)
    {
        var gimnasio = await _context.Gimnasios.FindAsync(id);
        if (gimnasio == null)
        {
            return NotFound("Gimnasio no encontrado.");
        }

        _context.Gimnasios.Remove(gimnasio);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    
}