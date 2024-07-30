using GEORGE.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AntDesign;
using GEORGE.Client.Pages.PDF;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAntDesign();

builder.Services.AddScoped<PdfReaderService>();
builder.Services.AddScoped<PdfDataParser>();

await builder.Build().RunAsync();
