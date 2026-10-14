using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostResult([FromBody] CreateResultDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser")
            {
                return StatusCode(403, new { message = "Forbidden: Organiser access required." });
            }

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                TotalSeconds = dto.TotalSeconds,
                OverallPosition = dto.OverallPosition,
                CategoryPosition = dto.CategoryPosition
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(result);
        }

        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return Unauthorized();

            var results = await _context.Results
                .Where(r => r.Enrolment.UserId == userId.Value)
                .Select(r => new
                {
                    r.ResultId,
                    r.FinishTime,
                    r.OverallPosition,
                    r.CategoryPosition,
                    EventTitle = r.Enrolment.Category.Event.Title,
                    CategoryName = r.Enrolment.Category.Name
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}