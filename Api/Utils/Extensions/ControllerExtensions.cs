using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Utils.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="ControllerBase"/> class to simplify returning HTTP responses and JSON content.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Returns an HTTP response with the specified status code.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="status">The HTTP status code to return.</param>
    /// <returns>An <see cref="IActionResult"/> with the specified status code.</returns>
    public static IActionResult ReturnResponseCode(this ControllerBase controller, HttpStatusCode status)
    {
        return controller.StatusCode((int)status);
    }
    
    /// <summary>
    /// Returns an HTTP response with the specified status code, message, and value.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="status">The HTTP status code to return.</param>
    /// <param name="message">The message to include in the response.</param>
    /// <param name="value">The value to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> with the specified status code, message, and value.</returns>
    public static IActionResult ReturnResponseCode(this ControllerBase controller, HttpStatusCode status, string message, object value)
    {
        return controller.StatusCode((int)status, JObject.FromObject(new { Message = message, Value = value }).ToString(Formatting.None));
    }
    
    /// <summary>
    /// Returns an HTTP response with the specified status code and message.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="status">The HTTP status code to return.</param>
    /// <param name="message">The message to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> with the specified status code and message.</returns>
    public static IActionResult ReturnResponseCode(this ControllerBase controller, HttpStatusCode status, string message)
    {
        return controller.StatusCode((int)status, message);
    }
    
    /// <summary>
    /// Returns a JSON response with the specified JSON string.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="json">The JSON string to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> with the specified JSON content.</returns>
    public static IActionResult ReturnJson(this ControllerBase controller, string json)
    {
        return controller.Content(json, "application/json");
    }
    
    /// <summary>
    /// Returns a JSON response with the specified object serialized to JSON.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="json">The object to serialize to JSON.</param>
    /// <returns>An <see cref="IActionResult"/> with the serialized JSON content.</returns>
    public static IActionResult ReturnJson(this ControllerBase controller, object json)
    {
        string jsonString = JsonConvert.SerializeObject(json, Formatting.None);
        return controller.Content(jsonString, "application/json");
    }
    
    /// <summary>
    /// Returns a JSON response with the specified <see cref="JObject"/>.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="json">The <see cref="JObject"/> to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> with the specified JSON content.</returns>
    public static IActionResult ReturnJson(this ControllerBase controller, JObject json)
    {
        return controller.Content(json.ToString(Formatting.None), "application/json");
    }
    
    /// <summary>
    /// Returns a JSON response with the specified <see cref="JArray"/>.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="json">The <see cref="JArray"/> to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> with the specified JSON content.</returns>
    public static IActionResult ReturnJson(this ControllerBase controller, JArray json)
    {
        return controller.Content(json.ToString(Formatting.None), "application/json");
    }
    
    /// <summary>
    /// Redirects to an error page with the specified status code and message.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="status">The HTTP status code to include in the redirect URL.</param>
    /// <param name="message">The error message to include in the redirect URL.</param>
    /// <returns>An <see cref="IActionResult"/> that redirects to the error page.</returns>
    public static IActionResult RedirectError(this ControllerBase controller, HttpStatusCode status, string message)
    {
        return controller.Redirect($"/oops?id={(int)status}&message={message}");
    }
}