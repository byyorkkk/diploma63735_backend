using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend63735.Models;
using AutoMapper;
using Backend63735.DTOs;
using Task = Backend63735.Models.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Backend63735.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly DemoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public TasksController(DemoDbContext context, UserManager<IdentityUser> userManager, IMapper mapper)
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
        public async Task<IActionResult> GetTasks()
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            var tasks = await _context.Tasks
                                     .Where(t => t.UserId == user.Id)
                                     .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<TaskDto>>(tasks));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            var task = await _context.Tasks
                                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TaskDto>(task));
        }

        [HttpPost("new")]
        public async Task<IActionResult> CreateTask([FromBody] TaskDto taskDto)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);


            var task = _mapper.Map<Task>(taskDto);
            task.UserId = user.Id;

            if (ModelState.IsValid)
            {
                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }

            return BadRequest(ModelState);
        }
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditTask(int id, [FromBody] TaskDto taskDto)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);
            if (task == null)
            {
                return NotFound();
            }

            _mapper.Map(taskDto, task);

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userEmail = GetUserEmail();
            var user = await _userManager.FindByEmailAsync(userEmail);

            var task = await _context.Tasks
                                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

            if (task == null)
            {
                return NotFound(new TaskResultDto
                {
                    IsSuccess = false,
                    Message = "Task not found."
                });
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new TaskResultDto
            {
                IsSuccess = true,
                Message = "Task successfully deleted."
            });
        }
    }
}
