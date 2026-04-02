namespace Tavstal.KonkordLauncher.Core;

public static class Constants
{
    public const string DiscordRpcClientId = "1440491989261877359";

    public static Uri InfoMailUrl { get; } = new("mailto:info@mestermc.hu");
    
    public static Uri FacebookPageUrl { get; } = new("https://www.facebook.com/mestermc.hu");
    
    public const string WebUrl = "https://mestermc.hu";

    public static Uri ServersWebEndpoint { get; } = new("https://mestermc.hu/szerverek");
    
    public static Uri BanyaszermeWebEndpoint { get; }= new("https://mestermc.hu/banyaszerme");
    
    public static Uri DashboardUrl { get; } = new("https://banyakozpont.mestermc.hu");
    
    // Please note that for further configurations visit MesterMcEndpoints
    public const string ApiUrl = "https://api.mestermc.hu";
}