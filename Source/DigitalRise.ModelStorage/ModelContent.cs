using DigitalRise.Mathematics;
using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public class ModelContent
	{
		public Dictionary<string, AnimationClipContent> Animations { get; } = new Dictionary<string, AnimationClipContent>();

		public BoneContent RootBone { get; set; }

		public List<VertexBufferContent> VertexBuffers { get; } = new List<VertexBufferContent>();

		public IndexBufferContent IndexBuffer { get; set; }

		public void Save(string folder, string name)
		{
			var output = Path.Combine(folder, name);

			var modelPath = Path.ChangeExtension(output, "jdrm");
			JsonSerialization.SerializeToFile(modelPath, this);
		}
	}
}
