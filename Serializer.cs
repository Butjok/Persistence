using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Bulka
{
	static public class Serializer
	{
		public const string PrefabTag = "$prefab";

		static public string Serialize(object obj)
		{
			var jo = JObject.FromObject(obj);
			var persistent = obj as IPersistent;
			if (persistent != null)
			{
				jo[PrefabTag] = persistent.Prefab.name;
			}

			return jo.ToString(Formatting.Indented);
		}

		static public T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		static public string PrefabName(string json)
		{
			return (string)JObject.Parse(json)[PrefabTag];
		}

		static public void Populate(string json, MonoBehaviour target)
		{
			JsonConvert.PopulateObject(json, target);
		}
	}
}