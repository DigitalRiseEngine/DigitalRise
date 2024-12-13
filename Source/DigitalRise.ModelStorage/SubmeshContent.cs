using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public class SubmeshContent : IBinarySerializable
	{
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public PrimitiveType PrimitiveType { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public int VertexBufferIndex { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public int StartVertex { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public int VertexCount { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public int StartIndex { get; set; }

		public SkinContent Skin { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public int PrimitiveCount { get; set; }
		public BoundingBox BoundingBox { get; set; }

		void IBinarySerializable.LoadFromBinary(BinaryReader br)
		{
			PrimitiveType = (PrimitiveType)br.ReadInt32();
			VertexBufferIndex = br.ReadInt32();
			StartVertex = br.ReadInt32();
			VertexCount = br.ReadInt32();
			StartIndex = br.ReadInt32();
			PrimitiveCount = br.ReadInt32();
			Skin = br.ReadIfNotNull<SkinContent>();
		}

		void IBinarySerializable.SaveToBinary(BinaryWriter bw)
		{
			bw.Write((int)PrimitiveType);
			bw.Write(VertexBufferIndex);
			bw.Write(StartVertex);
			bw.Write(VertexCount);
			bw.Write(StartIndex);
			bw.Write(PrimitiveCount);
			bw.WriteIfNotNull(Skin);
		}
	}
}
