using DigitalRise.ModelStorage.Binary;
using Microsoft.Xna.Framework;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public struct SkinJointContent
	{
		public int BoneIndex { get; set; }
		public Matrix InverseBindTransform { get; set; }
	}

	public class SkinContent : SerializableCollection<SkinJointContent>
	{
		protected override SkinJointContent LoadItem(BinaryReader reader)
		{
			return new SkinJointContent
			{
				BoneIndex = reader.ReadInt32(),
				InverseBindTransform = reader.ReadMatrix()
			};
		}

		protected override void SaveItem(BinaryWriter writer, SkinJointContent item)
		{
			writer.Write(item.BoneIndex);
			writer.Write(item.InverseBindTransform);
		}
	}
}
