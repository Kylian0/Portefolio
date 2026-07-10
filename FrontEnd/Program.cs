using FrontEnd;
using FrontEnd.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IAdminAccessService, DemoAdminAccessService>();
builder.Services.AddScoped<IAdminProjectService, DemoAdminProjectService>();
builder.Services.AddScoped<IAdminMessageService, DemoAdminMessageService>();

await builder.Build().RunAsync();
