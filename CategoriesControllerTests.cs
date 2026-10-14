using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class CategoriesControllerTests
    {
        private ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private CategoriesController GetControllerWithSession(ApplicationDbContext context, string? role = null)
        {
            var controller = new CategoriesController(context);
            var session = new TestSession();
            if (role != null) session.SetString("UserRole", role);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session }
            };
            return controller;
        }

        [Fact]
        public async Task CreateCategory_AsOrganiser_ReturnsOk()
        {
            var context = GetDatabaseContext();
            var ev = new Event { Title = "Event", EventType = "Run", Location = "Loc", EventDate = DateTime.Now };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Organiser");
            var dto = new CreateCategoryDto { EventId = ev.EventId, Name = "10km Dash", DistanceKm = 10, EntryFee = 100, MaxCapacity = 500 };

            var result = await controller.CreateCategory(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateCategory_InvalidEventId_ReturnsBadRequest()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Organiser");
            var dto = new CreateCategoryDto { EventId = 999, Name = "10km Dash", DistanceKm = 10, EntryFee = 100, MaxCapacity = 500 };

            var result = await controller.CreateCategory(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateCategory_AsParticipant_Returns403Forbidden()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Participant");
            var dto = new CreateCategoryDto { EventId = 1, Name = "10km", DistanceKm = 10, EntryFee = 50, MaxCapacity = 100 };

            var result = await controller.CreateCategory(dto) as ObjectResult;
            Assert.Equal(403, result?.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_AsOrganiser_RemovesCategory()
        {
            var context = GetDatabaseContext();
            var cat = new Category { EventId = 1, Name = "5km Walk", DistanceKm = 5, EntryFee = 50, MaxCapacity = 200 };
            context.Categories.Add(cat);
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Organiser");
            var result = await controller.DeleteCategory(cat.CategoryId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCategory_InvalidId_ReturnsNotFound()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Organiser");
            var result = await controller.DeleteCategory(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCategory_AsParticipant_Returns403Forbidden()
        {
            var context = GetDatabaseContext();
            var cat = new Category { EventId = 1, Name = "5km", DistanceKm = 5, EntryFee = 50, MaxCapacity = 100 };
            context.Categories.Add(cat);
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Participant");
            var result = await controller.DeleteCategory(cat.CategoryId) as ObjectResult;

            Assert.Equal(403, result?.StatusCode);
        }
    }
}