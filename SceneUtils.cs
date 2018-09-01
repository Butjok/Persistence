using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Bulka
{
	static public class SceneUtils
	{
		static public IEnumerable<Scene> Scenes()
		{
			for (var i = 0; i < SceneManager.sceneCount; i++)
			{
				yield return SceneManager.GetSceneAt(i);
			}
		}

		static public IEnumerable<Scene> LoadedScenes()
		{
			return Scenes().Where(scene => scene.isLoaded);
		}

		static public List<T> FindObjectsOfType<T>(this Scene scene, bool includeInactive = false)
		{
			var objects = new List<T>();
			foreach (var go in scene.GetRootGameObjects())
			{
				objects.AddRange(go.GetComponentsInChildren<T>(includeInactive));
			}

			return objects;
		}
	}
}