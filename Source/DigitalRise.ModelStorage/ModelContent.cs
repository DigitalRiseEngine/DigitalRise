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

			var binaryPath = Path.ChangeExtension(output, "bin");

			// Write binary
			using(var stream = File.OpenWrite(binaryPath))
			{
				for(var i = 0; i < VertexBuffers.Count; ++i)
				{
					var vertexBuffer = VertexBuffers[i];
					vertexBuffer.BufferOffset = (int)stream.Position;
					stream.Write(vertexBuffer.GetMemoryData());
				}

				if (IndexBuffer != null)
				{
					IndexBuffer.BufferOffset = (int)stream.Position;
					stream.Write(IndexBuffer.Data);
				}
			}

			var modelPath = Path.ChangeExtension(output, "jdrm");
			JsonSerialization.SerializeToFile(modelPath, this);
		}
	}
}
