using DigitalRise.Mathematics;
using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public class BoneContent : INamedObject, IBinarySerializable
	{
		public string Name { get; set; }
		public List<BoneContent> Children { get; } = new List<BoneContent>();
		public MeshContent Mesh { get; set; }
		public SrtTransform DefaultPose = SrtTransform.Identity;

		void IBinarySerializable.LoadFromBinary(BinaryReader br)
		{
			Name = br.ReadString();
			Mesh = br.ReadIfNotNull<MeshContent>();
			DefaultPose = br.ReadSrtTransform();
			Children.AddRange(br.ReadCollection<BoneContent>());
		}

		void IBinarySerializable.SaveToBinary(BinaryWriter bw)
		{
			bw.WriteString(Name);
			bw.WriteIfNotNull(Mesh);
			bw.Write(DefaultPose);
			bw.WriteCollection(Children);
		}
	}
}
