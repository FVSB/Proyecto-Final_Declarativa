
namespace DataBases;

/// <summary>
///  This class represent the Relation this class Contains instancies of the name of relation
/// </summary>
/// <param name=""></param>
/// <returns></returns>
public class Relation
{
    /// <summary>
    ///  The name of the relation. Can´t be 2 Relation with the same name
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public string Name { get; set; }
    /// <summary>   
    ///This Dictionary takes the Entities and there Cardinality. The cardinality attached is the result of simulating the relationship in the other entities.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    protected Dictionary<IEntity, Cardinality> Relations { get; set; }
    /// <summary>
    ///  Contains the keys of the instancies.  The keys must respect that there is a subset of them that satisfies an instance of each entity.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    protected HashSet<KeyClassWrapped> intancies { get; set; } = new HashSet<KeyClassWrapped>(new KeyClassWrappedEqualable());
    /// <summary>
    ///  This event notifies subscribers that an instance has been deleted.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public event Action<KeyClassWrapped> RemoveHandler;
    /// <summary>
    ///  This event notifies all subscribers that this relationship will be eliminated and therefore they should take the necessary measures.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public event Kill RemoveRelation;

    public Relation(string name, Dictionary<IEntity, Cardinality> Relations)
    {
        this.Name = name;
        this.Relations = Relations;
        Start(this.Relations.Keys.ToList());
    }

    protected void Start(List<IEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.RemoveHandler += Remove;
            entity.RemoveTheEntity += RemoveAllRelation;
        }
    }

    protected string RemoveAllRelation(string name)
    {
        this.RemoveRelation.Invoke(this.Name);
        return this.Name;
    }
    /// <summary>
    ///  This method maps between the keys of all instances and a key representing all subsets of the key that are subsets of each instance of the keys.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public List<KeyClassWrapped> SearchCanMatch(KeyClassWrapped key) => KeyClassWrapped.SearchAllCoincidences(key, this.intancies);

    protected virtual void Remove(KeyClassWrapped key) => this.RemoveInstance(SearchCanMatch(key));
    /// <summary>
    ///  Identifying whether it is possible to add an instance of the relationship
    /// </summary>
    /// <param name=""></param>
    /// <returns> True was possible false other cases</returns>
    public virtual bool AddInstance(KeyClassWrapped keys)
    {


        if (!Lock.CheckKeys(this.GetEntities(), keys) || !CheckTheCardinality(keys, this.intancies)) return false;
        if (!EntitiesContainsInstance(keys) || this.intancies.Contains(keys)) return false;
        this.intancies.Add(keys);
        return true;
    }
    #region Check Cardinality
    /// <summary>
    ///  To verify that the cardinality of the relationships is respected, you would need to check the number of instances or records associated with each entity involved in the relationship. The cardinality defines the number of instances that can be associated with another instance through a relationship.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    protected virtual bool CheckTheCardinality(KeyClassWrapped keys, HashSet<KeyClassWrapped> instancies)
    {
        var entitiesWithMaxCardinalityProblem = GetEntitiesWithCardinalityRestriction(true);
        if (entitiesWithMaxCardinalityProblem.Count == 0) return true;
        foreach (var item in entitiesWithMaxCardinalityProblem)
        {
            if (!CheckMaxCardinatity(item, keys, instancies)) return false;
        }
        return true;
    }
    /// <summary>
    ///  Returns the entities that may have cardinality constraints if they are different from 0 or >1 (interpreted as cardinality to "Many"). Set true to search for maximum cardinality and false to search for entities with minimum cardinality issues.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    protected virtual List<IEntity> GetEntitiesWithCardinalityRestriction(bool GetWithMaxCardinalityRestriction = true)
    {
        var result = new List<IEntity>();
        foreach (KeyValuePair<IEntity, Cardinality> item in this.Relations)
        {
            if (GetWithMaxCardinalityRestriction)
            {
                if (item.Value.Max == 1) result.Add(item.Key);
            }
            else
            {
                if (item.Value.Min == 1) result.Add(item.Key);
            }
        }
        return result;
    }
    protected virtual bool CheckMaxCardinatity(IEntity exclusion, KeyClassWrapped keys, HashSet<KeyClassWrapped> instancies)
    {

        // Tomamos la lista con la entidad excluida
        var listExclusion = this.Relations.Keys.ToList().GetNewListWhitoutAnItem(exclusion);

        var keysWithoutTheEntity = keys.GetKeyClassWrappedOnlyinThisIEntity(listExclusion);

        //Procedemos a verificar que su cardinalidad no sea mayor que 1
        foreach (var item in instancies)
        {
            if (KeyClassWrapped.CanMath(item, keysWithoutTheEntity)) return false;
        }
        return true;
    }
    #endregion
    protected virtual void RemoveLastAddChanges(List<(IEntity, KeyClassWrapped)> last)
    {
        foreach ((var entity, var key) in last)
        {
            entity.RemoveInstance(key.GetKeyClassWrappedOnlyinThisIEntity(entity));
        }
    }
    protected virtual bool EntitiesContainsInstance(KeyClassWrapped key)
    {
        foreach (var item in this.Relations.Keys)
        {
            if (!item.ContainsInstance(key.GetKeyClassWrappedOnlyinThisIEntity(item))) return false;
        }
        return true;
    }

    public virtual bool ContainsInstance(KeyClassWrapped key)
    {
        return this.intancies.Contains(key);
    }
    public virtual bool RemoveInstance(KeyClassWrapped remove)
    {
        if (this.RemoveHandler is not null) this.RemoveHandler.Invoke(remove);

        return this.intancies.Remove(remove);
    }
    public virtual bool RemoveInstance(List<KeyClassWrapped> list)
    {
        if (list.Count == 0 || list is null) return false;
        bool x = true;
        foreach (var item in list)
        {
            if (this.RemoveInstance(item)) x = false;
        }
        return x;
    }

    public virtual List<IEntity> GetEntities() => this.Relations.Keys.ToList();

    public virtual List<KeyClassWrapped> GetKeys() => this.intancies.ToList();
    public virtual Dictionary<IEntity, Cardinality> Cardinality() => this.Relations.Clone();

    public virtual bool ContainsIEntity(IEntity entity) => this.Relations.ContainsKey(entity);

    public virtual Cardinality GetCardinality(IEntity entity)
    {
        if (!this.Relations.ContainsKey(entity)) return new Cardinality(-1, -1);
        return this.Relations[entity];
    }
    public virtual void RemoveTheRelation() => this.RemoveRelation.Invoke(this.Name);

}






