// dotnet new mvc -au Individual
// dotnet add package Microsoft.EntityFrameworkCore.SqlServer
// dotnet ef migrations remove
// dotnet ef migrations add InitialCreate -o Data/Migrations
// change db owner (SSMS)
// dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
// dotnet aspnet-codegenerator controller -name MeasuressController -outDir Controllers -namespace AspNet.Controllers
// dotnet aspnet-codegenerator view Add Empty -outDir Views\Results  

// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// var connectionString = builder.Configuration.GetConnectionString("Delivered") ?? throw new InvalidOperationException("Connection string 'Delivered' not found.");
// builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();

// builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.UseMigrationsEndPoint();
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// app.MapRazorPages();

app.Run();
