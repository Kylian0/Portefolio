using BackEnd.Models;
using BackEnd.Services;
using BackEnd.Options;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEnd", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Portfolio API",
        Version = "v1",
        Description = "API du portfolio de Kylian."
    });
});
builder.Services.AddScoped<Project>();
builder.Services.AddScoped<ProjectDocument>();
builder.Services.AddScoped<ProjectDocumentBlock>();
builder.Services.AddScoped<Media>();
builder.Services.AddScoped<TechnologyCategory>();
builder.Services.AddScoped<Technology>();
builder.Services.AddScoped<ProjectTechnology>();
builder.Services.AddScoped<ProjectLearning>();
builder.Services.AddSingleton<HtmlDocumentBlockConverter>();
builder.Services.Configure<MediaStorageOptions>(builder.Configuration.GetSection(MediaStorageOptions.SectionName));
builder.Services.AddScoped<MediaFileStorageService>();
builder.Services.AddScoped<MediaReferenceService>();

var app = builder.Build();
var mediaOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<MediaStorageOptions>>().Value;
var mediaRoot = Path.GetFullPath(Path.IsPathRooted(mediaOptions.RootPath) ? mediaOptions.RootPath : Path.Combine(app.Environment.ContentRootPath, mediaOptions.RootPath));
Directory.CreateDirectory(mediaRoot);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Portfolio API";
    });

    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontEnd");
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(mediaRoot), RequestPath = "/" + mediaOptions.PublicPath.Trim('/') });
app.UseAuthorization();

app.MapControllers();

app.Run();
