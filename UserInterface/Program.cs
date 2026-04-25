using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Infra.Context;
using Auth.Domain;
using HealthChecks.UI.Client;
using Infra.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using UserInterface.ExceptionHandler;
using UserInterface.Swagger;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Logs
builder.Logging.ClearProviders();
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/logs.txt", 
        rollingInterval: RollingInterval.Day, 
        retainedFileCountLimit: 7, 
        fileSizeLimitBytes: 10_000_000, // 10 MB
        rollOnFileSizeLimit: true)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

//Environment
builder.Configuration.AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Support for requests with enum description
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});;

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString,
        assembly => assembly.MigrationsAssembly("Infra")));

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization();

//Exception Handler
builder.Services.AddExceptionHandler<AppExceptionHandler>();

//HealthChecks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "Postgres Check", tags: new string[] { "db", "data" });

builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

//Cors
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"] ?? "";
var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options => options.AddDefaultPolicy(builder =>
{
    builder.WithOrigins(origins.Length > 0 ? origins : ["http://localhost:3000"])
        .AllowAnyMethod()
        .WithHeaders(["Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin", "Cookie"])
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromHours(1));
}));

//Assembly Extensions for dependency injection
builder.Services.AddRepositories();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "System Template Back"));
}

app.MapGroup("/auth").MapIdentityApi<IdentityUser>();

//Health Check
app.UseHealthChecks("/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecksUI(options =>
{
    options.UIPath = "/healthDashboard";
});

//Exception Handler
app.UseExceptionHandler(_ => { });

// Seed Database
await DatabaseSeeder.SeedAsync(app.Services);

//Cors
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();