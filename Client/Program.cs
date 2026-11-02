using GEORGE.Client;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Client.Pages.Okna;
using GEORGE.Client.Pages.PDF;
using GEORGE.Client.Pages.Utils;                    // ⭐ dla GeometryUtils
using GEORGE.Client.Services;
using GEORGE.Shared.Class;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

var culture = new CultureInfo("pl-PL");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ⭐ Konfiguracja logowania (WYMAGA pakietów: Logging.Console + Logging.Configuration)
// ⭐ Konfiguracja logowania Blazor WebAssembly
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

// Filtry kategorii
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("AntDesign", LogLevel.Warning);

// Logi własnego projektu
builder.Logging.AddFilter("GEORGE", LogLevel.Debug);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ✅ Jeden HttpClient – bez AddApiAuthorization
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// AntDesign
builder.Services.AddAntDesign();

// Twoje serwisy
builder.Services.AddSingleton<Utilities.ILocalStorage, Utilities.LocalStorage>();
builder.Services.AddSingleton<AppState>();
builder.Services.AddSingleton<DialogService>();

builder.Services.AddScoped<PdfReaderService>();
builder.Services.AddScoped<PdfReaderServiceRys>();

builder.Services.AddScoped<PdfDataParser>();
builder.Services.AddScoped<PdfDataParserSzyby>();
builder.Services.AddScoped<PdfDataParserRys>();
builder.Services.AddScoped<PdfDataParserElementy>();

builder.Services.AddScoped<Generator>();

builder.Services.AddSingleton<DxfService>();
builder.Services.AddSingleton<DxfToSvgConverter>();
builder.Services.AddScoped<ImageGenerator>();
builder.Services.AddSingleton<ShapeTransferService>();
builder.Services.AddSingleton<GeneratorStateContainer>();

builder.Services.AddScoped<FileClipboardService>();

builder.Services.AddScoped<ElementLiniowyService>();

builder.Services.AddHighlightJS();

// Globalizacja
AppContext.SetSwitch("System.Globalization.Invariant", true);

// ⭐ Build aplikacji (musi być PRZED Initialize)
var app = builder.Build();

// ⭐ Inicjalizacja static loggera dla GeometryUtils
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
GeometryUtils.Initialize(loggerFactory);
Generator.Initialize(loggerFactory);
LineUtils.Initialize(loggerFactory);


// ⭐ Start
await app.RunAsync();