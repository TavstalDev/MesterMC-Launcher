namespace Tavstal.KonkordLauncher.Core.Models;

public class ModData
{
    public string Name { get;}
    public string Sha256Hash { get; }
    public string Url { get; }
    
    public ModData(string name, string sha256Hash, string url)
    {
        Name = name;
        Sha256Hash = sha256Hash;
        Url = url;
    }
}