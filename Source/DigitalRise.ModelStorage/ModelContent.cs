using System.Collections.Generic;

namespace DigitalRise.ModelStorage
{
	public class ModelContent
	{
		public BoneContent RootBone { get; set; }
		public List<VertexBufferContent> VertexBuffers { get; } = new List<VertexBufferContent>();
		public IndexBufferContent IndexBuffer { get; set; }
	}
}
