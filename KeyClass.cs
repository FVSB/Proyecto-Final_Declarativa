
using System.Diagnostics.CodeAnalysis;

namespace DataBases;
public class KeyClass : IEquatable<KeyClass>
{

    private List<AttributesSet> key { get; set; }

    public IEntity entity { get; set; }
    public KeyClass(List<AttributesSet> key)
    {
        this.key = key;
    }

    public List<AttributesSet> GetAttributes() => this.key.ToList();

    public bool VerifyAttributeValue(AttributesSet verify) => this.key.Contains(verify);

    public bool Equals(KeyClass? other)
    {
        if (other is null) return false;
        return this.key.Equality(other.GetAttributes());
    }

}

public class KeyClassWrapped
{
    public Dictionary<IEntity, KeyClass> key { get; set; }

    public KeyClassWrapped(Dictionary<IEntity, KeyClass> key) => this.key = key;

    public HashSet<string> GetUnionKeysOfTypes(List<IEntity> entities)
    {
        var setKeysTypes = new HashSet<string>();
        foreach (var item in entities)
        {
            setKeysTypes.UnionWith(item.GetKeysType());
        }
        return setKeysTypes;
    }

    public KeyClassWrapped GetKeyClassWrappedOnlyinThisIEntity(IEntity entity)
    {
        if (this.key.ContainsKey(entity)) return null!;
        var temp = this.key[entity];
        var dict = new Dictionary<IEntity, KeyClass>();
        dict.Add(entity, temp);

        return new KeyClassWrapped(dict);
    }

    public KeyClassWrapped GetKeyClassWrappedOnlyinThisIEntity(List<IEntity> entities)
    {
        var temp = new Dictionary<IEntity, KeyClass>();
        foreach (var item in entities)
        {
            if (this.key.ContainsKey(item))
            {
                var x = this.key[item];
                temp.Add(item, x);
            }
        }
        return new KeyClassWrapped(temp);
    }

    public List<IEntity> GetAllEntities() => this.key.Keys.ToList();

    public List<KeyClass> GetAllKeyClass() => this.key.Values.ToList();

    public bool ContainsIEntity(IEntity entity) => this.key.ContainsKey(entity);

    /// <summary>
    ///  Can return null
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public KeyClass GetTheKeyClass(IEntity entity)
    {
        if (!ContainsIEntity(entity)) return null!;
        return this.key[entity];
    }
    //Machea si x es subconjunto de Y
    public static bool CanMath(KeyClassWrapped x, KeyClassWrapped y)
    {
        var entitiesX = x.GetAllEntities().ToHashSet();
        var entitiesY = y.GetAllEntities().ToHashSet();
        if (entitiesY.IsSubsetOf(entitiesX)) return CanMath(y, x);
        if (!entitiesX.IsSubsetOf(entitiesY)) return false;
        foreach (var item in entitiesX)
        {
            var xKeys = x.GetTheKeyClass(item);
            var yKeys = y.GetTheKeyClass(item);
            if (!xKeys.Equals(yKeys)) return false;

        }
        return true;
    }
    public static List<KeyClassWrapped> SearchAllCoincidences(KeyClassWrapped searchKey, IEnumerable<KeyClassWrapped> fromSearch)
    {
        var result = new List<KeyClassWrapped>();
        foreach (var item in fromSearch)
        {
            if (KeyClassWrapped.CanMath(item, searchKey)) result.Add(item);
        }
        return result;
    }

}

public class KeyClassWrappedEqualable : IEqualityComparer<KeyClassWrapped>
{
    public bool Equals(KeyClassWrapped? x, KeyClassWrapped? y)
    {
        if (x is null || y is null) return false;
        return x.key.Equality(y.key);
    }

    public int GetHashCode([DisallowNull] KeyClassWrapped obj)
    {
        return obj.key.GetHashCode();
    }
}





