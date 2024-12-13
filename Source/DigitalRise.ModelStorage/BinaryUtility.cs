using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage
{
	internal static class BinaryUtility
	{
		public static void WriteByteArray(this BinaryWriter bw, byte[] data)
		{
			bw.Write(data.Length);
			bw.Write(data);
		}

		public static void WriteString(this BinaryWriter bw, string s)
		{
			if (s == null)
			{
				s = string.Empty;
			}

			bw.Write(s);
		}

		public static void Write(this BinaryWriter bw, Vector3 v)
		{
			bw.Write(v.X);
			bw.Write(v.Y);
			bw.Write(v.Z);
		}


		public static void Write(this BinaryWriter bw, Quaternion v)
		{
			bw.Write(v.X);
			bw.Write(v.Y);
			bw.Write(v.Z);
			bw.Write(v.W);
		}

		public static void Write(this BinaryWriter bw, Matrix v)
		{
			bw.Write(v.M11);
			bw.Write(v.M12);
			bw.Write(v.M13);
			bw.Write(v.M14);

			bw.Write(v.M21);
			bw.Write(v.M22);
			bw.Write(v.M23);
			bw.Write(v.M24);

			bw.Write(v.M31);
			bw.Write(v.M32);
			bw.Write(v.M33);
			bw.Write(v.M34);

			bw.Write(v.M41);
			bw.Write(v.M42);
			bw.Write(v.M43);
			bw.Write(v.M44);
		}


		public static void Write(this BinaryWriter bw, SrtTransform transform)
		{
			bw.Write(transform.Scale);
			bw.Write(transform.Rotation);
			bw.Write(transform.Translation);
		}

		public static void WriteCollection<T>(this BinaryWriter bw, ICollection<T> col, Action<BinaryWriter, T> serializer)
		{
			if (col == null)
			{
				bw.Write(0);
				return;
			}

			bw.Write(col.Count);

			foreach (var item in col)
			{
				serializer(bw, item);
			}
		}

		public static void WriteCollection<T>(this BinaryWriter bw, ICollection<T> col) where T : IBinarySerializable
		{
			bw.WriteCollection(col, (bw, item) =>
			{
				item.SaveToBinary(bw);
			});
		}

		public static void WriteIfNotNull(this BinaryWriter bw, IBinarySerializable item)
		{
			if (item != null)
			{
				bw.Write(1);
				item.SaveToBinary(bw);
			}
			else
			{
				bw.Write(0);
			}
		}

		public static byte[] ReadByteArray(this BinaryReader r)
		{
			var size = r.ReadInt32();

			return r.ReadBytes(size);
		}

		public static Vector3 ReadVector3(this BinaryReader r) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

		public static Quaternion ReadQuaternion(this BinaryReader r) => new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

		public static Matrix ReadMatrix(this BinaryReader r) =>
			new Matrix(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
				r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
				r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
				r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

		public static SrtTransform ReadSrtTransform(this BinaryReader r) => new SrtTransform(r.ReadVector3(), r.ReadQuaternion(), r.ReadVector3());


		public static List<T> ReadCollection<T>(this BinaryReader r, Func<BinaryReader, T> serializer)
		{
			var result = new List<T>();
			var count = r.ReadInt32();
			if (count == 0)
			{
				return result;
			}

			for (var i = 0; i < count; ++i)
			{
				result.Add(serializer(r));
			}

			return result;
		}

		public static List<T> ReadCollection<T>(this BinaryReader r) where T : IBinarySerializable, new()
		{
			return r.ReadCollection(br =>
			{
				var result = new T();
				result.LoadFromBinary(br);

				return result;
			});
		}

		public static T ReadIfNotNull<T>(this BinaryReader r) where T : class, IBinarySerializable, new()
		{
			var count = r.ReadInt32();
			if (count == 0)
			{
				return null;
			}

			var result = new T();
			result.LoadFromBinary(r);

			return result;
		}
	}
}
