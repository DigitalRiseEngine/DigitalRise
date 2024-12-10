using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace DigitalRise.ModelStorage.Meshes
{
	public class DRVertexBufferContent
	{
		private int? _vertexStride;
		private readonly MemoryStream _stream = new MemoryStream();

		[Browsable(false)]
		[JsonIgnore]
		public int VertexStride
		{
			get
			{
				Update();
				return _vertexStride.Value;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public int MemorySizeInBytes => (int)_stream.Length;

		[Browsable(false)]
		[JsonIgnore]
		public int MemoryVertexCount => MemorySizeInBytes / VertexStride;

		public int BufferOffset { get; set; }

		public int VertexCount { get; set; }

		public ObservableCollection<DRVertexElement> Elements { get; } = new ObservableCollection<DRVertexElement>();

		public DRVertexBufferContent()
		{
			Elements.CollectionChanged += Channels_CollectionChanged;
		}

		private void Channels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_vertexStride = null;
		}

		public bool HasChannel(VertexElementUsage usage)
		{
			return (from c in Elements where c.Usage == usage select c).Count() > 0;
		}

		private void Update()
		{
			if (_vertexStride != null)
			{
				return;
			}

			_vertexStride = GetVertexStride();
		}

		private int GetVertexStride()
		{
			var result = 0;

			foreach (var channel in Elements)
			{
				result += GetTypeSize(channel.Format);
			}

			return result;
		}

		private static int GetTypeSize(VertexElementFormat elementFormat)
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
			return 0;
		}

		public void Write(int offset, ReadOnlySpan<byte> data)
		{
			_stream.Seek(offset, SeekOrigin.Begin);
			_stream.Write(data);
		}

		public byte[] GetMemoryData() => _stream.GetBuffer();
	}
}
