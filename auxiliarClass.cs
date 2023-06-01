
using System.Diagnostics.CodeAnalysis;

namespace DataBases;

// Clase personalizada para comparar pares de llave-valor
public class KeyValuePairComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
{
    public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
    {

        return x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
    }

    public int GetHashCode(KeyValuePair<TKey, TValue> obj)
    {
        return obj.GetHashCode();
    }
}
public static class StringExtensions
{
    /// <summary>
    ///  Este metodo de extensión crea un nuevo string  que es copia del original
    /// </summary>
    /// <param name=""></param>
    /// <returns> nuevo string que es la copia </returns>

    public static string Copy(this string original)
    {
        return new string(original.ToCharArray());
    }

    public static bool Equality(this string original, string other)
    {
        return original.Equals(other) || original.ToLower().Equals(other.ToLower());
    }
}

public class StringEqualsClass : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        return x.Equality(y);
    }

    public int GetHashCode([DisallowNull] string obj)
    {
        return obj.GetHashCode();
    }
}



public static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> original)
    {
        Dictionary<TKey, TValue> clonedDictionary = new Dictionary<TKey, TValue>();
        foreach (KeyValuePair<TKey, TValue> keyValuePair in original)
        {
            clonedDictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }
        return clonedDictionary;
    }

    public static bool Equality<TKey, TValue>(this Dictionary<TKey, TValue> original, Dictionary<TKey, TValue> other)
    {
        if (original.Count != other.Count) return false;
        foreach (KeyValuePair<TKey, TValue> keyValuePair in original)
        {
            if (!other.ContainsKey(keyValuePair.Key) || !keyValuePair.Value.Equals(other[keyValuePair.Key])) return false;
        }
        return true;
    }
    /// <summary>
    ///  Da como resultado un diccionario que es la union de los dos
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
      this Dictionary<TKey, TValue> dictionary,
      Dictionary<TKey, TValue> otherDictionary)
    {
        var result = new Dictionary<TKey, TValue>(dictionary);
        foreach (var kvp in otherDictionary) { result.TryAdd(kvp.Key, kvp.Value); }
        return result;
    }


}


public static class ListExtension
{
    public static bool Equality<T>(this List<T> list, List<T> other)
    {
        if (other.Count != list.Count) return false;
        foreach (var item in list)
        {
            if (!other.Contains(item)) return false;
        }
        return true;
    }

    public static List<T> GetNewListWhitoutAnItem<T>(this List<T> list, T itemExcluse)
    {
        var result = new List<T>();
        foreach (var item in list)
        {
            if (item is not null && item.Equals(itemExcluse)) continue;
            result.Add(item);
        }
        return result;
    }
}


public class Cardinality
{
    public int Max { get; private set; }
    public int Min { get; private set; }

    public Cardinality(int max, int min) { this.Max = max; this.Min = min; }

    public bool ItCanBeWeak()
    {
        if (this.Max == 1 && this.Min == 1) return true;
        return false;
    }
}

public class StringComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        if (x == null && y == null) return true;
        return x.Equality(y);
    }

    public int GetHashCode([DisallowNull] string obj)
    {
        obj = obj.ToLower();
        return obj.GetHashCode();
    }
}

