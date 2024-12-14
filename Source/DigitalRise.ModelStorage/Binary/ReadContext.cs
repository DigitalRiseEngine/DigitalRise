using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DigitalRise.ModelStorage.Binary
{
	internal class ReadContext
	{
		private struct BufferInfo
		{
			public int Offset;
			public int Size;
		}

		private readonly BinaryReader _reader;
		private readonly List<BufferInfo> _buffers = new List<BufferInfo>();

		public ReadContext(BinaryReader reader)
		{
			_reader = reader ?? throw new ArgumentNullException(nameof(reader));

			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				var chunk = reader.ReadInt32();
				if (chunk != ChunkTypes.BinaryChunkType)
				{
					break;
				}

				var bi = new BufferInfo
				{
					Size = reader.ReadInt32(),
					Offset = (int)reader.BaseStream.Position
				};
				_buffers.Add(bi);
				reader.BaseStream.Seek(bi.Size, SeekOrigin.Current);
			}
		}

		public byte[] ReadByteArray(int bufferId)
		{
			var b = _buffers[bufferId];
			_reader.BaseStream.Seek(b.Offset, SeekOrigin.Begin);

			return _reader.ReadBytes(b.Size);
		}

		public void ReadCollection<T>(int bufferId, Func<BinaryReader, T> reader, List<T> output)
		{
			var b = _buffers[bufferId];
			_reader.BaseStream.Seek(b.Offset, SeekOrigin.Begin);

			var itemSize = Marshal.SizeOf(typeof(T));

			var size = b.Size;
			if (size % itemSize != 0)
			{
				throw new Exception($"Inconsistent buffer size. Size={size}, itemSize={itemSize}");
			}

			for (var i = 0; i < size / itemSize; i++)
			{
				output.Add(reader(_reader));
			}
		}
	}
}
