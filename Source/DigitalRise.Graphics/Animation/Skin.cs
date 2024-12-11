using System;
using Microsoft.Xna.Framework;

namespace DigitalRise.Animation
{
	public class SkinJoint
	{
		public int BoneIndex { get; }
		public Matrix InverseBindTransform { get; }

		public SkinJoint(int boneIndex, Matrix inverseBindTransform)
		{
			if (boneIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(boneIndex));
			}

			BoneIndex = boneIndex;
			InverseBindTransform = inverseBindTransform;
		}
	}

	public class Skin : INamedObject
	{
		public string Name { get; set; }
		public SkinJoint[] Joints { get; }
		public int SkinIndex { get; internal set; }

		/// <summary>
		/// Creates a skin from array of joints
		/// </summary>
		/// <param name="joints"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public Skin(SkinJoint[] joints)
		{
			if (joints == null)
			{
				throw new ArgumentNullException(nameof(joints));
			}

			if (joints.Length == 0)
			{
				throw new ArgumentException(nameof(joints), "no joints");
			}

			Joints = joints;
		}
	}
}