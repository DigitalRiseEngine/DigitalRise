using DigitalRise.ModelStorage.Meshes;
using System.Collections.Generic;

namespace DigitalRise.ModelStorage.SceneGraph
{
	public class DRModelNodeContent: DRSceneNodeContent
	{
		public List<DRVertexBufferContent> VertexBuffers { get; set; }
		public DRIndexBufferContent IndexBuffer { get; set; }
	}
}
