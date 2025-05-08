using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ECommerceAPIProject.Models;
using ECommerceAPIProject.EntityFrameworkCore;
using ECommerceAPIProject.EntityFrameworkCore.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Database Seeding
await SeedDatabaseAsync(app);

app.Run();

async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<AppDbContext>();

        await CreateRolesAsync(roleManager);
        await CreateAdminUserAsync(userManager);
        await CreateVisitorUserAsync(userManager);

        // Seed Products
        await SeedProductsAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}
async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Admin", "Visitor" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager)
{
    string adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FullName = "Admin User"
        };

        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
async Task CreateVisitorUserAsync(UserManager<ApplicationUser> userManager)
{
    string visitorEmail = "visitor@example.com";
    var visitorUser = await userManager.FindByEmailAsync(visitorEmail);

    if (visitorUser == null)
    {
        visitorUser = new ApplicationUser
        {
            UserName = "visitor",
            Email = visitorEmail,
            FullName = "Regular Visitor"
        };

        await userManager.CreateAsync(visitorUser, "Visitor@123");
        await userManager.AddToRoleAsync(visitorUser, "Visitor");
    }
}
async Task SeedProductsAsync(AppDbContext context)
{
    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product { ArabicName = "منتج 1", EnglishName = "Product 1", Price = 55 },
            new Product { ArabicName = "منتج 2", EnglishName = "Product 2", Price = 89 },
            new Product { ArabicName = "منتج 3", EnglishName = "Product 3", Price = 10 }
        );
        await context.SaveChangesAsync();
    }
}