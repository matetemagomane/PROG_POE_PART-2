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
    public class EnrolmentsControllerTests
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

        private EnrolmentsController GetControllerWithSession(ApplicationDbContext context, string? role = null, int? userId = null)
        {
            var controller = new EnrolmentsController(context);
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
        public async Task Enrol_AsParticipant_CreatesEnrolment()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Participant", userId: 1);
            var dto = new CreateEnrolmentDto { CategoryId = 10 };

            var result = await controller.Enrol(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Enrol_DuplicateCategory_ReturnsBadRequest()
        {
            var context = GetDatabaseContext();
            context.Enrolments.Add(new Enrolment { UserId = 1, CategoryId = 10 });
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Participant", userId: 1);
            var dto = new CreateEnrolmentDto { CategoryId = 10 };

            var result = await controller.Enrol(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Enrol_AsOrganiser_Returns403Forbidden()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Organiser", userId: 2);
            var dto = new CreateEnrolmentDto { CategoryId = 10 };

            var result = await controller.Enrol(dto) as ObjectResult;
            Assert.Equal(403, result?.StatusCode);
        }

        [Fact]
        public async Task GetMyEnrolments_Authenticated_ReturnsList()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Participant", userId: 1);

            var result = await controller.GetMyEnrolments();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMyEnrolments_Unauthenticated_ReturnsUnauthorized()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);

            var result = await controller.GetMyEnrolments();
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetEventEnrolments_AsOrganiser_ReturnsEnrolments()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Organiser", userId: 1);

            var result = await controller.GetEventEnrolments(1);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}