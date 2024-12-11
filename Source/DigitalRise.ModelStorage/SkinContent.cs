using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DigitalRise.ModelStorage
{
	public struct SkinJointContent
	{
		public int BoneIndex { get; set; }
		public Matrix InverseBindTransform { get; set; }
	}

	public class SkinContent
	{
		public List<SkinJointContent> Joints { get; } = new List<SkinJointContent>();
	}
}
