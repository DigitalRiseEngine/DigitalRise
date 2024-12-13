using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

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

	public class AnimationChannelContent : IBinarySerializable
	{
		public int BoneIndex { get; set; }
		public List<VectorKeyframeContent> Scales { get; } = new List<VectorKeyframeContent>();
		public List<QuaternionKeyframeContent> Rotations { get; } = new List<QuaternionKeyframeContent>();
		public List<VectorKeyframeContent> Translations { get; } = new List<VectorKeyframeContent>();

		void IBinarySerializable.LoadFromBinary(BinaryReader br)
		{
			BoneIndex = br.ReadInt32();

			Scales.AddRange(br.ReadCollection(reader => new VectorKeyframeContent(reader.ReadDouble(), reader.ReadVector3())));
			Rotations.AddRange(br.ReadCollection(reader => new QuaternionKeyframeContent(reader.ReadDouble(), reader.ReadQuaternion())));
			Translations.AddRange(br.ReadCollection(reader => new VectorKeyframeContent(reader.ReadDouble(), reader.ReadVector3())));
		}

		void IBinarySerializable.SaveToBinary(BinaryWriter bw)
		{
			bw.Write(BoneIndex);

			bw.WriteCollection(Scales, (bw, item) =>
			{
				bw.Write(item.Time);
				bw.Write(item.Value);
			});

			bw.WriteCollection(Rotations, (bw, item) =>
			{
				bw.Write(item.Time);
				bw.Write(item.Value);
			});

			bw.WriteCollection(Translations, (bw, item) =>
			{
				bw.Write(item.Time);
				bw.Write(item.Value);
			});
		}
	}
}
