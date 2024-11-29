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

// Rejestracja us³ug HTTP
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAntDesign();

// Rejestracja innych us³ug
builder.Services.AddSingleton<Utilities.ILocalStorage, Utilities.LocalStorage>();
builder.Services.AddSingleton<AppState>();
builder.Services.AddSingleton<DialogService>();

// Rejestracja serwisów PDF
builder.Services.AddScoped<PdfReaderService>();
builder.Services.AddScoped<PdfReaderServiceRys>();

// Rejestracja parserów PDF
builder.Services.AddScoped<PdfDataParser>();
builder.Services.AddScoped<PdfDataParserSzyby>();
builder.Services.AddScoped<PdfDataParserRys>();
builder.Services.AddScoped<PdfDataParserElementy>();

builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("utf-8"); // Dodajemy kodowanie utf-8
    return httpClient;
});

// Ustawienie kodowania JSON
builder.Services.AddSingleton<HttpClient>(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// W³¹czenie Invariant Globalization
AppContext.SetSwitch("System.Globalization.Invariant", true);

// Uruchomienie aplikacji
await builder.Build().RunAsync();
