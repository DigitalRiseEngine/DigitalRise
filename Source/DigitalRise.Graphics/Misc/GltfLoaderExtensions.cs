using DigitalRise.Data.Modelling;
using Microsoft.Xna.Framework;
using static glTFLoader.Schema.Accessor;
using static glTFLoader.Schema.AnimationSampler;

namespace DigitalRise.Misc
{
	internal static class GltfLoaderExtensions
	{
		private static readonly int[] ComponentsCount = new[]
		{
			1,
			2,
			3,
			4,
			4,
			9,
			16
		};

		private static readonly int[] ComponentSizes = new[]
		{
			sizeof(sbyte),
			sizeof(byte),
			sizeof(short),
			sizeof(ushort),
			0,	// There's no such component
			sizeof(uint),
			sizeof(float)
		};

		public static int GetComponentCount(this TypeEnum type) => ComponentsCount[(int)type];
		public static int GetComponentSize(this ComponentTypeEnum type) => ComponentSizes[(int)type - 5120];

		public static Vector3 ToVector3(this float[] array) => new Vector3(array[0], array[1], array[2]);
		public static Vector4 ToVector4(this float[] array) => new Vector4(array[0], array[1], array[2], array[3]);
		public static Quaternion ToQuaternion(this float[] array) => new Quaternion(array[0], array[1], array[2], array[3]);

		public static Matrix ToMatrix(this float[] array) =>
			new Matrix(array[0], array[1], array[2], array[3],
				array[4], array[5], array[6], array[7],
				array[8], array[9], array[10], array[11],
				array[12], array[13], array[14], array[15]);

		public static InterpolationMode ToInterpolationMode(this InterpolationEnum v)
		{
			switch (v)
			{
				case InterpolationEnum.LINEAR:
					return InterpolationMode.Linear;
				case InterpolationEnum.CUBICSPLINE:
					return InterpolationMode.Cubic;
			}

			return InterpolationMode.None;
		}
	}
}
