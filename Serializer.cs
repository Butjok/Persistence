using System.IO;
using Newtonsoft.Json;

static public class Serializer
{
	static public string ToJson(this object obj)
	{
		return JsonConvert.SerializeObject(obj, Formatting.Indented);
	}

	static public T To<T>(this string json)
	{
		return JsonConvert.DeserializeObject<T>(json);
	}

	static public void SaveTo(this object obj, string path)
	{
		File.WriteAllText(path, obj.ToJson());
	}

	static public T ReadFrom<T>(string path)
	{
		return File.ReadAllText(path).To<T>();
	}

	static public void Populate(this string json, object target)
	{
		JsonConvert.PopulateObject(json, target);
	}
}