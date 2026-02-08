using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tavstal.MesterMC.Api.Utils.Extensions;

public static class ControllerExtensions
{
    public static string GetAuthenticationToken(this ControllerBase controller)
    {
        // Check if the request has an Authorization header
        if (controller.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            // Return the token part of the Authorization header
            return authHeader.ToString();
        }

        if (controller.Request.Cookies.TryGetValue("kmc-token", out var authCookie))
        {
            // If the Authorization header is not present, check for an Authorization cookie
            return authCookie;
        }

        // If no Authorization header is present, return empty
        return string.Empty;
    }
    
    public static IActionResult ReturnResponseCode(this ControllerBase controller, HttpStatusCode status, string message, object value)
    {
        //return controller.StatusCode((int)status, JsonConvert.SerializeObject(new { Message = message, Value = value }, Formatting.None));
        return controller.StatusCode((int)status, JObject.FromObject(new { Message = message, Value = value }).ToString(Formatting.None));
    }
    
    public static IActionResult ReturnResponseCode(this ControllerBase controller, HttpStatusCode status, string message)
    {
        return controller.StatusCode((int)status, message);
    }
    
    public static IActionResult ReturnJson(this ControllerBase controller, HttpStatusCode status, object json)
    {
        return controller.StatusCode((int)status, JObject.FromObject(json).ToString(Formatting.None));
    }
    
    public static IActionResult ReturnJson(this ControllerBase controller, HttpStatusCode status, JObject json)
    {
        return controller.StatusCode((int)status, JObject.FromObject(json).ToString(Formatting.None));
    }
    
    public static IActionResult ReturnJson(this ControllerBase controller, HttpStatusCode status, JArray json)
    {
        return controller.StatusCode((int)status, JObject.FromObject(json).ToString(Formatting.None));
    }
    
    public static IActionResult RedirectError(this ControllerBase controller, HttpStatusCode status, string message)
    {
        return controller.Redirect($"/oops?id={(int)status}&message={message}");
    }
}