using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace DigitalRise.ModelStorage
{
	public class SubmeshContent
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public PrimitiveType PrimitiveType { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int VertexBufferIndex { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int StartVertex { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int VertexCount { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int StartIndex { get; set; }

		public SkinContent Skin { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int PrimitiveCount { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int MaterialIndex { get; set; }

		public BoundingBox BoundingBox { get; set; }
	}
}
