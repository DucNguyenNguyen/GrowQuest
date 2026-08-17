using GrowQuest.Data;
using GrowQuest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);


// =========================================
// MVC
// =========================================

builder.Services.AddControllersWithViews();


// =========================================
// DATABASE
// =========================================

builder.Services.AddDbContext<GrowQuestDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));


// =========================================
// ASP.NET CORE IDENTITY
// =========================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            // Require each account to use
            // a unique email address
            options.User.RequireUniqueEmail = true;

            // Simple but reasonable requirements
            // for this class project
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
        })
    .AddEntityFrameworkStores<GrowQuestDbContext>()
    .AddDefaultTokenProviders();


// Where unauthenticated users are sent
builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath =
            "/Account/Login";

        options.AccessDeniedPath =
            "/Account/Login";
    });


var app = builder.Build();


// =========================================
// ERROR HANDLING
// =========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();


// =========================================
// STATIC FILES
// =========================================

app.MapStaticAssets();


// =========================================
// AUTHENTICATION
// =========================================

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


// =========================================
// DEFAULT ROUTE
// =========================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Missions}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();