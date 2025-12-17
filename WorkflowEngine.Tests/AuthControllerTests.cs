using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WorkflowEngine.API.Controllers;
using WorkflowEngine.Core.DTOs;
using WorkflowEngine.Core.Entities;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Infrastructure.Data;
using Xunit;

namespace WorkflowEngine.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_WithCorrectPassword_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new WebUser
            {
                Username = "testuser",
                PasswordHash = hashedPassword,
                IsActive = true,
                Email = "test@example.com",
                Role = "User"
            };

            context.WebUsers.Add(user);
            await context.SaveChangesAsync();

            var mockJwtGenerator = new Mock<IJwtTokenGenerator>();
            mockJwtGenerator.Setup(x => x.GenerateToken(It.IsAny<WebUser>())).Returns("fake-token");

            var controller = new AuthController(context, mockJwtGenerator.Object);
            var request = new LoginRequest { Username = "testuser", Password = password };

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
        {
             // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new WebUser
            {
                Username = "testuser",
                PasswordHash = hashedPassword,
                IsActive = true,
                Email = "test@example.com",
                Role = "User"
            };

            context.WebUsers.Add(user);
            await context.SaveChangesAsync();

            var mockJwtGenerator = new Mock<IJwtTokenGenerator>();

            var controller = new AuthController(context, mockJwtGenerator.Object);
            var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };

            // Act
            var result = await controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid credentials.", unauthorizedResult.Value);
        }
    }
}
