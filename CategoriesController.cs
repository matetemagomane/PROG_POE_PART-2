using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden: Organiser access required." });
            }

            var category = new Category
            {
                EventId = dto.EventId,
                Name = dto.Name,
                DistanceKm = dto.DistanceKm,
                EntryFee = dto.EntryFee,
                MaxCapacity = dto.MaxCapacity
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden: Organiser access required." });
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}