using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend63735.Models;
using AutoMapper;
using Backend63735.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Server.IIS.Core;

namespace Backend63735.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/pills")]
    public class PillsController : ControllerBase
    {
        private readonly DemoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public PillsController(DemoDbContext context, UserManager<IdentityUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }
        private string GetUserEmail()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetPills()
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user != null)
            {
                var pills = await _context.Pills
                                          .Where(p => p.UserId == user.Id)
                                          .ToListAsync();

                var pillDtos = _mapper.Map<List<PillDto>>(pills);

                return Ok(pillDtos);
            }
            else
                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = "Fail", Message = "Seems like this user does not have any pills" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPill(int id)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user != null)
            {
                var pill = await _context.Pills
                                         .FirstOrDefaultAsync(p => p.PillId == id && p.UserId == user.Id);

                if (pill == null)
                {
                    return NotFound();
                }

                var pillDto = _mapper.Map<PillDto>(pill); // Using AutoMapper to map to DTO

                return Ok(pillDto);
            }
            else
                return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = "Fail", Message = "Seems like this user does not have a pill with the provided id" });
        }

        [HttpPost("new")]
        public async Task<IActionResult> CreatePill([FromBody] PillDto pillDto)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                // Using AutoMapper or manual mapping to convert DTO to Entity
                var pill = _mapper.Map<Pill>(pillDto);

                pill.UserId = user.Id;

                if (ModelState.IsValid)
                {
                    _context.Pills.Add(pill);
                    await _context.SaveChangesAsync();

                    // Optionally, you can convert the saved pill back to DTO
                    var resultPillDto = _mapper.Map<PillDto>(pill);

                    return CreatedAtAction(nameof(GetPill), new { id = pill.PillId }, resultPillDto);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditPill(int id, [FromBody] PillDto pillDto)
        {
            if (pillDto == null)
            {
                return BadRequest("Pill data is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return Unauthorized("User not authorized");
            }

            var pill = await _context.Pills
                                    .FirstOrDefaultAsync(p => p.PillId == id && p.UserId == user.Id);

            if (pill == null)
            {
                return NotFound("Pill not found");
            }

            _mapper.Map(pillDto, pill);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePill(int id)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return Unauthorized("User not authorized");
            }

            var pill = await _context.Pills
                                    .FirstOrDefaultAsync(p => p.PillId == id && p.UserId == user.Id);

            if (pill == null)
            {
                return NotFound("Pill not found");
            }

            _context.Pills.Remove(pill);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
