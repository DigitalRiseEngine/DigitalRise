using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DigitalRise.ModelStorage.Binary
{
	internal class WriteContext
	{
		private readonly BinaryWriter _writer;
		private int _lastId = 0;


		public WriteContext(BinaryWriter writer)
		{
			_writer = writer ?? throw new ArgumentNullException(nameof(writer));
		}

		public int WriteData(byte[] data)
		{
			var id = _lastId;

			_writer.Write(ChunkTypes.BinaryChunkType);
			_writer.Write(data.Length);
			_writer.Write(data);

			++_lastId;

			return id;
		}

		public int WriteCollection<T>(IReadOnlyCollection<T> collection, Action<BinaryWriter, T> writer)
		{
			var id = _lastId;

			_writer.Write(ChunkTypes.BinaryChunkType);

			var itemSize = Marshal.SizeOf(typeof(T));
			_writer.Write(collection.Count * itemSize);

			foreach (var item in collection)
			{
				writer(_writer, item);
			}

			++_lastId;

			return id;
		}
	}
}
