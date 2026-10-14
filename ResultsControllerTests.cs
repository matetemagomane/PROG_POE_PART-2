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
    public class ResultsControllerTests
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

        private ResultsController GetControllerWithSession(ApplicationDbContext context, string? role = null, int? userId = null)
        {
            var controller = new ResultsController(context);
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
        public async Task PostResult_AsOrganiser_CreatesResult()
        {
            var context = GetDatabaseContext();
            context.Enrolments.Add(new Enrolment { EnrolmentId = 1, UserId = 1, CategoryId = 1 });
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context, role: "Organiser", userId: 2);
            var dto = new CreateResultDto { EnrolmentId = 1, FinishTime = "00:45:10", TotalSeconds = 2710, OverallPosition = 1, CategoryPosition = 1 };

            var result = await controller.PostResult(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task PostResult_InvalidEnrolmentId_ReturnsBadRequest()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Organiser", userId: 2);
            var dto = new CreateResultDto { EnrolmentId = 999, FinishTime = "00:45:10", TotalSeconds = 2710, OverallPosition = 1, CategoryPosition = 1 };

            var result = await controller.PostResult(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PostResult_AsParticipant_Returns403Forbidden()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Participant", userId: 1);
            var dto = new CreateResultDto { EnrolmentId = 1, FinishTime = "00:45:10", TotalSeconds = 2710, OverallPosition = 1, CategoryPosition = 1 };

            var result = await controller.PostResult(dto) as ObjectResult;
            Assert.Equal(403, result?.StatusCode);
        }

        [Fact]
        public async Task GetMyResults_Authenticated_ReturnsResults()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context, role: "Participant", userId: 1);

            var result = await controller.GetMyResults();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMyResults_Unauthenticated_ReturnsUnauthorized()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);

            var result = await controller.GetMyResults();
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetEventResults_PublicAccess_ReturnsOk()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);

            var result = await controller.GetEventResults(1);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}