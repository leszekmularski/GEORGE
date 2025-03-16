using GEORGE.Server;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using AntDesign;
using ReservationBookingSystem.Services;
using GEORGE.Client.Pages.Administracja;


var builder = WebApplication.CreateBuilder(args);

// 1. Dodanie serwis�w do kontenera
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Konfiguracja Kestrel z ustawieniami z appsettings.json
builder.WebHost.ConfigureKestrel((context, options) =>
{
    var kestrelSection = context.Configuration.GetSection("Kestrel");
    options.Configure(kestrelSection);
});

// Wymuszenie przekierowania na HTTPS
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 5001; // Port HTTPS
});

// Konfiguracja Entity Framework z SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Konfiguracja logowania
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Rejestracja konfiguracji MailSettings z appsettings.json
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));

// Rejestracja MailService jako serwis
builder.Services.AddScoped<IMailService, MailService>();

// Konfiguracja obs�ugi plik�w statycznych (obs�uga specyficznych rozszerze�)
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.ContentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider
    {
        Mappings =
        {
            [".dwg"] = "application/acad",
            [".dxf"] = "application/acad",
            [ ".p" ] = "text/plain",
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors("AllowAll");

// 2. �rodkowe oprogramowanie (Middleware)
// Obs�uga �rodowiska
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Wymuszenie HTTPS
app.UseHttpsRedirection();

// Obs�uga plik�w statycznych
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Routing
app.UseRouting();

// Obs�uga autoryzacji
app.UseAuthorization();

// Mapowanie endpoint�w
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
