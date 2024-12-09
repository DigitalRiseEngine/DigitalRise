using DigitalRise.ModelStorage.Meshes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
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

		public static DRVertexBufferContent ToDRVertexBufferContent(this VertexContent vertexContent)
		{
			var result = new DRVertexBufferContent();

			var destChannel = DRVertexChannelContentBase.CreateChannel(VertexElementUsage.Position, typeof(Vector3), vertexContent.Positions);
			result.Channels.Add(destChannel);

			foreach (var sourceChannel in vertexContent.Channels)
			{
				VertexElementUsage usage;
				if (!VertexChannelNames.TryDecodeUsage(sourceChannel.Name, out usage))
				{
					throw new Exception($"Unable to decode usage for channel {sourceChannel.Name}");
				}

				destChannel = DRVertexChannelContentBase.CreateChannel(usage, sourceChannel.ElementType, sourceChannel);
				result.Channels.Add(destChannel);
			}

			return result;
		}
	}
}
