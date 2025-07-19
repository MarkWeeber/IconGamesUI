using System.Collections.Generic;

public static class Extensions
{
    public static void ReplaceAll(this List<string> list, string oldValue, string newValue)
    {
        for (int i = 0; i < list.Count; i++)
            list[i] = list[i].Replace(oldValue, newValue);
    }

    public static List<T> Modify<T>(this List<T> list, System.Func<T, T> modifier)
    {
        var newList = new List<T>(list.Count);
        for (int i = 0; i < list.Count; i++)
            newList.Add(modifier(list[i]));
        return newList;
    }
}