using System.Diagnostics.CodeAnalysis;
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

    /// <summary>
    /// Attempts to log in a user with the provided username and password.
    /// </summary>
    /// <param name="username">The username of the user attempting to log in.</param>
    /// <param name="password">The password of the user attempting to log in.</param>
    /// <returns>
    /// A tuple containing the authentication token and a boolean indicating if two-factor authentication is required,
    /// or null if the login fails.
    /// </returns>
    /// <remarks>
    /// This method uses code that may be removed during trimming.
    /// </remarks>
    [RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
    public static async Task<(string, bool)?> LoginAsync(string username, string password)
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
            {
                _logger.Error("Login unsuccessful: " + result.StatusCode);
                return null;
            }

            if (json.ContainsKey("redirect")) // When 2FA is required
            {
                string? sessionToken = json["token"]?.ToString();
                if (sessionToken == null)
                {
                    _logger.Error("Failed to get TFA session token: " + result.StatusCode);
                    return null;
                }
                return (sessionToken, true);
            }

            string? token = json["token"]?.ToString();
            if (token == null)
            {
                _logger.Error("Failed to get auth token: " + result.StatusCode);
                return null;
            }
            return (token, false);
        }
        catch (Exception ex)
        {
            _logger.Error("Login failed: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Submits a two-factor authentication (TFA) code for verification.
    /// </summary>
    /// <param name="token">The session token obtained during login.</param>
    /// <param name="code">The TFA code provided by the user.</param>
    /// <returns>
    /// The authentication token if the TFA code is successfully verified, or null if the verification fails.
    /// </returns>
    /// <remarks>
    /// This method uses code that may be removed during trimming.
    /// </remarks>
    [RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
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