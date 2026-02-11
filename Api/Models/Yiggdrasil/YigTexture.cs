using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace Tavstal.MesterMC.Api.Models.Yiggdrasil;

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
    
    public static string ComputeTextureHash(Image<Rgba32> img)
    {
        using var sha256 = SHA256.Create();

        int width = img.Width;
        int height = img.Height;

        byte[] buf = new byte[4096];

        PutInt(buf, 0, width);
        PutInt(buf, 4, height);

        int pos = 8;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Rgba32 pixel = img[x, y];

                int argb =
                    (pixel.A << 24) |
                    (pixel.R << 16) |
                    (pixel.G << 8) |
                    (pixel.B);


                PutInt(buf, pos, argb);

                // If alpha == 0 → zero out RGB
                if (buf[pos + 0] == 0)
                {
                    buf[pos + 1] = 0;
                    buf[pos + 2] = 0;
                    buf[pos + 3] = 0;
                }

                pos += 4;

                if (pos == buf.Length)
                {
                    sha256.TransformBlock(buf, 0, buf.Length, null, 0);
                    pos = 0;
                }
            }
        }

        if (pos > 0)
        {
            sha256.TransformBlock(buf, 0, pos, null, 0);
        }

        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        byte[] hash = sha256.Hash;

        return BitConverter.ToString(hash)
            .Replace("-", "")
            .ToLowerInvariant();
    }
    
    private static void PutInt(byte[] array, int offset, int value)
    {
        array[offset + 0] = (byte)((value >> 24) & 0xff);
        array[offset + 1] = (byte)((value >> 16) & 0xff);
        array[offset + 2] = (byte)((value >> 8) & 0xff);
        array[offset + 3] = (byte)((value >> 0) & 0xff);
    }
}