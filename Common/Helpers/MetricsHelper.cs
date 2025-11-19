using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class MetricsHelper
{
    private static CoreLogger _logger = CoreLogger.WithModuleType(typeof(MetricsHelper));
    
    public static async Task SendMetricAsync()
    {
        try
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            string clientID = settings.ClientId;
            if (string.IsNullOrEmpty(clientID))
            {
                _logger.Error($"{nameof(clientID)} is null or empty");
                return;
            }

            var metricData = OSHelper.CollectHardwareInfo();
            await HttpHelper.PostJsonAsync(MesterMcEndpoints.ApiBaseEndpoint + "launcher/metrics", metricData);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to send metric data: {ex.Message}");
        }
    }
}