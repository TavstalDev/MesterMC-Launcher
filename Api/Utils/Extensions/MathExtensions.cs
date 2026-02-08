namespace Tavstal.MesterMC.Api.Utils.Extensions;

public static class MathExtensions
{
    private static readonly Random Rnd = new();
    private static readonly Lock _lock = new();
    
    public static int Next(int max)
    {
        lock (_lock)
        {
            return Rnd.Next(max);
        }
    }
    
    public static int Next(int min, int max)
    {
        lock (_lock)
        {
            return Rnd.Next(min, max);
        }
    }
    
    public static int Clamp(this int value, int maxValue)
    {
        return maxValue < value ? maxValue : value;
    }
    
    public static int Clamp(this int value, int minValue, int maxValue)
    {
        return minValue > value ? minValue : maxValue < value ? maxValue : value;
    }
    
    public static decimal Clamp(this decimal value, decimal maxValue)
    {
        return maxValue < value ? maxValue : value;
    }
    
    public static decimal Clamp(this decimal value, decimal minValue, decimal maxValue)
    {
        return minValue > value ? minValue : maxValue < value ? maxValue : value;
    }
    
    public static string ToDecimalString(this decimal value, string format = "0.00")
    {
        return value.ToString(format).Replace(",", ".");
    }
}