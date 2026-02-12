using Microsoft.AspNetCore.Mvc;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

public class MinecraftServicesController : Controller
{
    [HttpGet("yggdrasil/minecraftservices/publickeys")]
    public IActionResult GetPublicKeys()
    {
        // Return an empty list or your public key in the expected Mojang format
        // For a mock server, an empty profile list often allows the server to continue
        return Ok(new { profileKeys = new object[] { } });
    }
}