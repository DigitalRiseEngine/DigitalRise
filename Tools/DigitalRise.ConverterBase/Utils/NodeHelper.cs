using DigitalRise.ModelStorage.Meshes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;

namespace DigitalRise.ConverterBase.Utils
{
	internal static class NodeHelper
	{
		public static void GetNodesOfType<T>(this NodeContent root, List<T> result) where T : NodeContent
		{
			var asT = root as T;
			if (asT != null)
			{
				result.Add(asT);
			}

			if (root.Children != null)
			{
				foreach (var child in root.Children)
				{
					child.GetNodesOfType(result);
				}
			}
		}

		public static DRVertexChannelContentBase CreateChannel<T>(VertexElementUsage usage, VertexChannel sourceChannelBase)
		{

			var result = new DRVertexChannelContent<T>(usage);

			var sourceChannel = (VertexChannel<T>)sourceChannelBase;
			for (var i = 0; i < sourceChannel.Count; ++i)
			{
				result.Data.Add(sourceChannel[i]);
			}

			return result;
		}


		public static DRSubmeshContent ToDRSubmeshContent(this GeometryContent geometryContent)
		{
			var result = new DRSubmeshContent();
			var vertexBuffer = new DRVertexBufferContent();
			foreach (var sourceChannel in geometryContent.Vertices.Channels)
			{
				VertexElementUsage usage;
				if (!VertexChannelNames.TryDecodeUsage(sourceChannel.Name, out usage))
				{
					throw new Exception($"Unknown vertex element usage for channel '{sourceChannel.Name}'");
				}

				DRVertexChannelContentBase channel;
				if (sourceChannel.ElementType == typeof(float))
				{
					channel = CreateChannel<float>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(Vector2))
				{
					channel = CreateChannel<Vector2>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(Vector3))
				{
					channel = CreateChannel<Vector3>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(Vector4))
				{
					channel = CreateChannel<Vector4>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(Color))
				{
					channel = CreateChannel<Color>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(Byte4))
				{
					channel = CreateChannel<Byte4>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(Short2))
				{
					channel = CreateChannel<Short2>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(Short4))
				{
					channel = CreateChannel<Short4>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(NormalizedShort2))
				{
					channel = CreateChannel<NormalizedShort2>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(NormalizedShort4))
				{
					channel = CreateChannel<NormalizedShort4>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(HalfVector2))
				{
					channel = CreateChannel<HalfVector2>(usage, sourceChannel);
				}
				else if (sourceChannel.ElementType == typeof(HalfVector4))
				{
					channel = CreateChannel<HalfVector4>(usage, sourceChannel);
				}
				else
				{
					throw new Exception($"Unrecognized vertex content type: '{sourceChannel.ElementType}'");
				}

				vertexBuffer.Channels.Add(channel);
			}

			result.VertexBuffer = vertexBuffer;
			result.VertexCount = vertexBuffer.VertexCount;

			foreach (var index in geometryContent.Indices)
			{
				result.Indices.Add(index);
			}

			result.PrimitiveCount = result.Indices.Count / 3;

			return result;
		}

		public static DRMeshContent ToDRMeshContent(this MeshContent meshContent)
		{
			var result = new DRMeshContent();

			result.Positions.AddRange(meshContent.Positions);

			foreach (var geometry in meshContent.Geometry)
			{
				result.Submeshes.Add(ToDRSubmeshContent(geometry));
			}

			return result;
		}

		public static DRVertexBufferContent ToDRVertexBufferContent(this VertexContent vertexContent)
		{
			var result = new DRVertexBufferContent();

			foreach(var sourceChannel in vertexContent.Channels)
			{
				VertexElementUsage usage;
				if (!VertexChannelNames.TryDecodeUsage(sourceChannel.Name, out usage))
				{
					throw new Exception($"Unable to decode usage for channel {sourceChannel.Name}");
				}

				var destChannel = DRVertexChannelContentBase.CreateChannel(usage, sourceChannel.ElementType, sourceChannel);

				result.Channels.Add(destChannel);
			}

			return result;
		}
	}
}
