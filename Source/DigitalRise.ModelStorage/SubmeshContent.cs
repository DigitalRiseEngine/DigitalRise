using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DigitalRise.ModelStorage
{
	public class SubmeshContent
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
	}
}
