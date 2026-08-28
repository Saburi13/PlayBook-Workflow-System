
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlayBook.Infrastructure.Data;
using PlayBook.Infrastructure.Extensions;
using PlayBook.Infrastructure.Workflows;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    "Server=(localdb)\\mssqllocaldb;Database=PlayBookDb;Trusted_Connection=True;MultipleActiveResultSets=true";
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase") ||
    connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfrastructure(connectionString, useInMemoryDatabase);
builder.Services.Configure<RenewalSchedulerOptions>(builder.Configuration.GetSection("RenewalScheduler"));
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PlayBookSystem",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PlayBookClients",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "PlayBookSuperSecretKeyForLocalDevelopment123!"))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<PlayBookDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseCors("DefaultCors");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlayBookDbContext>();

    if (useInMemoryDatabase)
    {
        db.Database.EnsureCreated();
    }
    else
    {
        db.Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        DevelopmentDataSeeder.SeedAsync(db).GetAwaiter().GetResult();
    }
}

app.Run();

