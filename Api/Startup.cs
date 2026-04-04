using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Authentication;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Services.Database.Interfaces;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Tavstal.MesterMC.Api;

/// <summary>
/// Application startup configuration class used by the host to configure services and the HTTP request pipeline.
/// 
/// Responsibilities:
/// <br/>- Initialize runtime-specific settings (e.g. upload directory).
/// <br/>- Register application services (database context, identity, authentication, rate limiting, CORS, Swagger, etc.).
/// <br/>- Configure the application's middleware pipeline.
/// </summary>
public class Startup
{
    private static Startup _instance;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly string _uploadDirectory;
    
    /// <summary>
    /// Gets the configured upload directory path used by the application.
    /// This value is initialized in the <see cref="Startup(IConfiguration)"/> constructor and provides
    /// a globally accessible path to the directory where uploaded files are stored.
    /// </summary>
    public static string UploadDirectory => _instance._uploadDirectory;

    /// <summary>
    /// Creates a new <see cref="Startup"/> instance.
    /// 
    /// Behavior:
    /// <br/>- Stores the provided <paramref name="configuration"/> for use when registering services.
    /// <br/>- Registers CodePages encoding provider to support additional encodings.
    /// <br/>- Initializes and ensures existence of the upload directory. The path is composed of
    ///   the application's base directory and the optional "UploadDirectory" configuration value.
    /// </summary>
    /// <param name="configuration">Application configuration provided by the host.</param>
    public Startup(IConfiguration configuration)
    {
        _instance = this;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _configuration = configuration;
        string basePath = AppContext.BaseDirectory;
        _uploadDirectory = Path.Combine(basePath, _configuration.GetValue<string>(Constants.ConfigurationKeys.RuntimeUploadDir) ?? "uploads");
        if (!Directory.Exists(_uploadDirectory))
            Directory.CreateDirectory(_uploadDirectory);
        
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();                
        });
        _logger = loggerFactory.CreateLogger<Startup>();
    }

    /// <summary>
    /// Configures services for dependency injection.
    /// 
    /// This method registers:
    /// <br/>- Database context (Entity Framework Core) and identity services.
    /// <br/>- JSON serialization options and Swagger/OpenAPI generation.
    /// <br/>- Authentication schemes (JWT Bearer, Cookie, and a custom Basic handler).
    /// <br/>- Form options (maximum multipart body size).
    /// <br/>- CORS policy and session configuration.
    /// <br/>- Memory cache, rate limiting policies, hosted services, and application singletons.
    /// 
    /// The method is invoked by the runtime and should only be used to register services.
    /// </summary>
    /// <param name="services">Service collection to which services should be added.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        #region Database
        // Configure identity options
        services.Configure<IdentityOptions>(x =>
        {
            x.User.AllowedUserNameCharacters = Constants.Authentication.AllowedUsernameCharacters;
        });
            
        string? connectionString = _configuration.GetValue<string>(Constants.ConfigurationKeys.DatabaseConnectionString);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string is missing from the configuration.");

        connectionString = connectionString
            .Replace($"${Constants.ConfigurationKeys.DatabaseUser}", _configuration.GetValue<string>(Constants.ConfigurationKeys.DatabaseUser))
            .Replace($"${Constants.ConfigurationKeys.DatabasePassword}", _configuration.GetValue<string>(Constants.ConfigurationKeys.DatabasePassword));

        string? databaseProvider = _configuration.GetValue<string>(Constants.ConfigurationKeys.DatabaseProvider);
        if (string.IsNullOrEmpty(databaseProvider))
            throw new InvalidOperationException("Database Provider is missing from the configuration.");
        
        string? databaseVersion = _configuration.GetValue<string>(Constants.ConfigurationKeys.DatabaseVersion);
        if (string.IsNullOrEmpty(databaseVersion) || !Version.TryParse(databaseVersion, out Version? dbVersion))
            throw new  InvalidOperationException("Database Version is missing from the configuration.");
        
        // Configure the database context
        services.AddDbContext<CustomDbContext>(options =>
        {
            switch (databaseProvider?.ToLower())
            {
                case "postgresql":
                    options.UseNpgsql(connectionString, optionsBuilder => 
                        optionsBuilder.EnableRetryOnFailure());
                    break;
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    options.UseMySql(connectionString, new MySqlServerVersion(dbVersion), optionsBuilder => optionsBuilder.EnableRetryOnFailure());
                    break;
            }
        });
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        #endregion
        #region Swagger Docs
        // Configure JSON options for controllers
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        // Configure Swagger generation
        services.AddSwaggerGen(config =>
        {
            // Add Swagger document with version and metadata
            config.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MesterMC.API",
                Version = "1.0.0",
                Description = "",
                Contact = new OpenApiContact
                {
                    Name = "MesterMC Community",
                    Url = new Uri("https://github.com/TavstalDev/MesterMC-Launcher/issues")
                },
                License = new OpenApiLicense
                {
                    Name = "GNU General Public License v3.0",
                    Url = new Uri("https://www.gnu.org/licenses/gpl-3.0.html")
                }
            });
            // Include XML comments for better documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            config.IncludeXmlComments(xmlPath);
            // Enable annotations for Swagger
            config.EnableAnnotations();
            // Add security definition for Bearer token
            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\nExample: \"Bearer 12345abcdef\"",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer",
            });
            // Add security definition for Basic authentication
            config.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Basic scheme. \r\n\r\nExample: \"Basic 12345abcdef:12345abcdef\"",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "basic",
            });
            // Add security definition for Cookie authentication
            config.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Cookie,
                Description = "JWT Authorization cookie. \r\n\r\nExample: \"mmc-token=12345abcdef\"",
                Name = "mmc-token",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "cookie",
            });
            // Add security requirements for Bearer, Basic and cookie authentication
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    []
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Basic"
                        }
                    },
                    []
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Cookie"
                        }
                    },
                    []
                }
            });
        });
        #endregion
        #region Authentication
        services.AddScoped<CustomUserStore>();
        services.AddScoped<CustomUserManager>();
        services.AddScoped<CustomSignInManager>();
        services.AddScoped<IPasswordHasher<CustomUser>, PasswordHasher<CustomUser>>();

        services.AddHttpContextAccessor();
        // Configure authentication schemes
        services.AddAuthentication(x =>
            {
                // Set the default challenge scheme to JWT Bearer
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                // Set the default authentication scheme to JWT Bearer
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // Set the default sign-in scheme to JWT Bearer
                x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                // Set the default sign-out scheme to JWT Bearer
                x.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Add JWT Bearer authentication  scheme
            .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>("Bearer", null)
            // Add Basic authentication scheme
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null)
            // Add cookie authentication  scheme
            .AddScheme<AuthenticationSchemeOptions, CookieAuthenticationHandler>("Cookie", null);
        #endregion

        // Configure form options
        int bodyLengthLimit = _configuration.GetValue(Constants.ConfigurationKeys.RateLimitingUploadLimit, 100);
        services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 1024 * 1024 * bodyLengthLimit; });
            
        #region Cors
        // Retrieve the CORS configuration section from the application configuration
        var corsDefault = _configuration.GetSection("CORS:Default");
        // Add distributed memory cache services

        var allowAnyOrigin = corsDefault.GetValue<bool>("AllowAnyOrigin");
        var allowAnyMethod = corsDefault.GetValue<bool>("AllowAnyMethod");
        var allowAnyHeader = corsDefault.GetValue<bool>("AllowAnyHeader");
        if (allowAnyOrigin && allowAnyHeader)
            _logger.LogWarning("CORS configuration allows any origin and any header, which may have security implications. Ensure this is intentional.");
        
        services.AddDistributedMemoryCache()
            // Add session services with specified options
            .AddSession(options =>
            {
                // Set the session idle timeout to 10 seconds
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                // Ensure the session cookie is HTTP-only
                options.Cookie.HttpOnly = true;
                // Mark the session cookie as essential
                options.Cookie.IsEssential = true;
            })
            // Add CORS services with specified options
            .AddCors(options =>
            {
                // Add a CORS policy named "Default"
                options.AddPolicy(name: "Default",
                    policy =>
                    {
                        // Allow origins specified in the CORS configuration section
                        policy.WithOrigins(corsDefault.GetSection("Sites").Get<string[]>() ?? throw new Exception("Cors sites section is not set"));
                        policy.WithHeaders(corsDefault.GetSection("Headers").Get<string[]>() ?? []);
                        policy.WithMethods(corsDefault.GetSection("Methods").Get<string[]>() ?? []);
                        policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsDefault.GetValue("MaxAge", 3600)));
                        // Allow any origin if specified in the CORS configuration
                        if (allowAnyOrigin)
                            policy.AllowAnyOrigin();
                        // Allow any header if specified in the CORS configuration
                        if (allowAnyHeader)
                            policy.AllowAnyHeader();
                        // Allow any method if specified in the CORS configuration
                        if (allowAnyMethod)
                            policy.AllowAnyMethod();
                    });
            });
        #endregion
        #region Services
        // Configure identity options for claims
        services.Configure<IdentityOptions>(options => options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier);
        // Add HTTP client factory for making HTTP requests
        services.AddHttpClient();
        // Add memory caching services
        services.AddMemoryCache();
        services.AddSingleton<MemoryCacheService>();
        // Configure IP rate limiting
        #region Rate Limiting

        var rules = _configuration
            .GetSection("RateLimiting:Rules")
            .Get<Dictionary<string, RateLimitRule>>();
        if (rules == null)
            throw new InvalidOperationException("Rate limiting rules are missing from the configuration.");
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = _configuration.GetValue(Constants.ConfigurationKeys.RateLimitingStatusCode, 429);
            foreach (var rule in rules)
            {
                JObject jObject = JObject.FromObject(rule.Value);
                if (jObject.TryGetValue("PermitLimit", out JToken? permitLimitToken) &&
                    jObject.TryGetValue("WindowSeconds", out JToken? windowToken) &&
                    jObject.TryGetValue("QueueLimit", out JToken? queueLimitToken))
                {
                    options.AddFixedWindowLimiter(rule.Key, config =>
                    {
                        config.PermitLimit = permitLimitToken.Value<int>();
                        config.Window = TimeSpan.FromSeconds(windowToken.Value<int>());
                        config.QueueLimit = queueLimitToken.Value<int>();
                        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    });
                }
            }
        });
        #endregion
        // Database cleaner service
        services.AddHostedService<DatabaseCleanerService>();
        // JwtSettings
        services.AddSingleton<Settings>();
        // Email Service
        services.AddSingleton<EmailService>();
        #endregion
    }
    
    /// <summary>
    /// Configures the application's HTTP request pipeline.
    /// 
    /// This method sets up:
    /// <br/>- Developer exception page (when in development).
    /// <br/>- Swagger UI and JSON endpoint at /docs.
    /// <br/>- HTTPS redirection, static files and method override.
    /// <br/>- CORS, routing, session, authentication and authorization middleware.
    /// <br/>- Forwarded headers (X-Forwarded-For, X-Forwarded-Proto) to support reverse proxies.
    /// <br/>- Endpoint routing with rate limiting applied.
    /// <br/>- Database initialization on startup (auto-creates tables if necessary).
    /// 
    /// Any runtime exceptions during database initialization are logged as critical.
    /// </summary>
    /// <param name="app">The application builder used to configure middleware.</param>
    /// <param name="env">Hosting environment information.</param>
    /// <param name="loggerFactory">Logger factory (unused directly here but available if required).</param>
    /// <param name="_logger">A logger instance scoped to <see cref="Startup"/>.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> _logger)
    {
        // Use developer exception page
        if (Program.IsDevelopment)
            app.UseDeveloperExceptionPage();
        // Use Swagger for API documentation
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MesterMC.API v1");
            c.RoutePrefix = "docs";
        });

        app.UseHttpsRedirection();
        app.UseHttpMethodOverride();
        app.UseStaticFiles();
            
        // Configure CORS.
        app.UseCors("Default");

        app.UseRouting();

        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
            
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireRateLimiting("default");
        });
            
        // Auto creates tables
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        if (serviceScope == null)
            return;
            
        try
        {
            var database = serviceScope.ServiceProvider.GetRequiredService<CustomDbContext>();
            var userStore = serviceScope.ServiceProvider.GetRequiredService<CustomUserStore>();
            CustomDbInitializer.Initialize(database, userStore);
        }
        catch (Exception ex)
        {
            var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
            logger.LogCritical(ex, "An error occurred while initializing the database.");
        }
    }
}