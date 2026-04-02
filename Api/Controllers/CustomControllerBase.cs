using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Helpers;

namespace Tavstal.MesterMC.Api.Controllers;

/// <summary>
/// Base controller class that provides common functionality for derived controllers.
/// </summary>
public abstract class CustomControllerBase : Controller
{
    /// <summary>
    /// Logger instance used for logging within the controller.
    /// </summary>
    protected readonly ILogger Logger;
    
    /// <summary>
    /// Reference to the application's <see cref="CustomUserStore"/> used to query and modify user data.
    /// </summary>
    protected readonly CustomUserStore UserStore;
    
    /// <summary>
    /// Application settings instance available to derived controllers.
    /// </summary>
    protected readonly Settings Settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomControllerBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to be used by the controller.</param>
    /// <param name="userStore">The <see cref="CustomUserStore"/> instance for accessing user data.</param>
    /// <param name="settings">The <see cref="Settings"/> instance containing application configuration used by controllers.</param>
    protected CustomControllerBase(ILogger logger, CustomUserStore userStore, Settings settings)
    {
        Logger = logger;
        UserStore = userStore;
        Settings = settings;
    }
    
    /// <summary>
    /// Gets the user ID of the currently authenticated user.
    /// </summary>
    protected string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Retrieves the current user asynchronously using the user store.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current user.</returns>
    protected async Task<CustomUser?> GetCurrentUserAsync()
    {
        if (string.IsNullOrEmpty(UserId))
            return null;
        return await UserStore.FindUserByIdAsync(UserId);
    }

    /// <summary>
    /// Returns an HTTP response with the specified status code.
    /// </summary>
    /// <param name="status">The HTTP status code to return.</param>
    /// <returns>An IActionResult representing the HTTP response.</returns>
    protected IActionResult ReturnResponseCode(HttpStatusCode status) => StatusCode((int)status);

    /// <summary>
    /// Returns an HTTP response with the specified status code, message, and value.
    /// </summary>
    /// <param name="status">The HTTP status code to return.</param>
    /// <param name="message">The message to include in the response.</param>
    /// <param name="value">The value to include in the response.</param>
    /// <returns>An IActionResult representing the HTTP response.</returns>
    protected IActionResult ReturnResponseCode(HttpStatusCode status, string message, object value) => StatusCode(
        (int)status, JObject.FromObject(new { Message = message, Value = value }).ToString(Formatting.None));

    /// <summary>
    /// Returns an HTTP response with the specified status code and message.
    /// </summary>
    /// <param name="status">The HTTP status code to return.</param>
    /// <param name="message">The message to include in the response.</param>
    /// <returns>An IActionResult representing the HTTP response.</returns>
    protected IActionResult ReturnResponseCode(HttpStatusCode status, string message) => StatusCode((int)status, message);

    /// <summary>
    /// Returns a JSON response with the specified JSON string.
    /// </summary>
    /// <param name="json">The JSON string to include in the response.</param>
    /// <returns>An IActionResult representing the JSON response.</returns>
    protected IActionResult ReturnJson(string json) => Content(json, "application/json");

    /// <summary>
    /// Returns a JSON response with the specified object serialized to JSON.
    /// </summary>
    /// <param name="json">The object to serialize and include in the response.</param>
    /// <returns>An IActionResult representing the JSON response.</returns>
    protected IActionResult ReturnJson(object json) =>
        Content(JsonConvert.SerializeObject(json, Formatting.None), "application/json");

    /// <summary>
    /// Returns a JSON response with the specified JObject.
    /// </summary>
    /// <param name="json">The JObject to include in the response.</param>
    /// <returns>An IActionResult representing the JSON response.</returns>
    protected IActionResult ReturnJson(JObject json) => Content(json.ToString(Formatting.None), "application/json");

    /// <summary>
    /// Returns a JSON response with the specified JArray.
    /// </summary>
    /// <param name="json">The JArray to include in the response.</param>
    /// <returns>An IActionResult representing the JSON response.</returns>
    protected IActionResult ReturnJson(JArray json) => Content(json.ToString(Formatting.None), "application/json");

    /// <summary>
    /// Redirects to an error page with the specified status code and message.
    /// </summary>
    /// <param name="status">The HTTP status code to include in the redirect.</param>
    /// <param name="message">The error message to include in the redirect.</param>
    /// <returns>An IActionResult representing the redirect response.</returns>
    protected IActionResult RedirectError(HttpStatusCode status, string message) =>
        Redirect($"/oops?id={(int)status}&message={message}");
    
    /// <summary>
    /// Computes a stable ETag (Entity Tag) for the given JSON string.
    /// The ETag is generated by computing the SHA1 hash of the JSON string
    /// and encoding it in Base64 format, enclosed in double quotes.
    /// </summary>
    /// <param name="json">The JSON string for which the ETag is to be computed.</param>
    /// <returns>A string representing the computed ETag.</returns>
    protected static string ComputeETag(string json)
    {
        using var sha1 = SHA1.Create();
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = sha1.ComputeHash(bytes);
        return "\"" + Convert.ToBase64String(hash) + "\"";
    }
    
    /// <summary>
    /// Computes a machine fingerprint for the specified user by combining traits from the current HTTP request
    /// (User-Agent and remote IP) with the user's identifier, then hashing the combined string using the
    /// application's encryption key.
    /// </summary>
    /// <param name="userId">The user's id is used as part of the fingerprint.</param>
    /// <returns>A string containing the hashed fingerprint.</returns>
    protected string GetMachineFingerprint(string userId)
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
    
        // Combine traits and hash them
        var rawData = $"{userId}-{userAgent}-{ipAddress}";
        return StringChiper.GetEncryptedHash(rawData, Settings.EncryptionKey);
    }
}