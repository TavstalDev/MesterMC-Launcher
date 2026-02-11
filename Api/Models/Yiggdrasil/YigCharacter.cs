namespace Tavstal.MesterMC.Api.Models.Yiggdrasil;

// TODO: Create an actual database model for this, because it is just an example
public class YigCharacter
{
    private readonly string uuid;
    private readonly string name;
    private readonly EModelType modelType;
    private readonly Dictionary<ETextureType, YigTexture> textures;
    private readonly List<ETextureType> uploadableTextures;
    private readonly YigUser owner;
    
    public YigCharacter(string uuid, string name, EModelType modelType, Dictionary<ETextureType, YigTexture> textures, List<ETextureType> uploadableTextures, YigUser owner)
    {
        this.uuid = uuid;
        this.name = name;
        this.modelType = modelType;
        this.textures = textures;
        this.uploadableTextures = uploadableTextures;
        this.owner = owner;
    }
    
    public string Uuid => uuid;
    public string Name => name;
    public EModelType ModelType => modelType;
    public Dictionary<ETextureType, YigTexture> Textures => textures;
    public List<ETextureType> UploadableTextures => uploadableTextures;
    public YigUser Owner => owner;
    
    public Dictionary<string, object> ToSimpleResponse()
    {
        return new Dictionary<string, object>
        {
            {"id", uuid},
            {"name", name}
        };
    }

    public Dictionary<string, object> ToCompleteResponse()
    {
        return new Dictionary<string, object>
        {
            { "id", uuid },
            { "name", name },
            { "modelType", modelType.ToString() },
            { "textures", textures.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value) },
            { "uploadableTextures", uploadableTextures.Select(t => t.ToString()).ToList() },
            { "owner", owner }
        };
    }
}