using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public class MeshContent : IBinarySerializable
	{
		public List<SubmeshContent> Submeshes { get; } = new List<SubmeshContent>();

		void IBinarySerializable.LoadFromBinary(BinaryReader br)
		{
			Submeshes.AddRange(br.ReadCollection<SubmeshContent>());
		}

		void IBinarySerializable.SaveToBinary(BinaryWriter bw)
		{
			bw.WriteCollection(Submeshes);
		}
	}
}
