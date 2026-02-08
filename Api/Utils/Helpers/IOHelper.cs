using System.Collections.Immutable;
using System.Diagnostics;
using MimeDetective;
using MimeDetective.Engine;

namespace Tavstal.MesterMC.Api.Utils.Helpers;

public static class IOHelper
{
    public static async Task<bool> SaveFileAsync(string filePath, byte[] fileContent, string[]? acceptedMimeTypes = null)
    {
        try
        {
            if (acceptedMimeTypes != null && !VerifyMimeType(fileContent, acceptedMimeTypes))
                return false;

            var tempPath = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempPath, fileContent);
            if (await IsFileInfectedAsync(tempPath))
                return false;

            File.Move(tempPath, filePath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }
    
    public static bool VerifyMimeType(byte[] fileContent, string[] expectedMimeTypes)
    {
        var inspector = new ContentInspectorBuilder().Build();
        var result = inspector.Inspect(fileContent);
        ImmutableArray<MimeTypeMatch> mimeType = result.ByMimeType();

        if (mimeType == null)
            return false;

        var mimeTypeString = mimeType.FirstOrDefault()?.MimeType;
        if (mimeTypeString == null)
            return false;

        return expectedMimeTypes.Contains(mimeTypeString);
    }
    
    public static async Task<bool> IsFileInfectedAsync(string filePath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "clamscan",
                Arguments = $"--no-summary {filePath}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (output.Contains("Infected"))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }
}