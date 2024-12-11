using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace DigitalRise.ModelStorage
{
	public enum DRIndexType
	{
		UShort,
		UInt
	}

	public class IndexBufferContent
	{
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public DRIndexType IndexType { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
		public int IndexCount { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public byte[] Data { get; set; }

		public int BufferOffset { get; set; }

		public IndexBufferContent()
		{
		}


		public IndexBufferContent(List<uint> indices)
		{
			// Determine index type
			IndexType = DRIndexType.UShort;
			IndexCount = indices.Count;

			foreach (var idx in indices)
			{
				if (idx > ushort.MaxValue)
				{
					IndexType = DRIndexType.UInt;
					break;
				}
			}

			using (var ms = new MemoryStream())
			{
				if (IndexType == DRIndexType.UShort)
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
