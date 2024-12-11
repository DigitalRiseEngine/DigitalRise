using DigitalRise.Mathematics;
using System.Collections.Generic;

namespace DigitalRise.ModelStorage
{
	public class BoneContent: INamedObject
	{
		public string Name { get; set; }
		public List<BoneContent> Children { get; } = new List<BoneContent>();
		public MeshContent Mesh { get; set; }
		public SrtTransform DefaultPose = SrtTransform.Identity;
	}
}
