using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;
using System.IO;

namespace DigitalRise.Mathematics
{
	public static class JsonSerialization
	{
		private static readonly StringEnumConverter _stringEnumConverter = new StringEnumConverter();

		public class QuaternionConverter : JsonConverter<Quaternion>
		{
			public static readonly QuaternionConverter Instance = new QuaternionConverter();

			private QuaternionConverter()
			{

			}

			public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.X, value.Y, value.Z, value.W);
				writer.WriteValue(str);
			}

			public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				string s = (string)reader.Value;

				var parts = s.Split(',');

				var result = new Quaternion(
					float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
					float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture),
					float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture),
					float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture));

				return result;
			}
		}



		public static JsonSerializerSettings CreateOptions()
		{
			var result = new JsonSerializerSettings
			{
				Culture = CultureInfo.InvariantCulture,
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto,
				DefaultValueHandling = DefaultValueHandling.Ignore,
			};

			result.Converters.Add(_stringEnumConverter);
			result.Converters.Add(QuaternionConverter.Instance);

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