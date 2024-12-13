using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public struct SkinJointContent
	{
		public int BoneIndex { get; set; }
		public Matrix InverseBindTransform { get; set; }
	}

	public class SkinContent : IBinarySerializable
	{
		public List<SkinJointContent> Joints { get; } = new List<SkinJointContent>();

		void IBinarySerializable.LoadFromBinary(BinaryReader br)
		{
			var items = br.ReadCollection(reader =>
			{
				return new SkinJointContent
				{
					BoneIndex = reader.ReadInt32(),
					InverseBindTransform = reader.ReadMatrix()
				};
			});

			Joints.AddRange(items);
		}

		void IBinarySerializable.SaveToBinary(BinaryWriter bw)
		{
			bw.WriteCollection(Joints, (writer, item) =>
			{
				bw.Write(item.BoneIndex);
				bw.Write(item.InverseBindTransform);
			});
		}
	}
}
