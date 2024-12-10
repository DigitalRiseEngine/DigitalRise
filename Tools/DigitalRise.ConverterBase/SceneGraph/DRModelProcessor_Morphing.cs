// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using DigitalRise.Mathematics;
using DigitalRise.ModelStorage.Meshes;
using DigitalRise.ModelStorage.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace DigitalRise.ConverterBase.SceneGraph
{
	partial class DRModelProcessor
	{
		private static void AddMorphTarget(DRSceneNodeContent sceneNode, MeshContent morphTarget)
		{
			var meshNode = sceneNode as DRMeshNodeContent;
			if (meshNode == null)
			{
				string message = String.Format(
				  CultureInfo.InvariantCulture,
				  "Morph target \"{0}\" needs to be a child of the base mesh.",
				  morphTarget.Name);
				throw new InvalidContentException(message, morphTarget.Identity);
			}

			var meshEx = (MeshNodeEx)meshNode.UserData;
			if (meshEx == null)
			{
				meshEx = new MeshNodeEx();
				meshNode.UserData = meshEx;
			}

			if (meshEx.InputMorphTargets == null)
				meshEx.InputMorphTargets = new List<MeshContent>();

			meshEx.InputMorphTargets.Add(morphTarget);
		}


		private static DRVertexBufferContent CreateMorphTargetVertexBuffer()
		{
			var result = new DRVertexBufferContent();

			result.Elements.Add(new DRVertexElement(VertexElementUsage.Position, VertexElementFormat.Vector3));
			result.Elements.Add(new DRVertexElement(VertexElementUsage.Normal, VertexElementFormat.Vector3));

			return result;
		}


		private static void MakeRelativeMorphTargets(MeshContent baseMesh, List<MeshContent> morphTargets)
		{
			foreach (var morphTarget in morphTargets)
			{
				// Make positions relative to base mesh.
				// (Positions are stored in MeshContent.Positions.)
				var basePositions = baseMesh.Positions;
				var morphPositions = morphTarget.Positions;
				int numberOfPositions = basePositions.Count;
				for (int i = 0; i < numberOfPositions; i++)
					morphPositions[i] -= basePositions[i];

				// Make normals relative to base mesh.
				// (Normals are stored as a vertex channel per submesh.)
				int numberOfSubmeshes = baseMesh.Geometry.Count;
				for (int i = 0; i < numberOfSubmeshes; i++)
				{
					var baseGeometry = baseMesh.Geometry[i];
					var morphGeometry = morphTarget.Geometry[i];
					var baseNormals = baseGeometry.Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal());
					var morphNormals = morphGeometry.Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal());
					int numberOfNormals = baseNormals.Count;
					for (int j = 0; j < numberOfNormals; j++)
						morphNormals[j] -= baseNormals[j];
				}
			}
		}


		private static void AddVertexReorderChannel(MeshContent mesh)
		{
			foreach (var geometry in mesh.Geometry)
			{
				// Add the original vertex indices 0, 1, 2, ... n as a new vertex channel. When
				// the vertices are optimized the vertex channel contains the vertex reorder map.
				// The index needs to be stored as Byte4 because Int32 is not allowed in a
				// VertexChannelCollection.
				var vertexOrder = Enumerable.Range(0, geometry.Vertices.VertexCount)
											.Select(index => new Byte4 { PackedValue = (uint)index });
				geometry.Vertices.Channels.Add("VertexReorder", vertexOrder);
			}
		}


		// Gets vertex reorder maps for geometry, removes "VertexReorder" channel.
		private static int[][] GetVertexReorderMaps(MeshContent mesh)
		{
			int numberOfSubmeshes = mesh.Geometry.Count;
			int[][] vertexReorderMaps = new int[numberOfSubmeshes][];
			for (int i = 0; i < numberOfSubmeshes; i++)
			{
				var vertices = mesh.Geometry[i].Vertices;
				var vertexReorderChannel = vertices.Channels.Get<Byte4>("VertexReorder");

				vertexReorderMaps[i] = new int[vertexReorderChannel.Count];
				for (int j = 0; j < vertexReorderMaps[i].Length; j++)
					vertexReorderMaps[i][j] = (int)vertexReorderChannel[j].PackedValue;

				vertices.Channels.Remove(vertexReorderChannel);
			}

			return vertexReorderMaps;
		}


		private List<DRMorphTargetContent> BuildMorphTargets(GeometryContent geometry, List<MeshContent> inputMorphTargets, int index)
		{
			int[] vertexReorderMap = _vertexReorderMaps[index];

			var morphTargets = new List<DRMorphTargetContent>();
			foreach (var inputMorphTarget in inputMorphTargets)
			{
				int numberOfVertices = geometry.Vertices.VertexCount;
				var morphGeometry = inputMorphTarget.Geometry[index];

				// Copy relative positions and normals into vertex buffer.
				var positions = morphGeometry.Vertices.Positions;
				var normals = morphGeometry.Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal());
				Vector3[] data = new Vector3[numberOfVertices * 2];
				for (int i = 0; i < numberOfVertices; i++)
				{
					int originalIndex = vertexReorderMap[i];
					data[2 * i] = positions[originalIndex];
					data[2 * i + 1] = normals[originalIndex];
				}

				// Determine if morph target is empty.
				bool isEmpty = true;
				for (int i = 0; i < data.Length; i++)
				{
					// File formats and preprocessing can introduce some inaccuracies.
					// --> Use a relative large epsilon. (The default Numeric.EpsilonF is too small.)
					const float epsilon = 1e-4f;
					if (!Numeric.IsZero(data[i].LengthSquared(), epsilon * epsilon))
					{
						Debug.Write(string.Format(
						  CultureInfo.InvariantCulture,
						  "Morph target \"{0}\", submesh index {1}: Position/normal delta is {2}.",
						  inputMorphTarget.Name, index, data[i].Length()));

						isEmpty = false;
						break;
					}
				}

				if (!isEmpty)
				{
					// (Note: VertexStride is set explicitly in CreateMorphTargetVertexBuffer().)
					// ReSharper disable once PossibleInvalidOperationException

					int vertexOffset = _morphTargetVertexBuffer.VertexCount;
					var asBytes = MemoryMarshal.AsBytes<Vector3>(data);
					_morphTargetVertexBuffer.Write(_morphTargetVertexBuffer.SizeInBytes, asBytes);

					morphTargets.Add(new DRMorphTargetContent
					{
						Name = inputMorphTarget.Name,
						VertexBuffer = _morphTargetVertexBuffer,
						StartVertex = vertexOffset,
					});
				}
			}

			return (morphTargets.Count > 0) ? morphTargets : null;
		}
	}
}
