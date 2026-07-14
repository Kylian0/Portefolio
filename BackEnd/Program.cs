using BackEnd.Models;
using BackEnd.Services;

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

var app = builder.Build();

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
app.UseAuthorization();

app.MapControllers();

app.Run();
