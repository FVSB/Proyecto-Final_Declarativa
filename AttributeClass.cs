namespace DataBases;

//

public interface IData : IEquatable<IData>
{
    public abstract bool Equals(IData? other);

}
//The example Class
public class Data<T> : IData where T : IComparable, IEquatable<T>
{
    T value { get; set; }

    public Data(T value) => this.value = value;

    public bool Equals(IData? other)
    {
        if (other is Data<T> otherData)
        {
            return value.Equals(otherData);
        }
        return false;
    }

}
/// <summary>
///  The class is a wrapper where the data types to be stored in the database are encapsulated.
/// </summary>
/// <param name=""></param>
/// <returns></returns>
public class AttributesSet


{
    /// <summary>
    ///  The name of the class this is the factor to be equals 2 instancies
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private string name { get; set; }
    public string Name => name;
    /// <summary>
    ///  have the values of the instancie of the attribute
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private HashSet<IData> values { get; set; }

    public AttributesSet(string name, HashSet<IData> values)
    {
        this.name = name;
        this.values = values;

    }


    public void ChangeName(string newName) { this.name = newName; }


    public bool AddData(IData newData)
    {
        if (this.values.Contains(newData)) return false;
        this.values.Add(newData);
        return true;
    }

    public bool RemoveData(IData data)
    {
        return this.values.Remove(data);
    }

}


