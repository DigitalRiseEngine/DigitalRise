using DigitalRise.ModelStorage.Meshes;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage.SceneGraph
{
	public class DRModelNodeContent: DRSceneNodeContent
	{
		public List<DRVertexBufferContent> VertexBuffers { get; set; }
		public DRIndexBufferContent IndexBuffer { get; set; }

		public void Save(string folder, string name)
		{
			var outputPath = Path.Combine(folder, name);

			// Save binary
			var binaryPath = Path.ChangeExtension(outputPath, "bin");
			using (var stream = File.OpenWrite(binaryPath))
			{
				// Index Buffer
				if (IndexBuffer != null)
				{
					IndexBuffer.BufferOffset = (int)stream.Position;
					stream.Write(IndexBuffer.Data);
				}

				// Vertex Buffers
				if (VertexBuffers != null)
				{
					for (var i = 0; i < VertexBuffers.Count; ++i)
					{
						var vertexBuffer = VertexBuffers[i];

						vertexBuffer.BufferOffset = (int)stream.Position;
						vertexBuffer.VertexCount = vertexBuffer.MemoryVertexCount;

						stream.Write(vertexBuffer.GetMemoryData());
					}
				}
			}

			// Save model json
			var modelPath = Path.ChangeExtension(outputPath, "jdrm");
			JsonSerialization.SerializeToFile(modelPath, this);
		}
	}
}
