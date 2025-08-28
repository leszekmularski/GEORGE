using GEORGE.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AntDesign;
using System.Globalization;
using GEORGE.Client.Pages.PDF;
using GEORGE.Client.Pages.KonfiguratorOkien;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

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

builder.Services.AddSingleton<DxfService>();
builder.Services.AddSingleton<DxfToSvgConverter>();
builder.Services.AddScoped<ImageGenerator>();
builder.Services.AddSingleton<ShapeTransferService>();
builder.Services.AddSingleton<GeneratorStateContainer>();

// Globalizacja
AppContext.SetSwitch("System.Globalization.Invariant", true);

// Start
await builder.Build().RunAsync();
