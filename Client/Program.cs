using GEORGE.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AntDesign;
using GEORGE.Client.Pages.PDF;
using System.Net;
using netDxf;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<HttpClient>(s => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAntDesign();

builder.Services.AddSingleton<Utilities.ILocalStorage, Utilities.LocalStorage>();

builder.Services.AddSingleton<AppState>();
builder.Services.AddSingleton<DialogService>();


builder.Services.AddScoped<PdfReaderService>();

builder.Services.AddScoped<PdfDataParser>();
builder.Services.AddScoped<PdfDataParserSzyby>();

builder.Services.AddScoped<PdfDataParserElementy>();
builder.Services.AddScoped<PdfDataParserElementy>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://twoj-serwer-api.com/") });


// W³¹czenie Invariant Globalization
AppContext.SetSwitch("System.Globalization.Invariant", true);

await builder.Build().RunAsync();

// Konfiguracja niestandardowej weryfikacji certyfikatu
ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

await builder.Build().RunAsync();

