using BackEnd.Models;
using BackEnd.Services;
using BackEnd.Options;
using Microsoft.Extensions.FileProviders;
using BackEnd.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<AdminWriteAuthorizationFilter>());
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
    options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
}).AddBearerToken(IdentityConstants.BearerScheme, options => options.BearerTokenExpiration = TimeSpan.FromHours(8));
builder.Services.AddAuthorization();
builder.Services.AddIdentityCore<AdminUser>(options =>
{
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
}).AddSignInManager();
builder.Services.AddScoped<IUserStore<AdminUser>, MySqlAdminUserStore>();
builder.Services.AddSingleton<AdminIdentityInitializer>();
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
await app.Services.GetRequiredService<AdminIdentityInitializer>().InitializeAsync();
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
