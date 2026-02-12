using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Provides helper methods for authentication, including login and two-factor authentication (TFA) submission.
/// </summary>
public static class AuthHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(AuthHelper));

    [RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
    public static async Task<(string, bool, string?)?> LoginAsync(string username, string password)
    {
        try
        {
            var result = await HttpHelper.PostJsonAsync(MesterMcEndpoints.AuthEndpoint, new
            {
                Username = username, 
                Password = password
            });

            if (result == null)
            {
                _logger.Error("Failed to login because the response was null.");
                return null;
            }

            if (!result.IsSuccessStatusCode && result.StatusCode != HttpStatusCode.Redirect)
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
            _logger.Debug("Login response content: " + content);
            JObject json = JObject.Parse(content);

            if (result.StatusCode == HttpStatusCode.Redirect) // When 2FA is required
            {
                string? sessionToken = json["Token"]?.ToString();
                if (sessionToken == null)
                {
                    _logger.Error("Failed to get TFA session token: " + result.StatusCode);
                    return null;
                }
                return (sessionToken, true, null);
            }

            string? token = json["Token"]?.ToString();
            if (token == null)
            {
                _logger.Error("Failed to get auth token: " + result.StatusCode);
                return null;
            }
            string? userId = json["UserId"]?.ToString();
            if (userId == null)
            {
                _logger.Error("Failed to get user ID: " + result.StatusCode);
                return null;
            }
            
            return (token, false, userId);
        }
        catch (Exception ex)
        {
            _logger.Error("Login failed: " + ex.Message);
            return null;
        }
    }
    
    [RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
    public static async Task<(string, string)?> SubmitTFA(string token, string code)
    {
        try
        {
            var client = HttpHelper.GetHttpClient();
            var result = await client.PostAsJsonAsync(new Uri(MesterMcEndpoints.TfaEndpoint), new
            {
                SessionToken = token,
                TwoFactorCode = code
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
            _logger.Debug("TFA submission response content: " + content);
            JObject json = JObject.Parse(content);
            string? accessToken = json["Token"]?.ToString();
            if (accessToken == null)
            {
                _logger.Error("Failed to get auth token: " + result.StatusCode);
                return null;
            }
            string? userId = json["UserId"]?.ToString();
            if (userId == null)
            {
                _logger.Error("Failed to get user ID: " + result.StatusCode);
                return null;
            }
            
            return (accessToken, userId);
        }
        catch (Exception ex)
        {
            _logger.Error("TFA submission failed: " + ex.Message);
            return null;
        }
    }
}