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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
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
        _uploadDirectory = Path.Combine(basePath, _configuration.GetValue<string>("UploadDirectory") ?? "uploads");
        if (!Directory.Exists(_uploadDirectory))
            Directory.CreateDirectory(_uploadDirectory);
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
        #region Database & Identity
        // Configure identity options
        services.Configure<IdentityOptions>(x =>
        {
            x.User.AllowedUserNameCharacters = Constants.Authentication.AllowedUsernameCharacters;
        });
            
        string? connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string is missing from the configuration.");
        }

        connectionString = connectionString
            .Replace("$DB_USER", _configuration.GetValue<string>("DB_USER"))
            .Replace("$DB_PASSWORD", _configuration.GetValue<string>("DB_PASSWORD"));

        // Configure the database context
        services.AddDbContext<CustomDbContext>(options =>
            // ReSharper disable once AccessToModifiedClosure
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 31)), optionsBuilder => 
            {
                optionsBuilder.EnableRetryOnFailure();
            }));
            
        // Configure identity core services with custom user and database context
        services.AddIdentityCore<CustomUser>(options => options.SignIn.RequireConfirmedAccount = true)
            // Add Entity Framework stores for the custom database context
            .AddEntityFrameworkStores<CustomDbContext>()
            // Add custom user manager
            .AddUserManager<CustomUserManager>()
            // Add sign-in manager for custom user
            .AddSignInManager<SignInManager<CustomUser>>()
            // Add custom user store
            .AddUserStore<CustomUserStore>()
            // Add default token providers for generating tokens
            .AddDefaultTokenProviders();
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
            // Add security requirements for Bearer and Basic authentication
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
                }
            });
        });
        #endregion
        #region Authentication
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
            // Add cookie authentication
            .AddCookie() 
            // Add JWT Bearer authentication
            .AddJwtBearer(x =>
            {
                // Include error details in the response
                x.IncludeErrorDetails = true;
                // Set token validation parameters
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validate the issuer of the token
                    ValidateIssuer = true,
                    // Validate the audience of the token
                    ValidateAudience = true,
                    // Validate the signing key of the token
                    ValidateIssuerSigningKey = true,
                    // Validate the lifetime of the token
                    ValidateLifetime = true,
                    // Require the token to have an expiration time
                    RequireExpirationTime = true,
                    // Set the valid issuer for the token
                    ValidIssuer = _configuration.GetValue<string>("Jwt:Issuer"),
                    // Set the valid audience for the token
                    ValidAudience = _configuration.GetValue<string>("Jwt:Audience"),
                    // Set the signing key for the token
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWT_ENCRYPTION_KEY") ?? throw new Exception("Encryption key is not set"))),
                };
            })
            // Add custom authentication scheme for Basic authentication
            .AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("Basic", null);
        #endregion

        // Configure form options
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
        });
            
        #region Cors
        // Retrieve the CORS configuration section from the application configuration
        var corsDefault = _configuration.GetSection("CORS:Default");
        // Add distributed memory cache services
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
                        // Allow any origin if specified in the CORS configuration
                        if (corsDefault.GetValue<bool>("AllowAnyOrigin"))
                            policy.AllowAnyOrigin();
                        // Allow any header if specified in the CORS configuration
                        if (corsDefault.GetValue<bool>("AllowAnyHeader"))
                            policy.AllowAnyHeader();
                        // Allow any method if specified in the CORS configuration
                        if (corsDefault.GetValue<bool>("AllowAnyMethod"))
                            policy.AllowAnyMethod();
                    });
            });
        #endregion
        #region Services
        // Configure identity options for claims
        services.Configure<IdentityOptions>(options => options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier);
        // Add memory caching services
        services.AddMemoryCache();
        services.AddSingleton<MemoryCacheService>();
        // Configure IP rate limiting
        #region Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // Default
            options.AddFixedWindowLimiter(RateLimits.DEFAULT, config =>
            {
                // Set the limit to 100 requests per minute
                config.PermitLimit = 100;
                // Set the window duration to 1 minute
                config.Window = TimeSpan.FromMinutes(1);
                // Set the queue limit to 10 requests
                config.QueueLimit = 10;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            // Auth Register
            options.AddFixedWindowLimiter(RateLimits.AUTH_REGISTER, config =>
            {
                config.PermitLimit = 2;
                config.Window = TimeSpan.FromMinutes(60);
                config.QueueLimit = 10;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            // Auth Login
            options.AddFixedWindowLimiter(RateLimits.AUTH_LOGIN, config =>
            {
                config.PermitLimit = 5;
                config.Window = TimeSpan.FromMinutes(5);
                config.QueueLimit = 5;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            // Auth TFA & Reset
            options.AddFixedWindowLimiter(RateLimits.AUTH_RESET, config =>
            {
                config.PermitLimit = 3;
                config.Window = TimeSpan.FromMinutes(60);
                config.QueueLimit = 5;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            // Upload
            options.AddFixedWindowLimiter(RateLimits.UPLOAD, config =>
            {
                config.PermitLimit = 20;
                config.Window = TimeSpan.FromMinutes(10);
                config.QueueLimit = 5;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            // Download
            options.AddFixedWindowLimiter(RateLimits.DOWNLOAD, config =>
            {
                config.PermitLimit = 30;
                config.Window = TimeSpan.FromMinutes(5);
                config.QueueLimit = 5;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            // Search
            options.AddFixedWindowLimiter(RateLimits.SEARCH, config =>
            {
                config.PermitLimit = 60;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueLimit = 5;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            // Write
            options.AddFixedWindowLimiter(RateLimits.WRITE, config =>
            {
                config.PermitLimit = 30;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueLimit = 5;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
            // Admin
            options.AddFixedWindowLimiter(RateLimits.ADMIN, config =>
            {
                config.PermitLimit = 20;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueLimit = 5;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
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
            CustomDbInitializer.Initialize(database);
        }
        catch (Exception ex)
        {
            var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
            logger.LogCritical(ex.ToString());
        }
    }
}