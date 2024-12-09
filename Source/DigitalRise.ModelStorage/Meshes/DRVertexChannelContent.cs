using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitalRise.ModelStorage.Meshes
{
	public abstract class DRVertexChannelContentBase
	{
		public abstract VertexElementFormat Format { get; }
		public VertexElementUsage Usage { get; set; }
		public abstract int Count { get; }

		public DRVertexChannelContentBase()
		{
		}

		public DRVertexChannelContentBase(VertexElementUsage vertexElement)
		{
			Usage = vertexElement;
		}

		public abstract void Write(DRVertexChannelContentBase source);

		public static DRVertexChannelContentBase CreateChannel(VertexElementUsage usage, VertexElementFormat format)
		{
			DRVertexChannelContentBase channel;

			switch (format)
			{
				case VertexElementFormat.Single:
					channel = new DRVertexChannelContent<float>();
					break;
				case VertexElementFormat.Vector2:
					channel = new DRVertexChannelContent<Vector2>();
					break;
				case VertexElementFormat.Vector3:
					channel = new DRVertexChannelContent<Vector3>();
					break;
				case VertexElementFormat.Vector4:
					channel = new DRVertexChannelContent<Vector4>();
					break;
				case VertexElementFormat.Color:
					channel = new DRVertexChannelContent<Color>();
					break;
				case VertexElementFormat.Byte4:
					channel = new DRVertexChannelContent<Byte4>();
					break;
				case VertexElementFormat.Short2:
					channel = new DRVertexChannelContent<Short2>();
					break;
				case VertexElementFormat.Short4:
					channel = new DRVertexChannelContent<Short4>();
					break;
				case VertexElementFormat.NormalizedShort2:
					channel = new DRVertexChannelContent<NormalizedShort2>();
					break;
				case VertexElementFormat.NormalizedShort4:
					channel = new DRVertexChannelContent<NormalizedShort4>();
					break;
				case VertexElementFormat.HalfVector2:
					channel = new DRVertexChannelContent<HalfVector2>();
					break;
				case VertexElementFormat.HalfVector4:
					channel = new DRVertexChannelContent<HalfVector4>();
					break;
				default:
					throw new Exception($"Unknown vertex element type {format}");
			}

			channel.Usage = usage;

			return channel;
		}

		private static DRVertexChannelContentBase CreateAndCopy<T>(IList data)
		{
			var result = new DRVertexChannelContent<T>();

			var dataT = (IList<T>)data;
			for(var i = 0; i < dataT.Count; ++i)
			{
				result.Data.Add(dataT[i]);
			}

			return result;
		}

		public static DRVertexChannelContentBase CreateChannel(VertexElementUsage usage, Type t, IList data)
		{
			DRVertexChannelContentBase channel;

			if (t == typeof(float))
			{
				channel = CreateAndCopy<float>(data);
			}
			else if (t == typeof(Vector2))
			{
				channel = CreateAndCopy<Vector2>(data);
			}
			else if (t == typeof(Vector3))
			{
				channel = CreateAndCopy<Vector3>(data);
			}
			else if (t == typeof(Vector4))
			{
				channel = CreateAndCopy<Vector4>(data);
			}
			else if (t == typeof(Color))
			{
				channel = CreateAndCopy<Color>(data);
			}
			else if (t == typeof(Byte4))
			{
				channel = CreateAndCopy<Byte4>(data);
			}
			else if (t == typeof(Short2))
			{
				channel = CreateAndCopy<Short2>(data);
			}
			else if (t == typeof(Short4))
			{
				channel = CreateAndCopy<Short4>(data);
			}
			else if (t == typeof(NormalizedShort2))
			{
				channel = CreateAndCopy<NormalizedShort2>(data);
			}
			else if (t == typeof(NormalizedShort4))
			{
				channel = CreateAndCopy<NormalizedShort4>(data);
			}
			else if (t == typeof(HalfVector2))
			{
				channel = CreateAndCopy<HalfVector2>(data);
			}
			else if (t == typeof(HalfVector4))
			{
				channel = CreateAndCopy<HalfVector4>(data);
			}
			else
			{
				throw new Exception($"Unrecognized vertex content type: '{t}'");
			}

			channel.Usage = usage;

			return channel;
		}
	}

	public class DRVertexChannelContent<T> : DRVertexChannelContentBase
	{
		private VertexElementFormat _format;

		public List<T> Data { get; } = new List<T>();

		public override VertexElementFormat Format => _format;

		public override int Count
		{
			get
			{
				if (Data == null)
				{
					return 0;
				}

				return Data.Count;
			}
		}

		public DRVertexChannelContent()
		{
			SetFormat();
		}

		public DRVertexChannelContent(VertexElementUsage usage) : base(usage)
		{
			SetFormat();
		}

		private void SetFormat()
		{
			var t = typeof(T);
			if (t == typeof(float))
			{
				_format = VertexElementFormat.Single;
			}
			else if (t == typeof(Vector2))
			{
				_format = VertexElementFormat.Vector2;
			}
			else if (t == typeof(Vector3))
			{
				_format = VertexElementFormat.Vector3;
			}
			else if (t == typeof(Vector4))
			{
				_format = VertexElementFormat.Vector4;
			}
			else if (t == typeof(Color))
			{
				_format = VertexElementFormat.Color;
			}
			else if (t == typeof(Byte4))
			{
				_format = VertexElementFormat.Byte4;
			}
			else if (t == typeof(Short2))
			{
				_format = VertexElementFormat.Short2;
			}
			else if (t == typeof(Short4))
			{
				_format = VertexElementFormat.Short4;
			}
			else if (t == typeof(NormalizedShort2))
			{
				_format = VertexElementFormat.NormalizedShort2;
			}
			else if (t == typeof(NormalizedShort4))
			{
				_format = VertexElementFormat.NormalizedShort4;
			}
			else if (t == typeof(HalfVector2))
			{
				_format = VertexElementFormat.HalfVector2;
			}
			else if (t == typeof(HalfVector4))
			{
				_format = VertexElementFormat.HalfVector4;
			}
			else
			{
				throw new Exception($"Unrecognized vertex content type: '{t}'");
			}
		}

		public override void Write(DRVertexChannelContentBase source)
		{
			if (source.Format != Format)
			{
				throw new Exception($"Different channel types: source = {source.Format}, dest = {Format}");
			}

			var sourceT = (DRVertexChannelContent<T>)source;
			Data.AddRange(sourceT.Data);
		}
	}
}
