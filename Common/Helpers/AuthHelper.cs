using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class AuthHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(AuthHelper));
    
    public static async Task<string?> LoginAsync(string username, string password)
    {
        try
        {
            var result = await HttpHelper.PostJsonAsync(MesterMcEndpoints.AuthEndpoint, new
            {
                username, password
            });

            if (result == null)
            {
                _logger.Error("Failed to login because the response was null.");
                return null;
            }
            
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error("Login failed with status code: " + result.StatusCode);
                try
                {
                    var localCont = await result.Content.ReadAsStringAsync();
                    JObject localJson = JObject.Parse(localCont);
                    
                    if (localJson.TryGetValue("message", out var value))
                        _logger.Error("Login error message: " + value);
                }
                catch
                {
                    // ignored
                }
                return null;
            }

            var content = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(content);
            
            bool success = json["success"]?.ToObject<bool>() ?? false;
            if (!success)
                return null;
            
            if (json.ContainsKey("redirect")) // When 2FA is required
                return string.Empty;
            
            return json["token"]?.ToString() ?? null;
        }
        catch (Exception ex)
        {
            _logger.Error("Login failed: " + ex.Message);
            return null;
        }
    }

    public static async Task<string?> SubmitTFA(string token, string code)
    {
        try
        {
            var client = HttpHelper.GetHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsJsonAsync(new Uri(MesterMcEndpoints.TfaEndpoint), new
            {
                code
            });

            if (!result.IsSuccessStatusCode)
            {
                _logger.Error("Login failed with status code: " + result.StatusCode);
                try
                {
                    var localCont = await result.Content.ReadAsStringAsync();
                    JObject localJson = JObject.Parse(localCont);
                    
                    if (localJson.TryGetValue("message", out var value))
                        _logger.Error("Login error message: " + value);
                }
                catch
                {
                    // ignored
                }
                return null;
            }
            
            var content = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(content);
            
            bool success = json["success"]?.ToObject<bool>() ?? false;
            if (!success)
                return null;
            
            return json["token"]?.ToString() ?? null;
        }
        catch (Exception ex)
        {
            _logger.Error("TFA submission failed: " + ex.Message);
            return null;
        }
    }
}