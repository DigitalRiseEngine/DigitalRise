using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace DigitalRise.ModelStorage
{
	public class IndexBufferContent
	{
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public IndexElementSize IndexType { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public int IndexCount { get; set; }

		public byte[] Data { get; set; }


		public IndexBufferContent()
		{
		}


		public IndexBufferContent(List<uint> indices)
		{
			// Determine index type
			IndexType = IndexElementSize.SixteenBits;
			IndexCount = indices.Count;

			foreach (var idx in indices)
			{
				if (idx > ushort.MaxValue)
				{
					IndexType = IndexElementSize.ThirtyTwoBits;
					break;
				}
			}

			using (var ms = new MemoryStream())
			{
				if (IndexType == IndexElementSize.SixteenBits)
				{
					var indicesShort = new ushort[indices.Count];
					for (var i = 0; i < indicesShort.Length; ++i)
					{
						indicesShort[i] = (ushort)indices[i];
					}

					var bytes = MemoryMarshal.AsBytes<ushort>(indicesShort);
					ms.Write(bytes);
				}
				else
				{
					var bytes = MemoryMarshal.AsBytes<uint>(indices.ToArray());
					ms.Write(bytes);
				}

				Data = ms.ToArray();
			}
		}
	}
}
