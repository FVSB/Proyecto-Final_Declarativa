

namespace DataBases;
using System;
class Program
{
    static void Main(string[] args)
    {/*
        var tipollave = new List<string>() { "ci" };
        var tipoattributo = new HashSet<string>() { "nombre" };
        var persona = new Entity("persona", tipollave, tipoattributo);
        var x = new Data<int>(1111);
        var nombre = new Data<string>("F");
        var ci = new AttributesSet("hola", new HashSet<IData>() { x });
        var nombreAtt = new AttributesSet("nombre", new HashSet<IData>() { nombre });
        var dict = new Dictionary<string, AttributesSet>();
        dict.Add("ci", ci);
        var keys = Lock.GenerateKey(dict, tipollave);
        var dict2 = new Dictionary<IEntity, KeyClass>();
        dict2.Add(persona, keys);
        var keysClass = new KeyClassWrapped(dict2);
        persona.AddInstance(keysClass, new List<AttributesSet>() { nombreAtt }, null);

        var nuevoatributos = new List<string> { "juego" };


        var personaJugadora = new SpecializedEntity(persona, "jugador", nuevoatributos);

        persona.CallToRemoveTheEntity("persona");
     */

        var manager = new Manager("Test");
        manager.CreateNormalEntity("persona", new HashSet<string>() { "ci" }, new HashSet<string>() { "ci", "nombre" });
        var attKeys = new AttributesSet("ci", new HashSet<IData>() { new Data<int>(1) });
        var attr = new AttributesSet("nombre", new HashSet<IData>() { new Data<string>("juan") });
        var keys = new KeyClass(new List<AttributesSet>() { attKeys });
        manager.AddEntityInstance("persona", keys, new List<AttributesSet>() { attr }, null);

        var attKeys1 = new AttributesSet("ci", new HashSet<IData>() { new Data<int>(2) });
        var attr1 = new AttributesSet("nombre", new HashSet<IData>() { new Data<string>("luis") });
        var keys1 = new KeyClass(new List<AttributesSet>() { attKeys1 });
        manager.AddEntityInstance("persona", keys1, new List<AttributesSet>() { attr1 }, null);
        //2 representa mucbos
        manager.CreateRelation("padre", new HashSet<(string nameEntities, Cardinality)>() { ("persona", new Cardinality(2, 1)) });
        var list = new List<(string entidad, KeyClass llave)>();
        list.Add(("persona", keys));
        list.Add(("persona", keys1));

        manager.AddRelationInstance("padre", list);

    }
}