namespace DataBases;

// este delegado es para crear un evento donde este dice si existe o no en la clase que se especializa o contiene a la instancia como un subconjunto
public delegate bool CanAddHandler(string entityName, KeyClassWrapped keyToAdd);

public delegate bool ContainsInstanceHandler(string entityNameWrapped, KeyClassWrapped key);

public delegate bool Filter(IEntity entity, KeyClassWrapped Keys, List<AttributesSet> attributes);

public delegate string Kill(string Name);
public interface IEntity : IEquatable<IEntity>
{

    string Name { get; set; }
    /// <summary>
    ///  Event that determines if a superentity or subentity can or cannot add an element.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    event CanAddHandler CanAdd;
    /// <summary>
    ///  Event that determines if the "child" nodes contain the instance.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    event ContainsInstanceHandler containsInstance;

    event Action<KeyClassWrapped> RemoveHandler;

    void ChangeName(string newName);

    /// <summary>
    ///  Este evento se invoca para las entidades y relaciones que dependen de la subentidad que se va a eliminar tb se elimine
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    event Kill RemoveTheEntity;


    /// <summary>
    ///  This function returns a HashSet where the entity key types are stored.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    HashSet<string> GetKeysType();
    /// <summary>
    ///  This function returns a HashSet with the attribute value types of the entity.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    HashSet<string> GetAttributesType();
    /// <summary>
    ///  This function inserts entity keys and attributes values into the entity set (Duplicate keys are not accepted).  
    /// </summary>
    /// <param name=""></param>
    /// <returns>It returns True if the insertion was successful, False in case of a duplicated key or if the instance could not be inserted in any sub-entity (in case of existence).</returns>
    bool AddInstance(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter);


    bool GetInstanceAttributes(KeyClassWrapped keys, out List<AttributesSet> attributes);
    /// <summary>
    ///  This function returns True if the key is in this entity or any SubEntity (in case it is a SuperEntity), and False otherwise.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    bool ContainsInstance(KeyClassWrapped keys);
    /// <summary>
    ///  This function removes the entity instance and all sub-entities, if any. 
    /// </summary>
    /// <param name=""></param>
    /// <returns>It returns true if the deletion was successful and false if the key to remove does not exist.</returns>
    bool RemoveInstance(KeyClassWrapped keys);
    /// <summary>
    ///  This function returns all instances of attributes for the entity and any sub-entities if they exist.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    List<List<AttributesSet>> GetAllAttributes();
    /// <summary>
    ///  This function returns all instances of keys for the entity and any sub-entities if they exist.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    List<KeyClass> GetAllKeys();


    string CallToRemoveTheEntity(string Name);
}

public abstract class EntityAbstract : IEntity
{
    public abstract string Name { get; set; }

    public abstract event CanAddHandler CanAdd;
    public abstract event ContainsInstanceHandler containsInstance;
    public abstract event Action<KeyClassWrapped> RemoveHandler;

    public virtual event Kill RemoveTheEntity;


    public virtual void ChangeName(string newName) => this.Name = newName;
    public abstract bool AddInstance(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter);

    public abstract bool ContainsInstance(KeyClassWrapped keys);

    public abstract bool RemoveInstance(KeyClassWrapped keys);



    public abstract HashSet<string> GetKeysType();

    public abstract HashSet<string> GetAttributesType();

    public abstract List<List<AttributesSet>> GetAllAttributes();

    public abstract List<KeyClass> GetAllKeys();

    public virtual bool Equals(IEntity? other)
    {
        if (other is null) return false;
        return this.Name.Equals(other.Name);
    }



    public virtual string CallToRemoveTheEntity(string Name)
    {
        this.RemoveTheEntity.Invoke(this.Name);
        return this.Name;
    }


    public abstract bool GetInstanceAttributes(KeyClassWrapped keys, out List<AttributesSet> attributes);



}

public class Entity : EntityAbstract
{
    //Este evento decide en caso que sea parte de una partición si se puede añadir la llave 

    public override event CanAddHandler CanAdd;
    public override event ContainsInstanceHandler containsInstance;

    public override event Action<KeyClassWrapped> RemoveHandler;

    public override string Name { get; set; }

    protected virtual Dictionary<KeyClass, List<AttributesSet>> instancies { get; set; } = new Dictionary<KeyClass, List<AttributesSet>>() { };

    protected virtual HashSet<string> KeysType { get; set; }

    HashSet<string> AttributesType { get; set; }

    public Entity(string Name, HashSet<string> KeysType, HashSet<string> AttributesType)
    {
        this.Name = Name;
        this.KeysType = KeysType;
        this.AttributesType = AttributesType;

    }

    public override bool AddInstance(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter)
    {
        if (!(Lock.CheckCorrectAttributeType(this.AttributesType, attributes) && Lock.CheckCorrectKeyType(keys, this.KeysType))) return false;
        if (!CanAddTheInstance(keys) && !filter.Invoke(this, keys, attributes)) return false;
        if (keys.key.ContainsKey(this))
        {
            return this.instancies.TryAdd(keys.key[this], attributes);
        }
        return false;

    }



    protected virtual bool CanAddTheInstance(KeyClassWrapped keysValue)
    {
        if (CanAdd is null || this.CanAdd.Invoke(this.Name, keysValue)) return true;
        return false;
    }


    public override bool ContainsInstance(KeyClassWrapped keys)
    {
        if (!keys.key.ContainsKey(this)) return false;
        if (containsInstance is not null)
        {
            if (containsInstance.Invoke(this.Name, keys)) return true;
        }

        return this.instancies.ContainsKey(keys.key[this]);
    }

    public override bool RemoveInstance(KeyClassWrapped keys)
    {
        if (!keys.key.ContainsKey(this)) return false;
        if (RemoveHandler is not null)
        {
            this.RemoveHandler.Invoke(keys);
        }
        return this.instancies.Remove(keys.key[this]);
    }

    public override HashSet<string> GetKeysType() => this.KeysType.ToHashSet();


    public override HashSet<string> GetAttributesType() => this.AttributesType.ToHashSet();

    public override List<List<AttributesSet>> GetAllAttributes()
    {
        return this.instancies.Values.ToList();
    }

    public override List<KeyClass> GetAllKeys()
    {
        return this.instancies.Keys.ToList();
    }



    public override bool GetInstanceAttributes(KeyClassWrapped keys, out List<AttributesSet> attributes)
    {
        attributes = new List<AttributesSet>();

        if (this.instancies.TryGetValue(keys.GetKeyClassWrappedOnlyinThisIEntity(this).GetTheKeyClass(this), out attributes)) return true;
        return false;
    }
}

public class SpecializedEntity : EntityAbstract
{
    #region  campos

    public virtual IEntity root { get; set; }

    protected virtual HashSet<string> newAttributes { get; set; }

    public override event CanAddHandler CanAdd;
    public override event ContainsInstanceHandler containsInstance;
    public override event Action<KeyClassWrapped> RemoveHandler;

    protected virtual Dictionary<KeyClass, List<AttributesSet>> instancies { get; set; } = new Dictionary<KeyClass, List<AttributesSet>>();
    public override string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    #endregion
    public SpecializedEntity(IEntity root, string name, HashSet<string> newAttribute)
    {
        this.Name = name;
        this.root = root;
        this.newAttributes = newAttribute;
        this.root.containsInstance += this.ContainsInstance;
        this.root.RemoveHandler += this.Remove;
        this.root.RemoveTheEntity += this.CallToRemoveTheEntity;
    }


    private bool ContainsInstance(string name, KeyClassWrapped key) => ContainsInstance(key);
    public override bool AddInstance(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter)
    {
        if (!keys.key.ContainsKey(this.root)) return false;
        if (!AddTheInstance(keys, attributes, filter)) return false;
        if (this.instancies.TryAdd(keys.key[this.root], attributes) || this.instancies.TryAdd(keys.key[this], attributes))
        {
            this.root.RemoveInstance(keys);
            return true;
        }

        return false;
    }

    private bool AddTheInstance(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter)
    {
        if (!keys.key.ContainsKey(this.root)) return false;
        var attributesType = this.newAttributes.ToHashSet();
        attributesType.UnionWith(this.root.GetAttributesType());
        if (!(Lock.CheckCorrectAttributeType(attributesType, attributes) && Lock.CheckCorrectKeyType(keys, this.root.GetKeysType()))) return false;
        if (filter is not null)
        {
            if (!filter.Invoke(this, keys, attributes)) return false;
        }
        if (this.CanAdd is not null)
        {
            if (!this.CanAdd.Invoke(this.Name, keys)) return false;
        }
        if (this.instancies.ContainsKey(keys.key[this.root])) return false;
        return true;
    }

    public override bool ContainsInstance(KeyClassWrapped keys)
    {
        if (!keys.key.ContainsKey(this.root)) return false;
        if (this.instancies.ContainsKey(keys.key[root])) return true;
        if (this.containsInstance is null) return false;
        return this.containsInstance.Invoke(this.Name, keys);
    }

    public override HashSet<string> GetAttributesType()
    {
        HashSet<string> hs = this.root.GetAttributesType();
        hs.UnionWith(this.newAttributes);
        return hs;
    }

    public override HashSet<string> GetKeysType() => this.root.GetKeysType();


    public override bool RemoveInstance(KeyClassWrapped keys)
    {
        if (!keys.key.ContainsKey(this.root)) return false;
        if (RemoveHandler is not null) this.RemoveHandler.Invoke(keys);
        return this.instancies.Remove(keys.key[root]);
    }

    protected virtual void Remove(KeyClassWrapped keys)
    {
        var mykeys = keys.GetKeyClassWrappedOnlyinThisIEntity(this.root);
        this.RemoveInstance(mykeys);
    }

    public override List<List<AttributesSet>> GetAllAttributes()
    {
        var x = new List<List<AttributesSet>>();
        x.AddRange(this.instancies.Values.ToList());
        x.AddRange(this.root.GetAllAttributes());
        return x;
    }

    public override List<KeyClass> GetAllKeys()
    {
        return this.root.GetAllKeys();

    }



    public override bool GetInstanceAttributes(KeyClassWrapped keys, out List<AttributesSet> attributes)
    {
        attributes = new List<AttributesSet>();
        if (this.instancies.TryGetValue(keys.GetKeyClassWrappedOnlyinThisIEntity(this).GetTheKeyClass(this), out attributes)) return true;
        if (this.instancies.TryGetValue(keys.GetKeyClassWrappedOnlyinThisIEntity(this.root).GetTheKeyClass(this.root), out attributes)) return true;
        return false;
    }
}
public class PartitionEntity : EntityAbstract


{
    public override string Name { get; set; }

    protected virtual HashSet<string> keysType { get; set; }

    protected virtual HashSet<IEntity> entities { get; set; }

    protected virtual HashSet<string> attributesType { get; set; }
    public PartitionEntity(string name, HashSet<string> keysType, HashSet<string> attributesType, HashSet<IEntity> entities)
    {
        this.Name = name;
        this.keysType = keysType;
        this.entities = entities;
        this.attributesType = attributesType;
        Tokenize(keysType, attributesType, entities);
    }

    public override event CanAddHandler CanAdd;
    public override event ContainsInstanceHandler containsInstance;
    public override event Action<KeyClassWrapped> RemoveHandler;

    protected virtual void KillChild(string name)
    {
        foreach (var item in this.entities) { if (item.Name.Equality(name)) ; this.entities.Remove(item); }
        if (this.entities.Count == 0) this.CallToRemoveTheEntity(this.Name);
    }
    public override bool AddInstance(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter)
    {
        if (this.CanAdd is not null)
        {
            if (!CanAdd.Invoke(this.Name, keys)) return false;
        }
        if (!Lock.CheckCorrectKeyType(keys, this.keysType)) return false;

        //Se examina que no este en ninguna de las particiones
        if (this.RemoveInstance(keys)) return false;
        bool firtsAdd = false;
        foreach (var item in this.entities)
        {
            if (item.AddInstance(keys, attributes, filter))
            {
                if (firtsAdd)
                {
                    this.RemoveInstance(keys);
                    this.RemoveInstance(keys);
                    return false;
                }
                firtsAdd = true;
            }
        }
        return firtsAdd;
    }

    protected virtual void Tokenize(HashSet<string> keysTipe, HashSet<string> attributesType, HashSet<IEntity> entities)
    {
        foreach (var item in entities)
        {
            var tempTypeAttributes = item.GetAttributesType();
            var tempTypeKeys = item.GetKeysType();
            if (!Check(keysTipe, tempTypeKeys, attributesType, tempTypeAttributes))
            {
                throw new Exception("Las llaves deben de coincidir y los atributos de la super entidad deben de ser subconjutos de las entidades particiones");
            }
            item.CanAdd += this.CanAddNewInstance;
            item.RemoveHandler += Remove;

        }

    }



    //Chequea que las entidades tienen el mismo tipo de llaves
    protected virtual bool Check(HashSet<string> keysTipe, HashSet<string> tempKeys, HashSet<string> attributesType, HashSet<string> tempAttributes)
    {
        //Si algunas de las claves no coinciden o el set de attributos de la super entidad no es subconjunto 
        // de las particiones hay que emitir falso
        return tempKeys.SetEquals(keysTipe) && attributesType.IsSubsetOf(tempAttributes);

    }


    protected virtual bool CanAddNewInstance(string entitieName, KeyClassWrapped key)
    {
        bool HaveThisEntitie = false;
        foreach (var item in this.entities)
        {
            if (item.Name == entitieName) { HaveThisEntitie = true; continue; }
            if (item.ContainsInstance(key)) return false;
        }
        return HaveThisEntitie;
    }

    public override bool ContainsInstance(KeyClassWrapped keys)
    {

        foreach (var item in this.entities)
        {
            if (item.ContainsInstance(keys)) return true;

        }
        if (this.containsInstance is not null) return this.containsInstance.Invoke(this.Name, keys);
        return false;
    }

    public override bool RemoveInstance(KeyClassWrapped keys)
    {
        foreach (var item in this.entities)
        {
            if (item.RemoveInstance(keys))
            {
                if (RemoveHandler is not null)
                {
                    this.RemoveHandler.Invoke(keys);
                }
                return true;

            }

        }
        return false;
    }

    protected virtual void Remove(KeyClassWrapped keys)
    {
        this.RemoveInstance(keys);
    }

    public override HashSet<string> GetKeysType() => this.keysType;


    public override HashSet<string> GetAttributesType() => this.attributesType;

    public override List<List<AttributesSet>> GetAllAttributes()
    {
        var temp = new List<List<AttributesSet>>();
        foreach (var item in this.entities)
        {
            temp.AddRange(item.GetAllAttributes());
        }
        return temp;
    }

    public override List<KeyClass> GetAllKeys()
    {
        var temp = new List<KeyClass>();
        foreach (var item in this.entities)
        {
            temp.AddRange(item.GetAllKeys());
        }
        return temp;
    }


    public override bool GetInstanceAttributes(KeyClassWrapped keys, out List<AttributesSet> attributes)
    {
        attributes = new List<AttributesSet>();
        foreach (var item in this.entities)
        {
            if (item.GetInstanceAttributes(keys, out attributes)) return true;
        }
        return false;
    }
}


public class AgregationEntity : EntityAbstract
{
    public override string Name { get; set; }

    public override event CanAddHandler CanAdd;
    public override event ContainsInstanceHandler containsInstance;
    public override event Action<KeyClassWrapped> RemoveHandler;

    protected virtual List<IEntity> entities { get { return this.relation.GetEntities(); } }

    protected virtual Relation relation { get; set; }

    protected virtual HashSet<string> newAttributesType { get; set; }

    // Se añaden a los atributos propios de la agregación 
    protected virtual Dictionary<KeyClassWrapped, List<AttributesSet>> instancies { get; set; } = new Dictionary<KeyClassWrapped, List<AttributesSet>>(
      new Dictionary<KeyClassWrapped, List<AttributesSet>>(), new KeyClassWrappedEqualable()
    );

    public AgregationEntity(string name, Relation relation, HashSet<string> newAttributesType)
    {
        this.Name = name;
        this.relation = relation;
        this.newAttributesType = newAttributesType;

        Start(entities, relation);

    }


    private void Start(List<IEntity> entities, Relation relation)
    {
        relation.RemoveRelation += this.CallToRemoveTheEntity;
        relation.RemoveHandler += this.Remove;
        foreach (var item in entities)
        {
            item.RemoveTheEntity += this.CallToRemoveTheEntity;
            item.RemoveHandler += this.Remove;
        }
    }
    // Siempre se añaden los attributos que tiene solo esta entidad
    /// <summary>
    ///  Se agregan los elementos a la nueva entidad para ello deben existir instancias de cada entidad que forma parte de la agregación  en caso de no cumplirse se lanza false;
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public override bool AddInstance(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter)
    {
        if (this.CanAdd is not null)
        {
            if (!this.CanAdd.Invoke(this.Name, keys)) return false;
        }
        if (filter is not null)
        {
            if (!filter.Invoke(this, keys, attributes)) return false;
        }
        if (!Lock.CheckKeys(this.entities, keys)) return false;


        if (!Lock.CheckCorrectAttributeType(this.newAttributesType, attributes)) return false;

        if (!this.relation.ContainsInstance(keys))
        { if (!this.relation.AddInstance(keys)) return false; }

        return this.instancies.TryAdd(keys, attributes);
    }



    protected virtual HashSet<string> GenerateKeysType()
    {
        var setKeysTypes = new HashSet<string>();
        foreach (var item in this.entities)
        {
            setKeysTypes.UnionWith(item.GetKeysType());
        }
        return setKeysTypes;
    }

    public override bool ContainsInstance(KeyClassWrapped keys)
    {
        if (containsInstance is not null)
        {
            if (this.containsInstance.Invoke(this.Name, keys)) return true;
        }
        return this.instancies.TryGetValue(keys, out List<AttributesSet> x) != null;
    }


    public override HashSet<string> GetAttributesType() => this.newAttributesType;


    public override HashSet<string> GetKeysType() => this.GenerateKeysType();



    public override bool RemoveInstance(KeyClassWrapped keys)
    {
        if (RemoveHandler is not null)
        {
            this.RemoveHandler(keys);
        }
        return this.instancies.Remove(keys);
    }

    protected virtual void Remove(KeyClassWrapped key)
    {
        var listRemove = new List<KeyClassWrapped>();
        foreach (var item in this.instancies.Keys)
        {
            if (KeyClassWrapped.CanMath(key, item)) listRemove.Add(item);
        }
        foreach (var item in listRemove) { this.RemoveInstance(item); }

    }
    public override List<List<AttributesSet>> GetAllAttributes() => instancies.Values.ToList();


    public override List<KeyClass> GetAllKeys()
    {
        var set = this.instancies.Keys.ToList();
        var result = new List<KeyClass>();
        foreach (var item in set)
        {
            result.AddRange(item.GetAllKeyClass());
        }
        return result;
    }

    public override bool GetInstanceAttributes(KeyClassWrapped keys, out List<AttributesSet> attributes)
    {
        attributes = new List<AttributesSet>();
        if (this.instancies.TryGetValue(keys, out attributes)) return true;
        return false;
    }
}


public class WeakEntity : EntityAbstract
{
    public override string Name { get; set; }

    public override event CanAddHandler CanAdd;
    public override event ContainsInstanceHandler containsInstance;
    public override event Action<KeyClassWrapped> RemoveHandler;


    public Relation relation { get; protected set; }

    public IEntity weakEntity { get; protected set; }

    public WeakEntity(string name, Relation relation, IEntity weakEntity)
    {
        this.Name = name;
        this.relation = relation;
        this.weakEntity = weakEntity;
        if (!this.relation.ContainsIEntity(this.weakEntity)) { throw new Exception("The weakEntity needs stay in the relation"); }
        if (!this.relation.GetCardinality(weakEntity).ItCanBeWeak())
        {
            throw new Exception
        ("The cardinality needs has (1,1)");
        }

    }

    private void Start()
    {
        this.weakEntity.CanAdd += CanAddTheWeak;
        this.weakEntity.containsInstance += ContainsTheWeak;
        this.weakEntity.RemoveHandler += Remove;
        this.RemoveTheEntity += this.CallToRemoveTheEntity;
        this.relation.RemoveHandler += Remove;
        this.relation.RemoveRelation += this.CallToRemoveTheEntity;
    }

    public override string CallToRemoveTheEntity(string Name)
    {
        this.weakEntity.CallToRemoveTheEntity(this.weakEntity.Name);
        return base.CallToRemoveTheEntity(this.Name);
    }

    protected virtual bool CanAddTheWeak(string entityName, KeyClassWrapped keyToAdd) => !this.relation.ContainsInstance(keyToAdd);

    public override bool AddInstance(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter)
    {
        if (this.relation.ContainsInstance(keys)) return false;
        if (filter is not null)
        {
            if (filter.Invoke(this, keys, attributes))
            {
                return CanAddPrivate(keys, attributes, null!);
            }
        }

        return CanAddPrivate(keys, attributes, filter);
    }

    protected bool CanAddPrivate(KeyClassWrapped keys, List<AttributesSet> attributes, Filter filter)
    {
        if (this.weakEntity.AddInstance(keys, attributes, filter!))
        {
            if (this.relation.AddInstance(keys)) { return true; }
            this.weakEntity.RemoveInstance(keys);
        }
        return false;
    }

    private bool ContainsTheWeak(string name, KeyClassWrapped keys) => this.ContainsInstance(keys);

    public override bool ContainsInstance(KeyClassWrapped keys) => this.relation.ContainsInstance(keys);


    public override HashSet<string> GetAttributesType()
    {
        var result = new HashSet<string>();
        foreach (var item in this.relation.GetEntities())
        {
            result.UnionWith(item.GetAttributesType());
        }
        return result;
    }

    public override HashSet<string> GetKeysType()
    {
        var result = new HashSet<string>();
        foreach (var item in this.relation.GetEntities())
        {
            result.UnionWith(item.GetKeysType());
        }
        return result;

    }

    public override bool RemoveInstance(KeyClassWrapped keys)
    {
        this.relation.RemoveInstance(keys);
        if (this.RemoveHandler is not null)
        {
            this.RemoveHandler.Invoke(keys);
        }
        return this.weakEntity.RemoveInstance(keys.GetKeyClassWrappedOnlyinThisIEntity(this.weakEntity));
    }
    protected virtual void Remove(KeyClassWrapped keys)
    {
        var result = new List<KeyClassWrapped>();
        foreach (var item in this.relation.GetKeys())
        {
            if (KeyClassWrapped.CanMath(keys, item)) { result.Add(item); }
        }
        foreach (var item in result) { this.RemoveInstance(item); }

    }

    public override List<List<AttributesSet>> GetAllAttributes()
    {
        var x = new List<List<AttributesSet>>();
        foreach (var item in this.relation.GetEntities())
        {
            x.AddRange(item.GetAllAttributes());
        }
        return x;
    }

    public override List<KeyClass> GetAllKeys()
    {
        var x = new List<KeyClass>();
        foreach (var item in this.relation.GetEntities())
        {
            x.AddRange(item.GetAllKeys());

        }
        return x;
    }

    public override bool GetInstanceAttributes(KeyClassWrapped keys, out List<AttributesSet> attributes)
    {
        attributes = new List<AttributesSet>();
        if (!ContainsInstance(keys)) return false;
        return this.weakEntity.GetInstanceAttributes(keys, out attributes);
    }
}
