using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Utils.Extensions;

public static class ControllerExtensions
{
    public static IActionResult ReturnResponseCode(this ControllerBase controller, HttpStatusCode status)
    {
        return controller.StatusCode((int)status);
    }
    
    public static IActionResult ReturnResponseCode(this ControllerBase controller, HttpStatusCode status, string message, object value)
    {
        return controller.StatusCode((int)status, JObject.FromObject(new { Message = message, Value = value }).ToString(Formatting.None));
    }
    
    public static IActionResult ReturnResponseCode(this ControllerBase controller, HttpStatusCode status, string message)
    {
        return controller.StatusCode((int)status, message);
    }
    
    public static IActionResult ReturnJson(this ControllerBase controller, string json)
    {
        return controller.Content(json, "application/json");
    }
    
    public static IActionResult ReturnJson(this ControllerBase controller, object json)
    {
        string jsonString = JsonConvert.SerializeObject(json, Formatting.None);
        return controller.Content(jsonString, "application/json");
    }
    
    public static IActionResult ReturnJson(this ControllerBase controller, JObject json)
    {
        return controller.Content(json.ToString(Formatting.None), "application/json");
    }
    
    public static IActionResult ReturnJson(this ControllerBase controller, JArray json)
    {
        return controller.Content(json.ToString(Formatting.None), "application/json");
    }
    
    public static IActionResult RedirectError(this ControllerBase controller, HttpStatusCode status, string message)
    {
        return controller.Redirect($"/oops?id={(int)status}&message={message}");
    }
}