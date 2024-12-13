using DigitalRise.ModelStorage.Binary;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace DigitalRise.ModelStorage
{
	public class VertexBufferContent
	{
		private int? _vertexStride;
		private byte[] _data;
		private readonly MemoryStream _stream = new MemoryStream();

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int BufferId { get; set; }

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

		public int VertexCount { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public byte[] Data
		{
			get
			{
				if (_data == null)
				{
					_data = _stream.ToArray();
				}

				return _data;
			}

			set
			{
				_data = value;
			}
		}

		public ObservableCollection<VertexElementContent> Elements { get; set; } = new ObservableCollection<VertexElementContent>();

		public VertexBufferContent()
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

			_vertexStride = Elements.CalculateStride();
		}

		public void Write(ReadOnlySpan<byte> data)
		{
			_stream.Write(data);
			_data = null;
		}

		public byte[] GetMemoryData() => _stream.GetBuffer();

		internal void LoadBinaryData(ReadContext ctx)
		{
			Data = ctx.ReadByteArray(BufferId);

			if (Data.Length % VertexStride != 0)
			{
				throw new Exception($"Inconsistent data size. Data.Length={Data.Length}, VertexStride={VertexStride}");
			}

			VertexCount = Data.Length / VertexStride;
		}

		internal void SaveBinaryData(WriteContext ctx)
		{
			VertexCount = MemoryVertexCount;
			BufferId = ctx.WriteData(Data);
		}
	}
}
