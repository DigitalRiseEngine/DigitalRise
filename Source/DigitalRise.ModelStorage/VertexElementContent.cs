using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DigitalRise.ModelStorage
{
	public struct VertexElementContent
	{
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public VertexElementUsage Usage { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public VertexElementFormat Format { get; set; }

		public int UsageIndex { get; set; }

		public VertexElementContent(VertexElementUsage usage, VertexElementFormat format, int usageIndex = 0)
		{
			Usage = usage;
			Format = format;
			UsageIndex = usageIndex;
		}

		public override string ToString() => $"{Usage}, {Format}, {UsageIndex}";

		public static bool Equals(VertexElementContent a, VertexElementContent b)
		{
			return a.Usage == b.Usage && a.Format == b.Format && a.UsageIndex == b.UsageIndex;
		}

		public static bool operator ==(VertexElementContent a, VertexElementContent b) => Equals(a, b);
		public static bool operator !=(VertexElementContent a, VertexElementContent b) => !Equals(a, b);
	}
}
