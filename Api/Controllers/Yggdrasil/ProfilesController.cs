using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

[ApiController]
[Route("yggdrasil/api/profiles")]
[Tags("Yggdrasil")]
public class ProfilesController : Controller
{
    private readonly ILogger _logger;
    private readonly CustomDbContext _dbContext;
    
    public ProfilesController(ILogger<ProfilesController> logger, CustomDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpPost("minecraft")]
    public IActionResult MinecraftProfile([FromBody] List<String> names)
    {
        List<CustomUser> users = _dbContext.GetUsers(x => names.Contains(x.UserName));
        if (users.Count == 0)
            return this.ReturnResponseCode(HttpStatusCode.NotFound);

        List<Dictionary<string, string>> response = new List<Dictionary<string, string>>();
        foreach (CustomUser user in users)
        {
            Dictionary<string, string> userData = new Dictionary<string, string>
            {
                { "id", user.Id },
                { "name", user.UserName }
            };
            response.Add(userData);
            
        }
        
        return this.ReturnJson(response);
    }
}