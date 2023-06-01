namespace DataBases;
public static class Lock
{
    // Chequea que las llaves pueden ser llaves de instancias de todas las entidades
    public static bool CheckKeys(List<IEntity> entities, KeyClassWrapped keys)
    {
        foreach (var item in entities)
        {
            var dict = keys.key;
            if (!dict.ContainsKey(item)) return false;
            var keysEntity = dict[item];
            if (!item.ContainsInstance(keys.GetKeyClassWrappedOnlyinThisIEntity(item))) return false;

        }
        return true;
    }
    //Dato un cjto todos los  Atributos al cual [nombre tipo de atributo: Atributo] y la lista de los tipos de atributos a ser llave
    //Genera una llave para una clase 
    public static KeyClass GenerateKey(Dictionary<string, AttributesSet> keys, List<string> keysType)
    {
        keys = new Dictionary<string, AttributesSet>(keys, new StringComparer());

        if (keys.Count != keysType.Count) return null!;

        var result = new List<AttributesSet>(keysType.Count);

        for (int i = 0; i < keysType.Count; i++) result.Add(null!);

        for (int i = 0; i < keysType.Count; i++)

        {
            var type = keysType[i];
            if (!keys.ContainsKey(type)) return null!;
            result[i] = keys[type];
        }
        return new KeyClass(result);
    }
    //Chequea que el tipo de las llaves es correcto 
    public static bool CheckCorrectKeyType(KeyClassWrapped keys, HashSet<string> keysType)
    {
        var temp = new HashSet<string>();

        foreach (KeyValuePair<IEntity, KeyClass> item in keys.key)
        {
            foreach (var attributes in item.Value.GetAttributes())
            {
                if (!keysType.Contains(attributes.Name)) return false;
                temp.Add(attributes.Name);
            }
        }
        return temp.Count == keysType.Count;

    }


    // Devuelve una lista de atributos dado que se entro el tipo de atributos que se quiere y un cjto de atributos
    public static List<AttributesSet> GenerateListToSave(List<string> type, Dictionary<string, AttributesSet> attributes)
    {
        var result = new List<AttributesSet>();
        if (type.Count != attributes.Count) return null!;
        for (int i = 0; i < type.Count; i++)
        {
            var typeName = type[i];
            if (!attributes.ContainsKey(typeName)) return null!;
            result.Add(attributes[typeName]);
        }
        return result;
    }
    //Chequea que sea correcto el tipo de atributos
    public static bool CheckCorrectAttributeType(HashSet<string> type, List<AttributesSet> attributesSets)
    {
        var temp = type.ToHashSet();
        foreach (var item in attributesSets)
        {
            if (!temp.Remove(item.Name)) return false;
        }
        return true;
    }


}



