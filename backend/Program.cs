using System.Text;
using Backend.Data;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<LockoutSettings>(builder.Configuration.GetSection("LockoutSettings"));
builder.Services.Configure<PasswordResetSettings>(builder.Configuration.GetSection("PasswordResetSettings"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(BuildConnectionString(builder.Configuration)));

var jwtSecret = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey not configured");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JwtSettings:Issuer not configured");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var cookieToken = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrWhiteSpace(cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ILockoutService, LockoutService>();
builder.Services.AddSingleton<IPasswordResetService, PasswordResetService>();
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedDemoDataAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string BuildConnectionString(IConfiguration configuration)
{
    var configuredConnection = configuration.GetConnectionString("DefaultConnection");
    var connectionBuilder = string.IsNullOrWhiteSpace(configuredConnection) || configuredConnection.Contains("<POSTGRES_CONNECTION_STRING_PLACEHOLDER>", StringComparison.Ordinal)
        ? new NpgsqlConnectionStringBuilder()
        : new NpgsqlConnectionStringBuilder(configuredConnection);

    var database = Environment.GetEnvironmentVariable("DB_NAME");
    var username = Environment.GetEnvironmentVariable("DB_USERNAME");
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var host = Environment.GetEnvironmentVariable("DB_HOST");
    var portValue = Environment.GetEnvironmentVariable("DB_PORT");

    if (!string.IsNullOrWhiteSpace(host))
    {
        connectionBuilder.Host = host;
    }
    else if (string.IsNullOrWhiteSpace(connectionBuilder.Host))
    {
        connectionBuilder.Host = "localhost";
    }

    if (int.TryParse(portValue, out var port))
    {
        connectionBuilder.Port = port;
    }
    else if (connectionBuilder.Port == 0)
    {
        connectionBuilder.Port = 5432;
    }

    if (!string.IsNullOrWhiteSpace(database))
    {
        connectionBuilder.Database = database;
    }

    if (!string.IsNullOrWhiteSpace(username))
    {
        connectionBuilder.Username = username;
    }

    if (!string.IsNullOrWhiteSpace(password))
    {
        connectionBuilder.Password = password;
    }

    return connectionBuilder.ConnectionString;
}

static async Task SeedDemoDataAsync(AppDbContext db)
{
    var roleNames = new[] { "Admin", "Receptionist", "Security Guard" };
    var existingRoles = await db.Roles
        .Where(role => roleNames.Contains(role.Name))
        .ToDictionaryAsync(role => role.Name, StringComparer.OrdinalIgnoreCase);

    var addedRoles = false;

    foreach (var roleName in roleNames)
    {
        if (existingRoles.ContainsKey(roleName))
        {
            continue;
        }

        var role = new Role
        {
            Name = roleName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Roles.Add(role);
        existingRoles[roleName] = role;
        addedRoles = true;
    }

    if (addedRoles)
    {
        await db.SaveChangesAsync();
    }

    var demoUsers = new[]
    {
        new { Email = "admin@vms.local", Role = "Admin", EmployeeId = "u-admin-001" },
        new { Email = "reception@vms.local", Role = "Receptionist", EmployeeId = "u-reception-001" },
        new { Email = "guard@vms.local", Role = "Security Guard", EmployeeId = "u-guard-001" }
    };

    var existingEmails = await db.Users
        .Where(user => user.Email != null && demoUsers.Select(item => item.Email).Contains(user.Email))
        .Select(user => user.Email!)
        .ToListAsync();

    foreach (var demoUser in demoUsers.Where(item => !existingEmails.Contains(item.Email, StringComparer.OrdinalIgnoreCase)))
    {
        db.Users.Add(new User
        {
            Email = demoUser.Email,
            EmployeeId = demoUser.EmployeeId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123", workFactor: 12),
            RoleId = existingRoles[demoUser.Role].Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    await db.SaveChangesAsync();
}