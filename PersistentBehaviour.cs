using Newtonsoft.Json;
using UnityEngine;

namespace Bulka
{
	[JsonObject(MemberSerialization.OptIn)]
	public class PersistentBehaviour : MonoBehaviour
	{
		public class Header
		{
			public const string TagPrefab = "$prefab";
			public const string TagType = "$type";
			[JsonProperty(TagPrefab)] public string PrefabName;
			[JsonProperty(TagType)] public string TypeName;
		}
		public PersistentBehaviour Prefab;
		[JsonProperty(Header.TagPrefab)] public string PrefabName => Prefab?.name;
		[JsonProperty(Header.TagType)] public string TypeName => GetType().FullName;
	}
}