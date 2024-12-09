using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace DigitalRise.ModelStorage.Meshes
{
	public class DRVertexBufferContent
	{
		private int? _vertexStride;

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
		public int VertexCount
		{
			get
			{
				if (Channels == null || Channels.Count == 0)
				{
					return 0;
				}

				return Channels[0].Count;
			}
		}

		public ObservableCollection<DRVertexChannelContentBase> Channels { get; } = new ObservableCollection<DRVertexChannelContentBase>();

		public DRVertexBufferContent()
		{
			Channels.CollectionChanged += Channels_CollectionChanged;
		}

		private void Channels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_vertexStride = null;
		}

		public bool HasChannel(VertexElementUsage usage)
		{
			return (from c in Channels where c.Usage == usage select c).Count() > 0;
		}

		public DRVertexChannelContent<T> FindChannel<T>(VertexElementUsage usage)
		{
			foreach (var channel in Channels)
			{
				if (channel.Usage == usage)
				{
					return (DRVertexChannelContent<T>)channel;
				}
			}

			return null;
		}

		public DRVertexChannelContent<T> EnsureChannel<T>(VertexElementUsage usage)
		{
			var result = FindChannel<T>(usage);
			if (result == null)
			{
				throw new Exception($"Unable to find channel with usage {usage}");
			}

			return result;
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

			foreach (var channel in Channels)
			{
				result += GetTypeSize(channel.Format);
			}

			return result;
		}

		public void Write(DRVertexBufferContent source)
		{
			if (source.Channels.Count != Channels.Count)
			{
				throw new Exception($"Inconsistent channels count: source = {source.Channels.Count}, dest = {Channels.Count}");
			}

			if (source.VertexStride != VertexStride)
			{
				throw new Exception($"Inconsistent vertex stride: source = {source.VertexStride}, dest = {VertexStride}");
			}

			for (var i = 0; i < Channels.Count; ++i)
			{
				var sourceChannel = source.Channels[i];
				var channel = Channels[i];

				if (sourceChannel.Usage != channel.Usage)
				{
					throw new Exception($"Inconsistent channel usage: index = {i}, source = {sourceChannel.Usage}, dest = {channel.Usage}");
				}

				channel.Write(sourceChannel);
			}
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
	}
}
