using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Paytak.Data;
using Paytak.Models;
using Paytak.Services;

// .env dosyasını yükle (proje klasörü veya çalışma dizini)
var envPaths = new[]
{
    Path.Combine(AppContext.BaseDirectory, ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env"), // bin/Debug/net9.0 -> proje klasörü (Paytak)
};
foreach (var envPath in envPaths)
{
    var fullPath = Path.GetFullPath(envPath);
    if (File.Exists(fullPath))
    {
        DotNetEnv.Env.Load(fullPath);
        break;
    }
}
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONNECTION_STRING")))
    DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("CONNECTION_STRING bulunamadı. .env dosyasının Paytak klasöründe olduğundan ve CONNECTION_STRING tanımlı olduğundan emin olun.");

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

// Add OpenAI Service
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

// Mevzuat servisi (Word dokümanlarını okur)
builder.Services.AddSingleton<IMevzuatService, MevzuatService>();

// Add HttpClient
builder.Services.AddHttpClient();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "app",
    pattern: "App",
    defaults: new { controller = "Home", action = "App" });

app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { controller = "Account" });

app.MapControllerRoute(
    name: "chat",
    pattern: "Chat/{action=Index}/{id?}",
    defaults: new { controller = "Chat" });

// API routes
app.MapControllers();

app.Run();
