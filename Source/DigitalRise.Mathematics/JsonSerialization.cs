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

		private static float ParseFloat(string s)
		{
			return float.Parse(s.Trim(), CultureInfo.InvariantCulture);
		}

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

				var p = s.Split(',');
				var result = new Quaternion(ParseFloat(p[0]), ParseFloat(p[1]), ParseFloat(p[2]), ParseFloat(p[3]));

				return result;
			}
		}

		public class MatrixConverter : JsonConverter<Matrix>
		{
			public static readonly MatrixConverter Instance = new MatrixConverter();

			private MatrixConverter()
			{

			}

			public override void WriteJson(JsonWriter writer, Matrix value, JsonSerializer serializer)
			{
				var str = string.Format(CultureInfo.InvariantCulture,
					"{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}",
					value.M11, value.M12, value.M13, value.M14,
					value.M21, value.M22, value.M23, value.M24,
					value.M31, value.M32, value.M33, value.M34,
					value.M41, value.M42, value.M43, value.M44);
				writer.WriteValue(str);
			}

			public override Matrix ReadJson(JsonReader reader, Type objectType, Matrix existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				string s = (string)reader.Value;

				var p = s.Split(',');

				var result = new Matrix(
					ParseFloat(p[0]), ParseFloat(p[1]), ParseFloat(p[2]), ParseFloat(p[3]),
					ParseFloat(p[4]), ParseFloat(p[5]), ParseFloat(p[6]), ParseFloat(p[7]),
					ParseFloat(p[8]), ParseFloat(p[9]), ParseFloat(p[10]), ParseFloat(p[11]),
					ParseFloat(p[12]), ParseFloat(p[13]), ParseFloat(p[14]), ParseFloat(p[15]));

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
			result.Converters.Add(MatrixConverter.Instance);

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