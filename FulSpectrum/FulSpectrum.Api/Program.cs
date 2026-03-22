//using Asp.Versioning;
// using Asp.Versioning.ApiExplorer;
//using FluentValidation;
// using FluentValidation.AspNetCore;
//using FulSpectrum.Api.Auth;
//using FulSpectrum.Api.Middlewares;
//using FulSpectrum.Api.Validators;
//using FulSpectrum.Application.Catalog.Dtos;
//using FulSpectrum.Application.Catalog.Queries;
//using FulSpectrum.Domain.Identity;
//using FulSpectrum.Infrastructure;
//using FulSpectrum.Infrastructure.Persistence;
//using HealthChecks.UI.Client;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.RateLimiting;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using Serilog;
//using Serilog.Events;
//using System.Text;
//using System.Threading.RateLimiting;
//using Swashbuckle.AspNetCore.SwaggerGen;
//using FulSpectrum.Api.Services;
//using Hangfire;
//using Hangfire.MemoryStorage;
//using Asp.Versioning.ApiExplorer;
//using FulSpectrum.Api.Observability;
//using OpenTelemetry.Metrics;
//using OpenTelemetry.Resources;
//using OpenTelemetry.Trace;
//using Serilog.Formatting.Compact;
//using System.Diagnostics;
//using Microsoft.Extensions.Caching.StackExchangeRedis;










//var builder = WebApplication.CreateBuilder(args);

//builder.Services.Configure<TelemetryOptions>(builder.Configuration.GetSection(TelemetryOptions.SectionName));

//var telemetryOptions = builder.Configuration
//    .GetSection(TelemetryOptions.SectionName)
//    .Get<TelemetryOptions>() ?? new TelemetryOptions();
//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
//    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
//    .Enrich.FromLogContext()
//    .Enrich.WithEnvironmentName()
//    .Enrich.WithThreadId()
//   .WriteTo.Console(new RenderedCompactJsonFormatter())
//    .WriteTo.File(new RenderedCompactJsonFormatter(), "logs/log-.json", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Host.UseSerilog();
//builder.Services.AddOpenTelemetry()
//    .ConfigureResource(resource => resource
//        .AddService(telemetryOptions.ServiceName)
//        .AddAttributes(new Dictionary<string, object>
//        {
//            ["deployment.environment"] = builder.Environment.EnvironmentName,
//            ["service.namespace"] = "FulSpectrum"
//        }))
//    .WithTracing(tracing => tracing
//        .AddAspNetCoreInstrumentation(options =>
//        {
//            options.RecordException = true;
//            options.EnrichWithHttpRequest = (activity, request) =>
//            {
//                activity.SetTag("correlation.id", request.HttpContext.TraceIdentifier);
//            };
//        })
//        .AddEntityFrameworkCoreInstrumentation()
//        .AddHttpClientInstrumentation())
//    .WithMetrics(metrics => metrics
//        .AddAspNetCoreInstrumentation()
//        .AddHttpClientInstrumentation()
//        .AddRuntimeInstrumentation()
//        .AddProcessInstrumentation()
//        .AddPrometheusExporter());

//if (telemetryOptions.EnableOtlpExporter && !string.IsNullOrWhiteSpace(telemetryOptions.OtlpEndpoint))
//{
//    builder.Services.AddOpenTelemetry()
//        .WithTracing(tracing => tracing.AddOtlpExporter(options => options.Endpoint = new Uri(telemetryOptions.OtlpEndpoint)))
//        .WithMetrics(metrics => metrics.AddOtlpExporter(options => options.Endpoint = new Uri(telemetryOptions.OtlpEndpoint)));
//}
//builder.Services.AddControllers();
//var redisCs = builder.Configuration.GetConnectionString("Redis");
//if (!string.IsNullOrWhiteSpace(redisCs))
//{
//    builder.Services.AddStackExchangeRedisCache(options =>
//    {
//        options.Configuration = redisCs;
//        options.InstanceName = "fulspectrum:";
//    });
//}
//else
//{
//    builder.Services.AddDistributedMemoryCache();
//}
//builder.Services.AddScoped<ICatalogCacheService, CatalogCacheService>();
//builder.Services.AddResponseCaching(); builder.Services.AddHttpContextAccessor();
//builder.Services.Configure<JwtOptions>(
//    builder.Configuration.GetSection(JwtOptions.SectionName));

//var jwtOptions = builder.Configuration
//    .GetSection(JwtOptions.SectionName)
//    .Get<JwtOptions>() ?? new JwtOptions();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = jwtOptions.Issuer,
//            ValidAudience = jwtOptions.Audience,
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
//            ClockSkew = TimeSpan.FromSeconds(30)
//        };
//    });

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("CanManageCatalog", policy => policy.RequireRole(UserRoles.Admin));
//    options.AddPolicy("CustomerOrAdmin", policy => policy.RequireRole(UserRoles.Customer, UserRoles.Admin));
//});

//builder.Services.AddApiVersioning(options =>
//{
//    options.DefaultApiVersion = new ApiVersion(1, 0);
//    options.AssumeDefaultVersionWhenUnspecified = true;
//    options.ReportApiVersions = true;
//    options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
//})
//.AddApiExplorer(options =>
//{
//    options.GroupNameFormat = "'v'VVV";
//    options.SubstituteApiVersionInUrl = true;
//});
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "FulSpectrum API",
//        Version = "v1"
//    });

//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Ingresá el token JWT. Ejemplo: Bearer eyJhbGciOi..."
//    });

//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("Default", policy =>
//    {
//        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

//        if (origins is { Length: > 0 })
//        {
//            policy.WithOrigins(origins)
//                  .AllowAnyHeader()
//                  .AllowAnyMethod()
//                  .AllowCredentials();
//        }
//        else
//        {
//            policy.AllowAnyOrigin()
//                  .AllowAnyHeader()
//                  .AllowAnyMethod();
//        }
//    });
//});

//builder.Services.AddHealthChecks();

//var hcUiCs = builder.Configuration.GetConnectionString("HealthChecksUI")
//           ?? builder.Configuration.GetConnectionString("DefaultConnection");

//if (!string.IsNullOrWhiteSpace(hcUiCs))
//{
//    builder.Services.AddHealthChecksUI().AddSqlServerStorage(hcUiCs);
//}

//builder.Services.AddInfrastructure(builder.Configuration);
//builder.Services.AddScoped<IEmailNotificationService, LoggingEmailNotificationService>();
//builder.Services.AddScoped<IImageStorageService, LocalImageStorageService>();
//builder.Services.AddHangfire(config => config.UseMemoryStorage());

//builder.Services.AddHangfireServer();
//builder.Services.AddRateLimiter(options =>
//{
//    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
//    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
//        RateLimitPartition.GetFixedWindowLimiter(
//            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
//            factory: _ => new FixedWindowRateLimiterOptions
//            {
//                PermitLimit = 40,
//                Window = TimeSpan.FromMinutes(1),
//                QueueLimit = 0
//            }));
//});

//builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
//builder.Services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
//builder.Services.AddScoped<IValidator<UpdateProductRequest>, UpdateProductRequestValidator>();
//builder.Services.AddScoped<IValidator<ProductListQuery>, ProductListQueryValidator>();
//builder.Services.AddTransient<ExceptionHandlingMiddleware>();
//builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
//     var app = builder.Build();

//Console.WriteLine($"ENVIRONMENT: {app.Environment.EnvironmentName}");
//Console.WriteLine($"IsDevelopment: {app.Environment.IsDevelopment()}");
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<FulSpectrumDbContext>();
//    db.Database.Migrate();

//    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
//    var normalizedAdminEmail = "ADMIN@FULSPECTRUM.LOCAL";

//    if (!db.Users.Any(u => u.NormalizedEmail == normalizedAdminEmail))
//    {
//        var admin = new AppUser
//        {
//            Id = Guid.NewGuid(),
//            Email = "admin@fulspectrum.local",
//            NormalizedEmail = normalizedAdminEmail,
//            FirstName = "System",
//            LastName = "Admin",
//            Role = UserRoles.Admin,
//            CreatedAtUtc = DateTime.UtcNow,
//            IsActive = true
//        };

//        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");
//        db.Users.Add(admin);
//        db.SaveChanges();
//    }
//}

////app.UseSerilogRequestLogging();
//app.UseMiddleware<CorrelationIdMiddleware>();
//app.UseSerilogRequestLogging(options =>
//{
//    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
//    {
//        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
//        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
//        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
//        diagnosticContext.Set("TraceId", Activity.Current?.TraceId.ToString() ?? string.Empty);
//    };
//});
//app.UseMiddleware<RequestLogEnrichmentMiddleware>();
//app.UseMiddleware<ExceptionHandlingMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(o =>
//        o.SwaggerEndpoint("/swagger/v1/swagger.json", "FulSpectrum API v1"));
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles(new StaticFileOptions
//{
//    OnPrepareResponse = context =>
//    {
//        if (context.File.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
//            || context.File.Name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
//            || context.File.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
//            || context.File.Name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
//        {
//            context.Context.Response.Headers.CacheControl = "public,max-age=604800";
//        }
//    }
//}); 
//app.Use(async (ctx, next) =>
//{
//    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
//    ctx.Response.Headers["X-Frame-Options"] = "DENY";
//    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
//    await next();
//});

//app.UseCors("Default");
//app.UseResponseCaching();
//app.UseRateLimiter();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();
//app.MapHangfireDashboard("/jobs");
//app.MapHealthChecks("/health/live", new HealthCheckOptions
//{
//    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//});

//app.MapHealthChecks("/health/ready", new HealthCheckOptions
//{
//    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//});
//app.MapPrometheusScrapingEndpoint("/metrics");
//if (!string.IsNullOrWhiteSpace(hcUiCs))
//{
//    app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
//}

//app.Run();



















using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using FulSpectrum.Api.Auth;
using FulSpectrum.Api.Middlewares;
using FulSpectrum.Api.Observability;
using FulSpectrum.Api.Services;
using FulSpectrum.Api.Validators;
using FulSpectrum.Application.Catalog.Dtos;
using FulSpectrum.Application.Catalog.Queries;
using FulSpectrum.Domain.Identity;
using FulSpectrum.Infrastructure;
using FulSpectrum.Infrastructure.Persistence;
using Hangfire;
using Hangfire.MemoryStorage;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TelemetryOptions>(
    builder.Configuration.GetSection(TelemetryOptions.SectionName));

var telemetryOptions = builder.Configuration
    .GetSection(TelemetryOptions.SectionName)
    .Get<TelemetryOptions>() ?? new TelemetryOptions();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        resource.AddService(
     serviceName: telemetryOptions.ServiceName);
    })
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = (activity, request) =>
            {
                activity.SetTag("correlation.id", request.HttpContext.TraceIdentifier);
            };
        })
        .AddEntityFrameworkCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter());

if (telemetryOptions.EnableOtlpExporter &&
    !string.IsNullOrWhiteSpace(telemetryOptions.OtlpEndpoint))
{
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
            tracing.AddOtlpExporter(options =>
                options.Endpoint = new Uri(telemetryOptions.OtlpEndpoint)))
        .WithMetrics(metrics =>
            metrics.AddOtlpExporter(options =>
                options.Endpoint = new Uri(telemetryOptions.OtlpEndpoint)));
}

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddFluentValidationAutoValidation();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "fulspectrum:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddResponseCaching();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

var hasWeakSecret =
    string.IsNullOrWhiteSpace(jwtOptions.SecretKey) ||
    jwtOptions.SecretKey.Contains("CHANGE_THIS", StringComparison.OrdinalIgnoreCase) ||
    jwtOptions.SecretKey.Length < 32;

if (hasWeakSecret && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "JWT secret is weak or missing. Configure Jwt:SecretKey via environment or user-secrets.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageCatalog",
        policy => policy.RequireRole(UserRoles.Admin));

    options.AddPolicy("CustomerOrAdmin",
        policy => policy.RequireRole(UserRoles.Customer, UserRoles.Admin));
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FulSpectrum API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresá el token JWT así: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

builder.Services.AddHealthChecks();

var healthChecksUiConnectionString =
    builder.Configuration.GetConnectionString("HealthChecksUI") ??
    builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(healthChecksUiConnectionString))
{
    builder.Services.AddHealthChecksUI()
        .AddSqlServerStorage(healthChecksUiConnectionString);
}

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IEmailNotificationService, LoggingEmailNotificationService>();
builder.Services.AddScoped<IImageStorageService, LocalImageStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;

        var partitionKey = isAuthenticated
            ? $"user:{httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown"}"
            : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

        var permitLimit = isAuthenticated ? 120 : 40;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateProductRequest>, UpdateProductRequestValidator>();
builder.Services.AddScoped<IValidator<ProductListQuery>, ProductListQueryValidator>();
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidator>();
builder.Services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();

var app = builder.Build();

Console.WriteLine($"ENVIRONMENT: {app.Environment.EnvironmentName}");
Console.WriteLine($"IsDevelopment: {app.Environment.IsDevelopment()}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FulSpectrumDbContext>();
    db.Database.Migrate();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
    var normalizedAdminEmail = "ADMIN@FULSPECTRUM.LOCAL";

    if (!db.Users.Any(u => u.NormalizedEmail == normalizedAdminEmail))
    {
        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@fulspectrum.local",
            NormalizedEmail = normalizedAdminEmail,
            FirstName = "System",
            LastName = "Admin",
            Role = UserRoles.Admin,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123*Temp");
        db.Users.Add(admin);
        db.SaveChanges();
    }
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("TraceId", Activity.Current?.TraceId.ToString() ?? string.Empty);
    };
});

app.UseMiddleware<RequestLogEnrichmentMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FulSpectrum API v1");
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (context.File.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            context.File.Name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            context.File.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            context.File.Name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.Headers.CacheControl = "public,max-age=604800";
        }
    }
});

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors("Default");
app.UseAntiforgery();
app.UseResponseCaching();
app.UseRateLimiter();
app.UseMiddleware<AuditTrailMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHangfireDashboard("/jobs");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapPrometheusScrapingEndpoint("/metrics");

if (!string.IsNullOrWhiteSpace(healthChecksUiConnectionString))
{
    app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
}

app.Run();
public partial class Program;