using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DigitalRise.ModelStorage
{
	public static class Utility
	{
		public static int CalculateStride(this IEnumerable<VertexElementContent> elements)
		{
			var result = 0;

			foreach (var channel in elements)
			{
				result += GetSize(channel.Format);
			}

			return result;
		}

		public static int GetSize(this VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 4;
				case VertexElementFormat.Vector2:
					return 8;
				case VertexElementFormat.Vector3:
					return 12;
				case VertexElementFormat.Vector4:
					return 16;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 4;
				case VertexElementFormat.Short4:
					return 8;
				case VertexElementFormat.NormalizedShort2:
					return 4;
				case VertexElementFormat.NormalizedShort4:
					return 8;
				case VertexElementFormat.HalfVector2:
					return 4;
				case VertexElementFormat.HalfVector4:
					return 8;
			}

			throw new Exception($"Unknown vertex element format {elementFormat}");
		}

		public static int GetSize(this IndexElementSize indexType)
		{
			switch (indexType)
			{
				case IndexElementSize.SixteenBits:
					return 2;
				case IndexElementSize.ThirtyTwoBits:
					return 4;
			}

			throw new Exception($"Unknown index buffer type {indexType}");
		}
	}
}
