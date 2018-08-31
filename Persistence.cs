using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

static public class Persistence
{
	static private Dictionary<string, object> prefabs = new Dictionary<string, object>();

	static public T Instantiate<T>(string json) where T : MonoBehaviour, IPersistent
	{
		var prefabName = Serializer.PrefabName(json);

		object _prefab;
		if (!prefabs.TryGetValue(prefabName, out _prefab) || _prefab == null)
		{
			_prefab = prefabs[prefabName] = Resources.Load<T>(prefabName);
			Assert.IsNotNull(_prefab, $"cannot load prefab {prefabName}");
		}

		var prefab = (T)_prefab;

		var old = prefab.gameObject.activeSelf;
		prefab.gameObject.SetActive(false);
		var instance = Object.Instantiate(prefab);
		prefab.gameObject.SetActive(old);

		instance.Prefab = prefab;
		Serializer.Populate(json, instance);
		instance.gameObject.SetActive(old);

		return instance;
	}
}