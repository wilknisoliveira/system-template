using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Infra.Context;
using Auth.Domain;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

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

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString,
        assembly => assembly.MigrationsAssembly("Infra")));

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization();

//HealthChecks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "Postgres Check", tags: new string[] { "db", "data" });

builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "System Template Back"));
    
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync(Roles.Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
    }
    if (!await roleManager.RoleExistsAsync(Roles.Member))
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.Member));
    }
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();