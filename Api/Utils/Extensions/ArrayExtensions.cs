namespace Tavstal.MesterMC.Api.Utils.Extensions;

public static class ArrayExtensions
{
    public static bool IsValidIndex<T>(this List<T> list, int index)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (list == null)
            return false;

        return list.Count - 1 >= index;
    }
    
    public static bool IsValidIndex<T>(this T[] list, int index)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (list == null)
            return false;

        return list.Length - 1 >= index;
    }
    
    public static void Shuffle<T>(this T[] list)
    {
        int count = list.Length;
        while (count > 0)
        {
            count--;
            int index = MathExtensions.Next(count + 1);
            (list[index], list[count]) = (list[count], list[index]);
        }
    }
    
    public static void Shuffle<T>(this List<T> list)
    {
        int count = list.Count;
        while (count > 0)
        {
            count--;
            int index = MathExtensions.Next(count + 1);
            (list[index], list[count]) = (list[count], list[index]);
        }
    }
    
    public static bool TryGetValue<T>(this List<T> list, Predicate<T> match, out T? value)
    {
        value = list.Find(match);
        return value != null;
    }
}