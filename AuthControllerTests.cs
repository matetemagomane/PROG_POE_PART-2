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
    public class AuthControllerTests
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

        private AuthController GetControllerWithSession(ApplicationDbContext context)
        {
            var controller = new AuthController(context);
            var httpContext = new DefaultHttpContext { Session = new TestSession() };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        [Fact]
        public async Task Register_Participant_CreatesUser()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);
            var dto = new RegisterDto { FullName = "Alice", Email = "alice@test.com", Password = "Pass123!", Role = "Participant" };

            var result = await controller.Register(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Register_Organiser_CreatesUserAndOrganiserProfile()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);
            var dto = new RegisterDto { FullName = "Bob", Email = "bob@test.com", Password = "Pass123!", Role = "Organiser", OrganizationName = "Run Club" };

            var result = await controller.Register(dto);
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(await context.EventOrganisers.FirstOrDefaultAsync());
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var context = GetDatabaseContext();
            context.Users.Add(new User { FullName = "Alice", Email = "alice@test.com", PasswordHash = "hash", Role = "Participant" });
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context);
            var dto = new RegisterDto { FullName = "Alice Two", Email = "alice@test.com", Password = "Pass123!", Role = "Participant" };

            var result = await controller.Register(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);
            var dto = new RegisterDto { FullName = "Admin", Email = "admin@test.com", Password = "Pass123!", Role = "SuperAdmin" };

            var result = await controller.Register(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var context = GetDatabaseContext();
            string pass = "Password123!";
            context.Users.Add(new User { FullName = "Alice", Email = "alice@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass), Role = "Participant" });
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context);
            var result = await controller.Login(new LoginDto { Email = "alice@test.com", Password = pass });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var context = GetDatabaseContext();
            context.Users.Add(new User { FullName = "Alice", Email = "alice@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword"), Role = "Participant" });
            await context.SaveChangesAsync();

            var controller = GetControllerWithSession(context);
            var result = await controller.Login(new LoginDto { Email = "alice@test.com", Password = "WrongPassword" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_NonExistentUser_ReturnsUnauthorized()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);
            var result = await controller.Login(new LoginDto { Email = "nobody@test.com", Password = "Password" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void Logout_ClearsSession_ReturnsOk()
        {
            var context = GetDatabaseContext();
            var controller = GetControllerWithSession(context);
            var result = controller.Logout();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}