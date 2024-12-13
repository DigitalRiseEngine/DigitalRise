using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public class AnimationClipContent : INamedObject, IBinarySerializable
	{
		public string Name { get; set; }
		public List<AnimationChannelContent> Channels { get; } = new List<AnimationChannelContent>();

		void IBinarySerializable.LoadFromBinary(BinaryReader br)
		{
			Name = br.ReadString();
			Channels.AddRange(br.ReadCollection<AnimationChannelContent>());
		}

		void IBinarySerializable.SaveToBinary(BinaryWriter bw)
		{
			bw.WriteString(Name);
			bw.WriteCollection(Channels);
		}
	}
}
