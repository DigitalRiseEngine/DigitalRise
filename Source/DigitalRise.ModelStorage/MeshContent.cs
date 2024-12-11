using System.Collections.Generic;

namespace DigitalRise.ModelStorage
{
	public class MeshContent : INamedObject
	{
		public string Name { get; set; }
		public List<SubmeshContent> Submeshes { get; } = new List<SubmeshContent>();
	}
}
