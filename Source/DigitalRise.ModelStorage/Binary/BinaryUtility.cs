using Microsoft.Xna.Framework;
using System.IO;

namespace DigitalRise.ModelStorage.Binary
{
	internal static class BinaryUtility
	{
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


		public static Vector3 ReadVector3(this BinaryReader r) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

		public static Quaternion ReadQuaternion(this BinaryReader r) => new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

		public static Matrix ReadMatrix(this BinaryReader r) =>
			new Matrix(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
				r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
				r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
				r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
	}
}
