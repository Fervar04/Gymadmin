using APIGym.Data;
using APIGym.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using APIGym.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using APIGym.Services;

[Route("api/[controller]")]
[ApiController]
public class UsuariosController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;

    public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConfiguration configuration, EmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    // POST: api/Usuarios/CrearUsuario
    [HttpPost("CrearUsuario")]
    [AllowAnonymous] // Permitir acceso sin autenticación
    public async Task<IActionResult> CrearUsuario([FromBody] UsuarioConDetallesDto usuarioDto)
    {
        // Verificar si el usuario ya existe en AspNetUsers por su correo electrónico
        var existingUser = await _userManager.FindByEmailAsync(usuarioDto.Email);
        if (existingUser != null)
        {
            return Conflict("El usuario ya existe en AspNetUsers.");
        }

        // Crear un nuevo usuario en AspNetUsers
        var user = new IdentityUser
        {
            UserName = usuarioDto.Email,
            Email = usuarioDto.Email,
            EmailConfirmed = false // Requerir confirmación de email
        };

        // Intentar crear el usuario en AspNetUsers
        var createUserResult = await _userManager.CreateAsync(user, usuarioDto.Password);
        if (!createUserResult.Succeeded)
        {
            return BadRequest(createUserResult.Errors);
        }

        // Confirmar que el usuario se ha creado en AspNetUsers y obtener el ID
        var createdUser = await _userManager.FindByEmailAsync(usuarioDto.Email);
        if (createdUser == null)
        {
            return StatusCode(500, "Error al verificar el usuario creado en AspNetUsers.");
        }

        // Crear el registro en UserDetails usando el UserId del usuario recién creado
        var userDetails = new UserDetails
        {
            UserId = createdUser.Id, // Usando el ID correcto del usuario creado
            Nombre = usuarioDto.Nombre,
            Apellido = usuarioDto.Apellido,
            Telefono = usuarioDto.Telefono,
            FotoPerfil = usuarioDto.FotoPerfil,
            FechaNacimiento = usuarioDto.FechaNacimiento,
            CorreoElectronico = usuarioDto.Email,
            Genero = usuarioDto.Genero,
            PesoActual = usuarioDto.PesoActual,
            PesoObjetivo = usuarioDto.PesoObjetivo,
            Altura = usuarioDto.Altura,
            Objetivo = usuarioDto.Objetivo
        };

        // Intentar agregar y guardar UserDetails en la base de datos
        _context.UserDetails.Add(userDetails);
        await _context.SaveChangesAsync();

        // Generar el token de confirmación de correo
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(createdUser);

        // Crear el enlace de confirmación
        var confirmationLink = Url.Action(nameof(ConfirmarEmail), "Usuarios",
            new { userId = createdUser.Id, token }, Request.Scheme);

        // Contenido del correo
        var message = $"<p>Por favor, confirma tu correo electrónico haciendo clic en el siguiente enlace:</p><p><a href='{confirmationLink}'>Confirmar Email</a></p>";

        // Enviar el correo de confirmación
        await _emailService.SendEmailAsync(usuarioDto.Email, "Confirmación de correo electrónico", message);

        return Ok("Usuario creado exitosamente con detalles adicionales. Revisa tu correo para confirmar la cuenta.");
    }
    // GET: api/Usuarios/ConfirmarEmail
    [HttpGet("ConfirmarEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("Usuario no encontrado.");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Ok("Cuenta confirmada con éxito.") : BadRequest("Error al confirmar la cuenta.");
    }
    // POST: api/Usuarios/CrearAdmin
    // POST: api/Usuarios/CrearAdmin
    [HttpPost("CrearAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CrearAdmin([FromBody] UsuarioConDetallesDto adminDto)
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        Console.WriteLine("Claims recibidos en el servidor:");
        foreach (var claim in claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }

        if (!User.IsInRole("SuperAdmin"))
        {
            return Unauthorized("El usuario no tiene el rol SuperAdmin.");
        }

        return Ok("El usuario tiene los permisos necesarios.");
    }
    // DELETE: api/Usuarios/Eliminar/{userId}
    [HttpDelete("Eliminar/{userId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> EliminarUsuario(string userId)
    {
        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario == null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var userDetails = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserId == userId);
        if (userDetails != null)
        {
            _context.UserDetails.Remove(userDetails);
        }

        await _userManager.DeleteAsync(usuario);
        await _context.SaveChangesAsync();

        return Ok("Usuario eliminado con éxito.");
    }
    // PUT: api/Usuarios/Editar/{userId}
    [HttpPut("Editar/{userId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> EditarUsuario(string userId, [FromBody] UsuarioConDetallesDto usuarioDto)
    {
        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario == null)
        {
            return NotFound("Usuario no encontrado.");
        }

        usuario.Email = usuarioDto.Email;
        usuario.UserName = usuarioDto.Email;
        await _userManager.UpdateAsync(usuario);

        var userDetails = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserId == userId);
        if (userDetails != null)
        {
            userDetails.Nombre = usuarioDto.Nombre;
            userDetails.Apellido = usuarioDto.Apellido;
            userDetails.Telefono = usuarioDto.Telefono;
            userDetails.FotoPerfil = usuarioDto.FotoPerfil;
            userDetails.FechaNacimiento = usuarioDto.FechaNacimiento;
            userDetails.Genero = usuarioDto.Genero;
            userDetails.PesoActual = usuarioDto.PesoActual;
            userDetails.PesoObjetivo = usuarioDto.PesoObjetivo;
            userDetails.Altura = usuarioDto.Altura;
            userDetails.Objetivo = usuarioDto.Objetivo;
            await _context.SaveChangesAsync();
        }

        return Ok("Usuario actualizado con éxito.");
    }
    // GET: api/Usuarios/Listar
    [HttpGet("Listar")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ListarUsuarios()
    {
        // Obtener los IDs de los usuarios
        var usuariosIds = await _userManager.Users
            .Select(u => u.Id)
            .ToListAsync();

        // Filtrar los detalles de usuario con los IDs obtenidos
        var usuariosConDetalles = await _context.UserDetails
            .Where(d => usuariosIds.Contains(d.UserId))
            .ToListAsync();

        return Ok(usuariosConDetalles);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var usuario = await _userManager.FindByEmailAsync(loginDto.Email);
        if (usuario == null || !await _userManager.CheckPasswordAsync(usuario, loginDto.Password))
        {
            return Unauthorized("Credenciales incorrectas.");
        }

        // Crear los claims para el token
        var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.UserName),
        new Claim(ClaimTypes.Email, usuario.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        // Obtener los roles del usuario
        var roles = await _userManager.GetRolesAsync(usuario);
        foreach (var role in roles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Configuración del JWT
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"])),
            claims: authClaims,
            signingCredentials: creds
        );

        // Obtener información adicional del usuario
        var userDetails = await _context.UserDetails.FirstOrDefaultAsync(ud => ud.UserId == usuario.Id);
        var asignacionesCliente = await _context.ClienteGimnasios
            .Where(cg => cg.UserId == usuario.Id)
            .Include(cg => cg.Gimnasio)
            .ToListAsync();

        var gimnasiosDto = asignacionesCliente.Select(ac => new GimnasioDto
        {
            IdGimnasio = ac.Gimnasio.IdGimnasio,
            Nombre = ac.Gimnasio.Nombre,
            Ciudad = ac.Gimnasio.Ciudad,
            Estado = ac.Gimnasio.Estado,
            Pais = ac.Gimnasio.Pais,
            CodigoPostal = ac.Gimnasio.CodigoPostal,
            FotoPerfil = ac.Gimnasio.FotoPerfil
        }).ToList();

        var suscripcionesDto = await _context.ClienteSuscripciones
            .Where(cs => cs.UserId == usuario.Id)
            .Include(cs => cs.Suscripcion)
            .Select(cs => new SuscripcionDto
            {
                IdPago = cs.Suscripcion.IdPago,
                IdGimnasio = cs.IdGimnasio,
                NombrePago = cs.Suscripcion.NombrePago,
                Monto = cs.Suscripcion.Monto,
                MontoDinamico = cs.Suscripcion.MontoDinamico,
                Frecuencia = cs.Suscripcion.Frecuencia,
                EsPlazoForzoso = cs.Suscripcion.EsPlazoForzoso,
                DiaInicio = cs.Suscripcion.DiaInicio ?? DateTime.UtcNow,
                DiasRecordatorio = cs.Suscripcion.DiasRecordatorio,
                TieneDeuda = !_context.HistorialPagosClientes
                    .Any(h => h.IdClienteSuscripcion == cs.IdClienteSuscripcion &&
                              h.FechaFin >= DateTime.UtcNow)  // Deuda si no tiene pagos válidos
            }).ToListAsync();

        // Seleccionar el primer rol como rol principal (puede haber múltiples roles)
        string primaryRole = roles.FirstOrDefault() ?? "Usuario";

        // Crear la respuesta incluyendo el token, rol y todos los datos actuales
        var response = new
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = usuario.Id,
            UserName = usuario.UserName,
            Email = usuario.Email,
            Role = primaryRole, // Agregado el rol principal
            UserDetails = userDetails != null ? new
            {
                userDetails.Nombre,
                userDetails.Apellido,
                userDetails.Telefono,
                userDetails.FotoPerfil,
                userDetails.FechaNacimiento,
                userDetails.CorreoElectronico,
                userDetails.Genero,
                userDetails.PesoActual,
                userDetails.PesoObjetivo,
                userDetails.Altura,
                userDetails.Objetivo
            } : null,
            Gimnasios = gimnasiosDto,
            Suscripciones = suscripcionesDto
        };

        return Ok(response);
    }
    [HttpPost("GenerarCodigo")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GenerarCodigo(int idGimnasio, string rolAsignado, string email)
    {
        var gimnasio = await _context.Gimnasios.FindAsync(idGimnasio);
        if (gimnasio == null)
            return NotFound("Gimnasio no encontrado.");

        // Generar un código de 6 caracteres alfanuméricos
        var codigo = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        var nuevoCodigo = new CodigoAsignacion
        {
            Codigo = codigo,
            IdGimnasio = idGimnasio,
            RolAsignado = rolAsignado,
            FechaCreacion = DateTime.UtcNow
        };

        _context.CodigosAsignacion.Add(nuevoCodigo);
        await _context.SaveChangesAsync();

        // Enviar el código al correo especificado
        var mensaje = $"Tu código de asignación es: {codigo}. Usa este código para registrarte en el gimnasio.";
        await _emailService.SendEmailAsync(email, "Código de Asignación de Gimnasio", mensaje);

        return Ok($"Código generado y enviado a {email}");
    }
    [HttpPost("UsarCodigo")]
    public async Task<IActionResult> UsarCodigo(string codigo, string userId)
    {
        // Buscar el código de asignación en la base de datos y verificar que no haya sido usado
        var codigoAsignacion = await _context.CodigosAsignacion
            .Include(c => c.Gimnasio)
            .FirstOrDefaultAsync(c => c.Codigo == codigo && !c.Usado);

        if (codigoAsignacion == null)
            return BadRequest("Código inválido o ya utilizado.");

        // Buscar el usuario en la base de datos
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("Usuario no encontrado.");

        // Asignar el rol al usuario basado en el rol del código
        var result = await _userManager.AddToRoleAsync(user, codigoAsignacion.RolAsignado);
        if (!result.Succeeded)
            return BadRequest("Error al asignar el rol al usuario.");

        // Asignar el usuario al gimnasio correspondiente
        _context.ClienteGimnasios.Add(new ClienteGimnasio
        {
            UserId = user.Id,
            IdGimnasio = codigoAsignacion.IdGimnasio
        });

        // Marcar el código como usado
        codigoAsignacion.Usado = true;

        // Guardar los cambios en la base de datos
        await _context.SaveChangesAsync();

        return Ok("Usuario asignado al gimnasio con éxito y rol asignado correctamente.");
    }
}