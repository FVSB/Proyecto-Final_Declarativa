namespace DataBases;
public enum TypeEntity
{
    Normal = 1,
    Specialization = 2,
    Aggregation = 3,
    Weak = 4

}

public class Manager
{
    public string Name { get; protected set; }
    Dictionary<string, IEntity> entities { get; set; } = new Dictionary<string, IEntity>(new StringEqualsClass()) { };

    Dictionary<string, Relation> relation { get; set; } = new Dictionary<string, Relation>(new StringEqualsClass());

    public Manager(string name) => this.Name = name;

    protected HashSet<string> GetASetWithStringRules(HashSet<string> x) => new HashSet<string>(x, new StringEqualsClass());



    public bool CreateNormalEntity(string nameEntitie, HashSet<string> typeKeys, HashSet<string> typeAttributes)
    {
        var entity = new Entity(Name, GetASetWithStringRules(typeKeys), GetASetWithStringRules(typeAttributes));
        entity.RemoveTheEntity += KillEntity;
        return this.entities.TryAdd(Name, entity);
    }

    public bool ContainsIEntity(string name) => this.entities.ContainsKey(name);

    public bool ContainsRelation(string name) => this.relation.ContainsKey(name);

    public bool CreateSpecialization(string rootEntity, string nameSpecialEntity, HashSet<string> newAttribute)
    {
        if (!this.entities.TryGetValue(rootEntity, out var root)) return false;
        var copy = GetASetWithStringRules(newAttribute);
        copy.IntersectWith(root.GetAttributesType());
        if (copy.Count > 0) return false;
        var temp = new SpecializedEntity(root, nameSpecialEntity, GetASetWithStringRules(newAttribute));
        temp.RemoveTheEntity += KillEntity;
        return this.entities.TryAdd(nameSpecialEntity, temp);
    }

    public bool CreateParticion(string nameSuperEntity, HashSet<string> SubEntites)
    {
        if (!CanBeSubEntities(SubEntites, out var lisEntity, out var attributesType, out var keysType)) return false;
        var temp = new PartitionEntity(nameSuperEntity, keysType, attributesType, lisEntity);
        temp.RemoveTheEntity += KillEntity;
        return this.entities.TryAdd(nameSuperEntity, temp);
    }
    protected bool CanBeSubEntities(HashSet<string> SubEntites, out HashSet<IEntity> list, out HashSet<string> AttributesType,
    out HashSet<string> keysType)
    {
        list = new HashSet<IEntity>();
        keysType = new HashSet<string>();
        AttributesType = new HashSet<string>();
        if (!this.entities.TryGetValue(SubEntites.ElementAt(0), out var first)) return false;
        keysType = first.GetKeysType()!;
        AttributesType = first.GetAttributesType().ToHashSet();
        foreach (var item in SubEntites)
        {
            if (!this.entities.TryGetValue(item, out var temp)) return false;
            if (!keysType.SequenceEqual(temp.GetKeysType(), new StringEqualsClass())) return false;
            AttributesType = AttributesType.Intersect(temp.GetAttributesType(), new StringEqualsClass()).ToHashSet();
            list.Add(temp);
        }

        return true;

    }

    public bool CreateRelation(string name, HashSet<(string nameEntities, Cardinality)> entities)
    {
        if (this.ContainsRelation(name)) return false;
        if (!Relations(entities, out var temp)) return false;
        var relation = new Relation(name, temp);
        relation.RemoveRelation += KillRelation;
        return this.relation.TryAdd(name, relation);
    }

    protected bool Relations(HashSet<(string nameEntities, Cardinality)> entities, out Dictionary<IEntity, Cardinality> dict)
    {
        dict = new Dictionary<IEntity, Cardinality>();
        foreach ((string nameEntities, Cardinality cardinality) item in entities)
        {
            if (this.entities.TryGetValue(item.nameEntities, out var temp)) return false;
            if (dict.TryAdd(temp, item.cardinality)) return false;
        }
        return true;
    }

    public bool CreateAggregation(string name, string relationName, HashSet<string> newAttributes)
    {
        if (ContainsIEntity(name) || !this.ContainsRelation(relationName)) return false;
        var relation = this.relation[relationName];
        var temp = new AgregationEntity(name, relation, newAttributes);
        temp.RemoveTheEntity += KillEntity;
        return this.entities.TryAdd(name, temp);

    }

    public bool CreateWeakEntity(string entityToBeWeakName, string relationName)
    {
        if (!this.entities.TryGetValue(entityToBeWeakName, out var weak)) return false;
        if (!this.relation.TryGetValue(relationName, out var relation)) return false;
        var newName = weak.Name + "  -_weak";
        weak.ChangeName(newName);
        this.entities.Remove(entityToBeWeakName);
        if (this.entities.TryAdd(newName, weak))
        {
            this.entities.Add(entityToBeWeakName, weak);
            return false;
        }
        var temp = new WeakEntity(entityToBeWeakName, relation, weak);
        temp.RemoveTheEntity += KillEntity;
        return this.entities.TryAdd(entityToBeWeakName, temp);
    }


    public bool ContainsEntityKeys(string entityName, KeyClassWrapped keys)
    {
        if (!this.entities.TryGetValue(entityName, out var entity)) return false;
        return entity.ContainsInstance(keys);
    }

    public List<AttributesSet> GetAttributes(string entityName, KeyClassWrapped keys)
    {
        if (!this.ContainsEntityKeys(entityName, keys)) return new List<AttributesSet>();
        this.entities[entityName].GetInstanceAttributes(keys, out var temp);
        return temp;
    }

    public bool ContainsRelationKey(string relationName, KeyClassWrapped keys)
    {
        if (!this.ContainsRelation(relationName)) return false;
        var relation = this.relation[relationName];
        return relation.ContainsInstance(keys);
    }

    public bool AddEntityInstance(string entityName, KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter)
    {
        if (!this.ContainsIEntity(entityName)) return false;
        var entity = this.entities[entityName];
        if (filter is null) filter = GetAlwaysTrueFilter();
        return entity.AddInstance(keys, attributes, filter);
    }
    public bool AddEntityInstance(string entityName, KeyClass keys, List<AttributesSet> attributes, Filter filter)
    {
        if (!this.ContainsIEntity(entityName)) return false;
        var entity = this.entities[entityName];
        if (filter is null) filter = GetAlwaysTrueFilter();
        var x = new Dictionary<IEntity, KeyClass>() { };
        x.Add(entity, keys);
        var temp = new KeyClassWrapped(x);
        return entity.AddInstance(temp, attributes, filter);
    }
    public bool AddRelationInstance(string relationName, KeyClassWrapped keys)
    {
        if (!this.ContainsRelation(relationName)) return false;
        return this.relation[relationName].AddInstance(keys);
    }

    public bool AddRelationInstance(string relationName, List<(string entidad, KeyClass llave)> keys)
    {
        if (!this.ContainsRelation(relationName)) return false;
        var dicc = new Dictionary<IEntity, KeyClass>();
        foreach (var item in keys)
        {
            if (!this.ContainsIEntity(item.entidad)) return false;
            dicc.Add(this.entities[item.entidad], item.llave);
        }

        return this.relation[relationName].AddInstance(new KeyClassWrapped(dicc));
    }


    protected Filter GetAlwaysTrueFilter()
    {
        return new Filter(AlwaysTrue);
    }

    private bool AlwaysTrue(DataBases.IEntity entity, DataBases.KeyClassWrapped Keys, List<AttributesSet> attributes) => true;


    public bool DeleteEntityInstance(string entityName, KeyClassWrapped key)
    {
        if (!this.ContainsIEntity(entityName)) return false;
        return this.entities[entityName].RemoveInstance(key);
    }
    public bool DeleteRelationInstance(string relationName, KeyClassWrapped key)
    {
        if (!this.ContainsRelation(relationName)) return false;
        return this.relation[relationName].RemoveInstance(key);
    }

    protected string KillEntity(string name)
    {
        if (this.ContainsIEntity(name)) this.entities.Remove(name);

        return "";

    }


    protected string KillRelation(string name)
    {
        if (this.ContainsRelation(name)) this.relation.Remove(name);

        return "";
    }

    public bool DeleteEntity(string entityName)
    {
        if (!this.ContainsIEntity(entityName)) return false;
        var temp = this.entities[entityName];
        temp.CallToRemoveTheEntity(temp.Name);
        return this.entities.Remove(entityName);
    }

    public bool DeleteRelation(string relationName)

    {
        if (!this.ContainsRelation(relationName)) return false;
        var temp = this.relation[relationName];
        temp.RemoveTheRelation();
        return this.relation.Remove(relationName);
    }

}