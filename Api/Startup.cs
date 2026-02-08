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
using Microsoft.OpenApi;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Models.Swagger;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    
    public Startup(IConfiguration configuration)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _configuration = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        #region Database & Identity
        // Configure identity options
        services.Configure<IdentityOptions>(x =>
        {
            x.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        });
            
        string? connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string is missing from the configuration.");
        }

        connectionString = connectionString
            .Replace("$DB_USER", _configuration.GetValue<string>("Database:User"))
            .Replace("$DB_PASSWORD", _configuration.GetValue<string>("Database:Password"));

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
                    Name = "Developer",
                    Email = "info@localhost.dev"
                },
                License = new OpenApiLicense
                {
                    Name = "GNU General Public License v3.0",
                    Url = new Uri("https://www.gnu.org/licenses/gpl-3.0.html")
                }
            });
            // Add schema filter for enums
            config.SchemaFilter<EnumSchemaFilter>();
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
            config.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer")] = [],
                [new OpenApiSecuritySchemeReference("Basic")] = []

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("EncryptionKey") ?? throw new Exception("Encryption key is not set"))),
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
        // Configure IP rate limiting
        services.AddMemoryCache();
        #region Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("default", config =>
            {
                // Set the limit to 100 requests per minute
                config.PermitLimit = 100;
                // Set the window duration to 1 minute
                config.Window = TimeSpan.FromMinutes(1);
                // Set the queue limit to 10 requests
                config.QueueLimit = 10;
                config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });
        #endregion
        // JwtSettings
        services.AddSingleton<JwtSettings>();
        // Email Service
        services.AddSingleton<EmailService>();
        #endregion
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        // Use developer exception page
        if (Program.IsDevelopment)
            app.UseDeveloperExceptionPage();
        // Use Swagger for API documentation
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MesterMC.API v1");
            //c.IndexStream = () => File.OpenRead("wwwroot/swagger/index.html");
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