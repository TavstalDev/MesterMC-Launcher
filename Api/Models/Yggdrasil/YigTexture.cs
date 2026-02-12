using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace Tavstal.MesterMC.Api.Models.Yggdrasil;

public class YigTexture
{
    private readonly string _hash;
    public string Hash => _hash;
    private readonly byte[] data;
    public byte[] Data => data;
    private readonly string url;
    public string Url => url;
    
    public YigTexture(string hash, byte[] data, string url)
    {
        _hash = hash;
        this.data = data;
        this.url = url;
    }
    
    
}