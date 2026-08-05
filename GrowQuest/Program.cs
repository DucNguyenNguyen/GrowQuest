using GrowQuest.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add MVC services
builder.Services.AddControllersWithViews();


// Add GrowQuest database
builder.Services.AddDbContext<GrowQuestDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


var app = builder.Build();


// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();


// Serve static files
app.MapStaticAssets();


// Make GrowQuest dashboard the default page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Missions}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();