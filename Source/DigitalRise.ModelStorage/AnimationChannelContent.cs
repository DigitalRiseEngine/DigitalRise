using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DigitalRise.ModelStorage
{
	public struct VectorKeyframeContent
	{
		public double Time;
		public Vector3 Value;

		public VectorKeyframeContent(double time, Vector3 value)
		{
			Time = time;
			Value = value;
		}
	}

	public struct QuaternionKeyframeContent
	{
		public double Time;
		public Quaternion Value;

		public QuaternionKeyframeContent(double time, Quaternion value)
		{
			Time = time;
			Value = value;
		}
	}

	public class AnimationChannelContent
	{
		public int BoneIndex { get; set; }
		public List<VectorKeyframeContent> Translations { get; } = new List<VectorKeyframeContent>();
		public List<VectorKeyframeContent> Scales { get; } = new List<VectorKeyframeContent>();
		public List<QuaternionKeyframeContent> Rotations { get; } = new List<QuaternionKeyframeContent>();
	}
}
