using ECommerceAPIProject.Controllers;
using ECommerceAPIProject.EntityFrameworkCore;
using ECommerceAPIProject.EntityFrameworkCore.Entities;
using ECommerceAPIProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ECommerceAPIProject.Tests
{
    public class ProductsControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IAuthorizationService> _authService = new();

        public ProductsControllerTests()
        {
            // Configure in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        private ProductsController CreateController(bool isAdmin = false)
        {
            // Create mock authorization service
            _authService.Setup(a =>
                a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<string>()))
                .ReturnsAsync(isAdmin ? AuthorizationResult.Success() : AuthorizationResult.Failed());

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                isAdmin ? new[] { new Claim(ClaimTypes.Role, "Admin") } : Array.Empty<Claim>(),
                "TestAuth"));

            return new ProductsController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = user,
                        RequestServices = new ServiceCollection()
                            .AddSingleton(_authService.Object)
                            .BuildServiceProvider()
                    }
                }
            };
        }

        [Fact]
        public async Task CreateProduct_AdminRole_ReturnsOk()
        {
            // Arrange
            var controller = CreateController(isAdmin: true);
            var validProduct = new Product
            {
                ArabicName = "منتج تجريبي",
                EnglishName = "Test Product",
                Price = 100.00m
            };

            // Act
            var result = await controller.CreateProduct(validProduct); 

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Single(_context.Products);
        }
        [Fact]
        public async Task CreateProduct_InvalidData_ReturnsBadRequest()
        {
            var controller = CreateController(isAdmin: true);
            var invalidProduct = new Product(); 

            var result = await controller.CreateProduct(invalidProduct);

            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task CreateProduct_NonAdmin_ReturnsForbidden()
        {
            // Arrange
            var controller = CreateController();
            var validProduct = new Product
            {
                ArabicName = "منتج",
                EnglishName = "Product",
                Price = 100
            };

            // Act
            var result = await controller.CreateProduct(validProduct);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Single(_context.Products);
        }

        [Fact]
        public async Task CreateProduct_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController(isAdmin: true);
            controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = await controller.CreateProduct(new Product());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SoftDeleteProduct_AdminRole_Success()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                ArabicName = "منتج اختبار",
                EnglishName = "Test Product",
                Price = 100
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var controller = CreateController(isAdmin: true);

            // Act
            var result = await controller.SoftDeleteProduct(1);

            // Assert
            Assert.IsType<OkResult>(result); // Changed to OkResult
            var deletedProduct = await _context.Products.FindAsync(1);
            Assert.True(deletedProduct?.IsDeleted);
        }
    }
}