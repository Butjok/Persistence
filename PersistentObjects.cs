using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Bulka
{
	[ExecuteInEditMode]
	public class PersistentObjects : MonoBehaviour, IReadOnlyDictionary<string, PersistentBehaviour>
	{
		[Serializable]
		public class Table : SerializableDictionary<string, PersistentBehaviour> { }
		static private Table prefabs = new Table();
		static public Func<string> GuidGenerator = DefaultGuidGenerator;
		[TextArea(10, 20), SerializeField] private string debugJson = "";
		[SerializeField] private Table objects = new Table();
		public ILogger Logger = Debug.unityLogger;
		private HashSet<string> spawnedGuids = new HashSet<string>();

		[ContextMenu(nameof(RefreshTable))]
		public void RefreshTable()
		{
			var actual = SceneUtils.LoadedScenes()
				.SelectMany(scene => scene.FindObjectsOfType<PersistentBehaviour>())
				.ToList();

			// Remove objects from the table, if they are not present in the scene now.
			// This will remove null objects too, because actual does not hold references to null objects.
			foreach (var guid in objects.Keys.Where(guid => !actual.Contains(objects[guid])).ToList())
			{
				objects.Remove(guid);
			}

			// Add new objects to the table, with new guids.
			foreach (var obj in actual.Where(obj => !objects.ContainsValue(obj)))
			{
				objects[GuidGenerator()] = obj;
			}
		}

		public T Get<T>(string guid) where T : PersistentBehaviour
		{
			return (T)this[guid];
		}

		public void Load(string json)
		{
			RefreshTable();

			var headers = json.To<Dictionary<string, PersistentBehaviour.Header>>();
			var states = json.To<Dictionary<string, object>>();

			foreach (var guid in objects.Keys.Where(guid => !headers.ContainsKey(guid)).ToList())
			{
				var obj = objects[guid];
				objects.Remove(guid);

				Logger?.Log($"Destroying {obj}");
				DestroyImmediate(obj.gameObject);
			}

			spawnedGuids.Clear();
			foreach (var guid in headers.Keys.Where(guid => !objects.ContainsKey(guid)))
			{
				var header = headers[guid];

				// Get prefab
				PersistentBehaviour prefab;
				if (!prefabs.TryGetValue(header.PrefabName, out prefab) || !prefab)
				{
					var type = Type.GetType(header.TypeName);

					Assert.IsNotNull(type);
					Assert.IsTrue(type.IsSubclassOf(typeof(PersistentBehaviour)));

					prefab = prefabs[header.PrefabName] = (PersistentBehaviour)Resources.Load(header.PrefabName, type);

					Assert.IsNotNull(prefab, $"Cannot load prefab {header.PrefabName} of type {type}");
				}

				var old = prefab.gameObject.activeSelf;
				prefab.gameObject.SetActive(false);

				var instance = objects[guid] = Instantiate(prefab);
				instance.Prefab = prefab;
				states[guid].ToJson().Populate(instance);

				prefab.gameObject.SetActive(old);
				instance.gameObject.SetActive(old);

				Logger?.Log($"Instantiated {instance}");

				spawnedGuids.Add(guid);
			}

			foreach (var guid in objects.Keys.Where(guid => !spawnedGuids.Contains(guid)))
			{
				var obj = objects[guid];
				states[guid].ToJson().Populate(obj);

				Logger?.Log($"Populated {obj}");
			}
		}

		static public string DefaultGuidGenerator()
		{
			return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
		}

		[ContextMenu(nameof(DebugPrint))]
		private void DebugPrint()
		{
			Debug.Log(this.ToJson());
		}

		[ContextMenu(nameof(DebugLoad))]
		private void DebugLoad()
		{
			Load(debugJson);
		}

		protected void OnEnable()
		{
			if (Application.isEditor)
			{
				RefreshTable();
			}
		}

		public IEnumerator<KeyValuePair<string, PersistentBehaviour>> GetEnumerator()
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

		public bool TryGetValue(string key, out PersistentBehaviour value)
		{
			return objects.TryGetValue(key, out value);
		}

		public PersistentBehaviour this[string key] => objects[key];
		public IEnumerable<string> Keys => objects.Keys;
		public IEnumerable<PersistentBehaviour> Values => objects.Values;
	}
}