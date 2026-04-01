using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Tests.Services;
using Tavstal.MesterMC.Api.Utils.Helpers;

namespace Tavstal.MesterMC.Api.Tests.Helpers;

public class TestHelper
{
    public const string UserAgent = "UnitTest/1.0";
    public const string IpAddress = "127.0.0.1";
    public static ServiceProvider ServiceProvider { get; private set; } = new ServiceCollection()
        .AddLogging()
        .AddMemoryCache()
        .BuildServiceProvider();

    public static MemoryCacheService MemoryCacheService { get; private set; } = new(ServiceProvider.GetRequiredService<IMemoryCache>());
    
    public static FakeEmailService FakeEmailService { get; private set; } = new();
    
    public static IPasswordHasher<CustomUser> PasswordHasher { get; private set; } = new PasswordHasher<CustomUser>();
    
    public static void InitTestServices()
    {
        ServiceProvider = new ServiceCollection()
            .AddLogging()
            .AddMemoryCache()
            .BuildServiceProvider();

        MemoryCacheService = new MemoryCacheService(ServiceProvider.GetRequiredService<IMemoryCache>());
        FakeEmailService = new FakeEmailService();
    }

    public static string GetFingerprint(string userId)
    {
        var rawData = $"{userId}-{UserAgent}-{IpAddress}";
        return StringChiper.GetEncryptedHash(rawData, "QvHRAnkn2cr7fTa2PjcaWaQhKndzRNl6");
    }
    
    public static CustomDbContext CreateInMemoryDbContext(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<CustomDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var logger = NullLogger<CustomDbContext>.Instance;
        var db = new CustomDbContext(options, logger);

        // Ensure database tables created for EF InMemory
        db.Database.EnsureCreated();
        return db;
    }

    public static CustomUserManager CreateCustomUserManager(CustomDbContext db)
    {
        var userStore = new CustomUserStore(db); // adjust if constructor differs

        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new PasswordHasher<CustomUser>();
        var userValidators = new List<IUserValidator<CustomUser>>();
        var passwordValidators = new List<IPasswordValidator<CustomUser>>();
        var lookupNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();

        var logger = NullLogger<CustomUserManager>.Instance;

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var settings = CreateTestSettings();

        var manager = new CustomUserManager(
            userStore,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            lookupNormalizer,
            errors,
            ServiceProvider,
            logger,
            db,
            configuration,
            MemoryCacheService,
            settings);

        return manager;
    }

    public static Settings CreateTestSettings()
    {
        var pfxResult = CreateSelfSignedPfx();
        
        return new Settings(
            "http://localhost",
            "http://localhost",
            "root",
            "ascent",
            "QvHRAnkn2cr7fTa2PjcaWaQhKndzRNl6",
            "test-issuer",
            "test-audience",
            "localhost",
            1025,
            "example@localhost",
            "12345678",
            ["localhost"],
            pfxResult.password,
            pfxResult.pfxBytes,
            "MesterMC Development",
            "yggdrasil-mock-server",
            "1.0.0"
            );
    }
    
    public static (byte[] pfxBytes, string password) CreateSelfSignedPfx(string subjectName = "CN=localhost", string password = "changeit")
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)); // ServerAuth

        var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
        var notAfter = DateTimeOffset.UtcNow.AddYears(10);
        using var cert = req.CreateSelfSigned(notBefore, notAfter);

        // Export PFX bytes (include private key)
        var pfx = cert.Export(X509ContentType.Pkcs12, password);
        return (pfx, password);
    }
}