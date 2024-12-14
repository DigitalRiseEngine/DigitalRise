// This code was borrowed from the MonoGame: https://github.com/MonoGame/MonoGame

using Assimp;
using DigitalRise.ModelStorage;
using Microsoft.Xna.Framework;
using System.IO;
using Quaternion = Microsoft.Xna.Framework.Quaternion;

namespace DigitalRise.ModelConverter
{
	internal static class Utils
	{
		public static Matrix ToXna(this Matrix4x4 matrix)
		{
			var result = Matrix.Identity;

			result.M11 = matrix.A1;
			result.M12 = matrix.B1;
			result.M13 = matrix.C1;
			result.M14 = matrix.D1;

			result.M21 = matrix.A2;
			result.M22 = matrix.B2;
			result.M23 = matrix.C2;
			result.M24 = matrix.D2;

			result.M31 = matrix.A3;
			result.M32 = matrix.B3;
			result.M33 = matrix.C3;
			result.M34 = matrix.D3;

			result.M41 = matrix.A4;
			result.M42 = matrix.B4;
			result.M43 = matrix.C4;
			result.M44 = matrix.D4;

			return result;
		}

		public static Vector2 ToXnaVector2(this Vector3D v) => new Vector2(v.X, v.Y);
		public static Vector3 ToXna(this Vector3D v) => new Vector3(v.X, v.Y, v.Z);
		public static Color ToXna(this Color4D v) => new Color(new Vector4(v.R, v.G, v.B, v.A));
		public static Quaternion ToXna(this Assimp.Quaternion v) => new Quaternion(v.X, v.Y, v.Z, v.W);

		public static void Write(this BinaryWriter writer, Vector2 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
		}

		public static void Write(this BinaryWriter writer, Vector3 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
			writer.Write(v.Z);
		}

		public static void Write(this BinaryWriter writer, Vector4 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
			writer.Write(v.Z);
			writer.Write(v.W);
		}

		public static void Write(this BinaryWriter writer, Color c)
		{
			writer.Write(c.PackedValue);
		}
	}
}
