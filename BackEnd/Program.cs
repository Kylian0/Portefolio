using BackEnd.Models;
using BackEnd.Services;
using BackEnd.Options;
using Microsoft.Extensions.FileProviders;
using BackEnd.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
var maxMediaRequestSize = builder.Configuration.GetValue<long>("MediaStorage:MaxRequestSizeBytes", 104_857_600);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<AdminWriteAuthorizationFilter>());
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = maxMediaRequestSize);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
    options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
}).AddBearerToken(IdentityConstants.BearerScheme, options => options.BearerTokenExpiration = TimeSpan.FromHours(1));
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("admin-login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.AddPolicy("contact-form", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(30),
            QueueLimit = 0
        }));
});
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
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    options.IterationCount = 210_000;
});
builder.Services.AddScoped<IUserStore<AdminUser>, MySqlAdminUserStore>();
builder.Services.AddSingleton<AdminIdentityInitializer>();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length > 0)
{
    if (allowedOrigins.Any(origin =>
            origin == "*" ||
            !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
    {
        throw new InvalidOperationException(
            "Chaque origine CORS doit être une URL HTTP(S) absolue. L'origine générique '*' n'est pas autorisée.");
    }

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontEnd", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}
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
builder.Services.AddScoped<ContactMessage>();
builder.Services.AddSingleton<HtmlDocumentBlockConverter>();
builder.Services.Configure<MediaStorageOptions>(builder.Configuration.GetSection(MediaStorageOptions.SectionName));
builder.Services.AddScoped<MediaFileStorageService>();
builder.Services.AddScoped<MediaReferenceService>();

var app = builder.Build();
await app.Services.GetRequiredService<AdminIdentityInitializer>().InitializeAsync();
await using (var scope = app.Services.CreateAsyncScope()) await scope.ServiceProvider.GetRequiredService<ContactMessage>().EnsureTableAsync();
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

app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path.Equals("/api/media/upload"))
    {
        var requestSizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (requestSizeFeature is { IsReadOnly: false }) requestSizeFeature.MaxRequestBodySize = maxMediaRequestSize;
    }

    await next(context);
});
app.UseRouting();
if (allowedOrigins.Length > 0)
{
    app.UseCors("FrontEnd");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRoot),
    RequestPath = "/" + mediaOptions.PublicPath.Trim('/'),
    OnPrepareResponse = context => context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff"
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
