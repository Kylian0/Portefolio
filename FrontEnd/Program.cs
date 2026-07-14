using FrontEnd;
using FrontEnd.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<IProjectApiService, ProjectApiService>();
builder.Services.AddScoped<IProjectDocumentationService, ProjectDocumentationService>();
builder.Services.AddScoped<IMediaApiService, MediaApiService>();
builder.Services.AddScoped<IProjectTechnologyApiService, ProjectTechnologyApiService>();
builder.Services.AddScoped<IProjectLearningApiService, ProjectLearningApiService>();
builder.Services.AddScoped<DocumentBlockHtmlConverter>();
builder.Services.AddScoped<IAdminAccessService, DemoAdminAccessService>();
builder.Services.AddScoped<IAdminProjectService, DemoAdminProjectService>();
builder.Services.AddScoped<IAdminMessageService, DemoAdminMessageService>();

await builder.Build().RunAsync();
