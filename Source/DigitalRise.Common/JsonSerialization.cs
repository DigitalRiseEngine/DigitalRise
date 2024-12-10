using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.IO;

namespace DigitalRise
{
	public static class JsonSerialization
	{
		public static JsonSerializerSettings CreateOptions()
		{
			var result = new JsonSerializerSettings
			{
				Culture = CultureInfo.InvariantCulture,
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto,
				DefaultValueHandling = DefaultValueHandling.Ignore,
			};

			result.Converters.Add(new StringEnumConverter());

			return result;
		}

		public static void SerializeToFile<T>(string path, T data)
		{
			var options = CreateOptions();
			var s = JsonConvert.SerializeObject(data, typeof(T), options);
			File.WriteAllText(path, s);
		}

		public static T DeserializeFromString<T>(string data)
		{
			var options = CreateOptions();
			return JsonConvert.DeserializeObject<T>(data, options);
		}
	}
}