using WebTrafficBuilder.Models;
using WebTrafficBuilder.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDBSetting>(builder.Configuration.GetSection("MongoDB")); //appsettingsle eþleþir parametre
builder.Services.AddSingleton<MongoDBService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
