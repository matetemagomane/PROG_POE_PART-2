using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Description,
                    e.EventType,
                    e.Location,
                    e.EventDate,
                    Categories = e.Categories.Select(c => new
                    {
                        c.CategoryId,
                        c.Name,
                        c.DistanceKm,
                        c.EntryFee,
                        c.MaxCapacity
                    })
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Categories)
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Description,
                    e.EventType,
                    e.Location,
                    e.EventDate,
                    Categories = e.Categories.Select(c => new
                    {
                        c.CategoryId,
                        c.Name,
                        c.DistanceKm,
                        c.EntryFee,
                        c.MaxCapacity
                    })
                })
                .FirstOrDefaultAsync();

            if (ev == null) return NotFound(new { message = "Event not found." });
            return Ok(ev);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (role != "Organiser" || !userId.HasValue)
            {
                return StatusCode(403, new { message = "Forbidden: Organiser access required." });
            }

            var newEvent = new Event
            {
                OrganiserUserId = userId.Value,
                Title = dto.Title,
                Description = dto.Description,
                EventType = dto.EventType,
                Location = dto.Location,
                EventDate = dto.EventDate
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, newEvent);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden: Organiser access required." });
            }

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.EventType = dto.EventType;
            ev.Location = dto.Location;
            ev.EventDate = dto.EventDate;

            await _context.SaveChangesAsync();
            return Ok(ev);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden: Organiser access required." });
            }

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}