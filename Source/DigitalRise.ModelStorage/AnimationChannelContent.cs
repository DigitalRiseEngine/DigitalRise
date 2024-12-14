using DigitalRise.ModelStorage.Binary;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DigitalRise.ModelStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
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

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
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

	public class VectorKeyframeChannelContent : SerializableCollection<VectorKeyframeContent>
	{
		protected override VectorKeyframeContent LoadItem(BinaryReader reader)
		{
			return new VectorKeyframeContent(reader.ReadDouble(), reader.ReadVector3());
		}

		protected override void SaveItem(BinaryWriter writer, VectorKeyframeContent item)
		{
			writer.Write(item.Time);
			writer.Write(item.Value);
		}
	}

	public class QuaternionKeyframeChannelContent : SerializableCollection<QuaternionKeyframeContent>
	{
		protected override QuaternionKeyframeContent LoadItem(BinaryReader reader)
		{
			return new QuaternionKeyframeContent(reader.ReadDouble(), reader.ReadQuaternion());
		}

		protected override void SaveItem(BinaryWriter writer, QuaternionKeyframeContent item)
		{
			writer.Write(item.Time);
			writer.Write(item.Value);
		}
	}

	public class AnimationChannelContent
	{
		public int BoneIndex { get; set; }
		public VectorKeyframeChannelContent Scales { get; set; } = new VectorKeyframeChannelContent();
		public QuaternionKeyframeChannelContent Rotations { get; set; } = new QuaternionKeyframeChannelContent();
		public VectorKeyframeChannelContent Translations { get; set; } = new VectorKeyframeChannelContent();


		internal void LoadBinaryData(ReadContext ctx)
		{
			Scales.LoadBinaryData(ctx);
			Rotations.LoadBinaryData(ctx);
			Translations.LoadBinaryData(ctx);
		}

		internal void SaveBinaryData(WriteContext ctx)
		{
			Scales.SaveBinaryData(ctx);
			Rotations.SaveBinaryData(ctx);
			Translations.SaveBinaryData(ctx);
		}
	}
}
