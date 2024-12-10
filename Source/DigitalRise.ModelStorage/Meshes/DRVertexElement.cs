using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DigitalRise.ModelStorage.Meshes
{
	public struct DRVertexElement
	{
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public VertexElementUsage Usage { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public VertexElementFormat Format { get; set; }

		public int UsageIndex { get; set; }

		public DRVertexElement(VertexElementUsage usage, VertexElementFormat format, int usageIndex = 0)
		{
			Usage = usage;
			Format = format;
			UsageIndex = usageIndex;
		}
	}
}
