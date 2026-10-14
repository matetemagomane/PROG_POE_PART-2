using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnrolmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (role != "Participant" || !userId.HasValue)
            {
                return StatusCode(403, new { message = "Forbidden: Participant access required." });
            }

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.UserId == userId.Value && e.CategoryId == dto.CategoryId);

            if (alreadyEnrolled)
            {
                return BadRequest(new { message = "You are already enrolled in this race category." });
            }

            var enrolment = new Enrolment
            {
                UserId = userId.Value,
                CategoryId = dto.CategoryId,
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(enrolment);
        }

        [HttpGet("my-enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return Unauthorized();

            var list = await _context.Enrolments
                .Where(e => e.UserId == userId.Value)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EnrolledAt,
                    Category = e.Category.Name,
                    Event = e.Category.Event.Title,
                    EventDate = e.Category.Event.EventDate
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}