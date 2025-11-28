using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Provides functionality for sending metrics data to the server.
/// </summary>
public static class MetricsHelper
{
    private static CoreLogger _logger = CoreLogger.WithModuleType(typeof(MetricsHelper));

    /// <summary>
    /// Asynchronously sends metrics data, including hardware information, to the server.
    /// </summary>
    /// <remarks>
    /// This method retrieves the launcher settings to obtain the client ID. If the client ID is null or empty,
    /// the operation is aborted. The hardware information is collected and sent to the server as JSON data.
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SendMetricAsync()
    {
        try
        {
            // Retrieve launcher settings
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            string clientID = settings.ClientId;

            // Validate client ID
            if (string.IsNullOrEmpty(clientID))
            {
                _logger.Error($"{nameof(clientID)} is null or empty");
                return;
            }

            // Collect hardware information and send it to the server
            var metricData = OSHelper.CollectHardwareInfo();
            await HttpHelper.PostJsonAsync(MesterMcEndpoints.ApiBaseEndpoint + "launcher/metrics", metricData);
        }
        catch (Exception ex)
        {
            // Log any errors that occur during the operation
            _logger.Error($"Failed to send metric data: {ex.Message}");
        }
    }
}