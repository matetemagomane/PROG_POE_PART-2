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
    public class EventsControllerTests
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

        private EventsController GetControllerWithSession(ApplicationDbContext context, string? role = null, int? userId = null)
        {
            var controller = new EventsController(context);
            var session = new TestSession();
            if (role != null) session.SetString("UserRole", role);
            if (userId.HasValue) session.SetInt32("UserId", userId.Value);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session }
            };
            return controller;
        }

        [Fact]
        public async Task GetEvents_ReturnsAllEvents()
        {
            var context = GetDatabaseContext();
            context.Events.Add(new Event { Title = "Marathon", EventType = "Run", Location = "Pretoria", EventDate = DateTime.Now });
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context);
            var result = await controller.GetEvents();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetEvent_ExistingId_ReturnsEvent()
        {
            var context = GetDatabaseContext();
            var ev = new Event { Title = "Cycle Tour", EventType = "Cycle", Location = "JHB", EventDate = DateTime.Now };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context);
            var result = await controller.GetEvent(ev.EventId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetEvent_InvalidId_ReturnsNotFound()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);
            var result = await controller.GetEvent(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Organiser", userId: 1);
            var dto = new CreateEventDto { Title = "Fun Run", EventType = "Run", Location = "Park", EventDate = DateTime.Now };

            var result = await controller.CreateEvent(dto);
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_Returns403Forbidden()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Participant", userId: 2);
            var dto = new CreateEventDto { Title = "Forbidden Event", EventType = "Run", Location = "Park", EventDate = DateTime.Now };

            var result = await controller.CreateEvent(dto) as ObjectResult;
            Assert.Equal(403, result?.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_Unauthenticated_Returns403Forbidden()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);
            var dto = new CreateEventDto { Title = "No Auth Event", EventType = "Run", Location = "Park", EventDate = DateTime.Now };

            var result = await controller.CreateEvent(dto) as ObjectResult;
            Assert.Equal(403, result?.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_AsOrganiser_UpdatesData()
        {
            var context = GetDatabaseContext();
            var ev = new Event { Title = "Old Title", EventType = "Run", Location = "Loc", EventDate = DateTime.Now };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Organiser", userId: 1);
            var dto = new CreateEventDto { Title = "New Title", EventType = "Run", Location = "Loc", EventDate = DateTime.Now };

            var result = await controller.UpdateEvent(ev.EventId, dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateEvent_AsParticipant_Returns403Forbidden()
        {
            var context = GetDatabaseContext();
            var ev = new Event { Title = "Title", EventType = "Run", Location = "Loc", EventDate = DateTime.Now };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Participant", userId: 2);
            var dto = new CreateEventDto { Title = "Hacked Title", EventType = "Run", Location = "Loc", EventDate = DateTime.Now };

            var result = await controller.UpdateEvent(ev.EventId, dto) as ObjectResult;
            Assert.Equal(403, result?.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_AsOrganiser_RemovesEvent()
        {
            var context = GetDatabaseContext();
            var ev = new Event { Title = "Delete Me", EventType = "Run", Location = "Loc", EventDate = DateTime.Now };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Organiser", userId: 1);
            var result = await controller.DeleteEvent(ev.EventId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_AsParticipant_Returns403Forbidden()
        {
            var context = GetDatabaseContext();
            var ev = new Event { Title = "Protected Event", EventType = "Run", Location = "Loc", EventDate = DateTime.Now };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Participant", userId: 2);
            var result = await controller.DeleteEvent(ev.EventId) as ObjectResult;

            Assert.Equal(403, result?.StatusCode);
        }
    }
}