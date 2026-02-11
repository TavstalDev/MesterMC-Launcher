using Tavstal.MesterMC.Api.Utils;

namespace Tavstal.MesterMC.Api.Models.Yiggdrasil;

// TODO: Merge with the actual user model
public class YigUser
{
    private string _uuid;
    private string _email;
    private string _password;
    private List<YigCharacter> _characters;

    public YigUser(string uuid, string email, string password, List<YigCharacter> characters)
    {
        this._uuid = uuid;
        this._email = email;
        this._password = password;
        this._characters = characters;
    }

    public string Uuid => _uuid;

    public void SetUuid(string uuid)
    {
        this._uuid = uuid;
    }

    public string Email => _email;

    public void SetEmail(string email)
    {
        this._email = email;
    }

    public string Password => _password;

    public void SetPassword(string password)
    {
        this._password = password;
    }

    public List<YigCharacter> Characters => _characters;

    public void SetCharacters(List<YigCharacter> characters)
    {
        this._characters = characters;
    }
    
    public Dictionary<string, object> ToResponse()
    {
        return new Dictionary<string, object>
        {
            {"id", _uuid},
            {"properties", YiggdrasilHelper.Properties()}
        };
    }
}