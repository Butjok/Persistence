using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PersistentTracker : MonoBehaviour, IReadOnlyDictionary<string, MonoBehaviour>
{
	[Serializable]
	public class Dict : SerializableDictionary<string, MonoBehaviour> { }
	[SerializeField] private Dict objects = new Dict();

	static public string NewGuid()
	{
		return Guid.NewGuid().ToString();
	}

	[ContextMenu(nameof(Refresh))]
	public void Refresh()
	{
		var actual = FindObjectsOfType<MonoBehaviour>().Where(obj => obj is IPersistent).ToList();
		foreach (var pair in objects.Where(pair => !actual.Contains(pair.Value)).ToList())
		{
			objects.Remove(pair.Key);
		}

		foreach (var obj in actual.Where(obj => !objects.ContainsValue(obj)))
		{
			objects[NewGuid()] = obj;
		}
	}

	public T Get<T>(string guid) where T : MonoBehaviour
	{
		return (T)this[guid];
	}

	protected void OnEnable()
	{
		if (Application.isEditor)
		{
			Refresh();
		}
	}

	public IEnumerator<KeyValuePair<string, MonoBehaviour>> GetEnumerator()
	{
		return objects.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)objects).GetEnumerator();
	}

	public int Count => objects.Count;

	public bool ContainsKey(string key)
	{
		return objects.ContainsKey(key);
	}

	public bool TryGetValue(string key, out MonoBehaviour value)
	{
		return objects.TryGetValue(key, out value);
	}

	public MonoBehaviour this[string key] => objects[key];
	public IEnumerable<string> Keys => objects.Keys;
	public IEnumerable<MonoBehaviour> Values => objects.Values;
}