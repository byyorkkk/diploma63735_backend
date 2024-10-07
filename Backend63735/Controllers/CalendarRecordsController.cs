using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend63735.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AutoMapper;
using Backend63735.DTOs;

namespace Backend63735.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/calendarRecord")]
    public class CalendarRecordsController : ControllerBase
    {
        private readonly DemoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public CalendarRecordsController(DemoDbContext context, UserManager<IdentityUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }
        private string GetUserEmail()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }

        [HttpGet("getRecords")]
        public async Task<IActionResult> GetRecords()
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            var records = await _context.CalendarRecords
                 .Include(r => r.CalendarRecordPills)
                                        .Where(r => r.UserId == user.Id)
                                        .ToListAsync();
            var recordDtos = _mapper.Map<List<CalendarRecordDto>>(records);

            return Ok(recordDtos);
        }

        [HttpGet("getRecord/{id}")]
        public async Task<IActionResult> GetRecord(int id)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            var calendarRecord = await _context.CalendarRecords
                                               .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (calendarRecord == null)
            {
                return NotFound();
            }

            var recordDto = _mapper.Map<CalendarRecordDto>(calendarRecord);

            return Ok(recordDto);
        }

        [HttpPost("createRecord")]
        public async Task<IActionResult> CreateRecord([FromBody] CalendarRecordDto calendarRecordDto)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null || calendarRecordDto == null)
            {
                return BadRequest("Invalid user or calendar record.");
            }

            // Mapping CalendarRecordDto to CalendarRecord
            var calendarRecord = _mapper.Map<CalendarRecord>(calendarRecordDto);
            calendarRecord.UserId = user.Id;

            // Handling the Pills relationship
            if (calendarRecordDto.PillIds != null && calendarRecordDto.PillIds.Any())
            {
                // Fetching the pills based on provided IDs and linking them to the calendar record
                calendarRecord.CalendarRecordPills = calendarRecordDto.PillIds
                    .Select(id => new CalendarRecordPill { PillId = id, CalendarRecord = calendarRecord })
                    .ToList();
            }

            if (ModelState.IsValid)
            {
                _context.Add(calendarRecord);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRecord), new { id = calendarRecord.Id }, calendarRecordDto);
            }

            return BadRequest(ModelState);
        }

        [HttpPut("editRecord/{id}")]
        public async Task<IActionResult> EditRecord(int id, [FromBody] CalendarRecordDto calendarRecordDto)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null || calendarRecordDto == null)
            {
                return BadRequest("Invalid user or calendar record.");
            }

            var existingRecord = await _context.CalendarRecords.Include(cr => cr.CalendarRecordPills)
                                                               .FirstOrDefaultAsync(cr => cr.Id == id);

            if (existingRecord == null || existingRecord.UserId != user.Id)
            {
                return BadRequest("Record not found or user mismatch.");
            }

            // Mapping DTO to the existing entity
            _mapper.Map(calendarRecordDto, existingRecord);


            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteRecord(int id)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return BadRequest("Invalid user.");
            }

            var calendarRecord = await _context.CalendarRecords
                                              .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (calendarRecord == null)
            {
                return NotFound();
            }

            _context.CalendarRecords.Remove(calendarRecord);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}