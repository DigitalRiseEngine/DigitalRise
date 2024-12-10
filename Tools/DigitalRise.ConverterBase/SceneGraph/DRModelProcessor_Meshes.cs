// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DigitalRise.Mathematics;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using DigitalRise.ModelStorage.Meshes;
using DigitalRise.ModelStorage.SceneGraph;


namespace DigitalRise.ConverterBase.SceneGraph
{
	partial class DRModelProcessor
	{
		//--------------------------------------------------------------
		#region Nested Types
		//--------------------------------------------------------------

		private class MeshNodeEx
		{
			public MeshContent InputMesh;
			public List<MeshContent> InputMorphTargets;
		}

		private class SubmeshInfo
		{
			public GeometryContent Geometry;
			public int OriginalIndex;                 // The index of the Submesh/GeometryContent in the owning mesh.
			public VertexBufferContent VertexBuffer;
			public int VertexBufferIndex;             // Index into _vertexBuffers of the processor.
			public List<DRMorphTargetContent> MorphTargets;
			public object Material;                   // The XML file (string) or the local material (MaterialContent).
			public int MaterialIndex;
		}


		private class SubmeshInfoComparer : Singleton<SubmeshInfoComparer>, IComparer<SubmeshInfo>
		{
			public int Compare(SubmeshInfo x, SubmeshInfo y)
			{
				int result = x.VertexBufferIndex - y.VertexBufferIndex;

				if (result == 0)
					result = x.MaterialIndex - y.MaterialIndex;

				if (result == 0)
					result = x.OriginalIndex - y.OriginalIndex;

				return result;
			}
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		private void BuildMeshes()
		{
			_vertexBuffers = new List<DRVertexBufferContent>();
			_indices = new List<int>();
			_morphTargetVertexBuffer = CreateMorphTargetVertexBuffer();

			var meshNodes = _model.GetSubtree().OfType<DRMeshNodeContent>();
			foreach (var meshNode in meshNodes)
			{
				BuildMesh(meshNode);
			}

			_model.VertexBuffers = _vertexBuffers;
			_model.IndexBuffer = new DRIndexBufferContent(_indices);
		}


		private void BuildMesh(DRMeshNodeContent meshNode)
		{
			var meshEx = (MeshNodeEx)meshNode.UserData;

			var mesh = meshEx.InputMesh;
			var meshDescription = (_modelDescription != null) ? _modelDescription.GetMeshDescription(mesh.Name) : null;

			// Before modifying the base mesh: Prepare morph targets.
			bool hasMorphTargets = (meshEx.InputMorphTargets != null && meshEx.InputMorphTargets.Count > 0);
			if (hasMorphTargets)
			{
				// Convert absolute morph targets to relative morph targets ("delta blend shapes").
				MakeRelativeMorphTargets(mesh, meshEx.InputMorphTargets);

				// Add "VertexReorder" channel to base mesh.
				AddVertexReorderChannel(mesh);
			}

			if (meshDescription != null)
			{
				meshNode.MaxDistance = meshDescription.MaxDistance;
				meshNode.LodDistance = meshDescription.LodDistance;
			}
			else if (_modelDescription != null)
			{
				meshNode.MaxDistance = _modelDescription.MaxDistance;
				meshNode.LodDistance = 0;
			}

			// Ensure that model has tangents and binormals if required.
			AddTangentFrames(mesh, _modelDescription, meshDescription);

			// Process vertex colors, bone weights and bone indices.
			ProcessVertexChannels(mesh);

			if (_modelDescription != null && _modelDescription.SwapWindingOrder)
				MeshHelper.SwapWindingOrder(mesh);

			OptimizeForCache(mesh);

			if (hasMorphTargets)
			{
				// Get the vertex reorder maps for matching the morph targets with the
				// base mesh. (Removes the "VertexReorder" channel from the geometry.)
				_vertexReorderMaps = GetVertexReorderMaps(mesh);
			}

			var submeshInfos = BuildSubmeshInfos(mesh, meshEx.InputMorphTargets);

			// Sort submeshes by vertex declaration, material, and original index.
			Array.Sort(submeshInfos, SubmeshInfoComparer.Instance);

			// Build submeshes (including materials).
			var submeshes = BuildSubmeshes(mesh, submeshInfos);

			// Calculate a bounding shape for the whole mesh.
			var boundingShape = BuildBoundingShape(meshNode);

			meshNode.Mesh = new DRMeshContent
			{
				Name = mesh.Name,
				BoundingShape = boundingShape,
				Submeshes = submeshes,
				Skeleton = _skeleton,
				Animations = _animations,
			};
		}


		private void AddTangentFrames(MeshContent mesh, ModelDescription modelDescription, MeshDescription meshDescription)
		{
			string textureCoordinateChannelName = VertexChannelNames.TextureCoordinate(0);
			string tangentChannelName = VertexChannelNames.Tangent(0);
			string binormalChannelName = VertexChannelNames.Binormal(0);

			bool normalsCalculated = false;
			for (int i = 0; i < mesh.Geometry.Count; i++)
			{
				var geometry = mesh.Geometry[i];

				// Check whether tangent frames are required.
				var submeshDescription = (meshDescription != null) ? meshDescription.GetSubmeshDescription(i) : null;
				if (submeshDescription != null && submeshDescription.GenerateTangentFrames
					|| meshDescription != null && meshDescription.GenerateTangentFrames
					|| modelDescription != null && modelDescription.GenerateTangentFrames)
				{
					// Ensure that normals are set.
					if (!normalsCalculated)
					{
						CalculateNormals(mesh, false);
						normalsCalculated = true;
					}

					var channels = geometry.Vertices.Channels;
					bool tangentsMissing = !channels.Contains(tangentChannelName);
					bool binormalsMissing = !channels.Contains(binormalChannelName);
					if (tangentsMissing || binormalsMissing)
					{
						// Texture coordinates are required for calculating tangent frames.
						if (!channels.Contains(textureCoordinateChannelName))
						{
							Log(string.Format("Texture coordinates missing in mesh '{0}', submesh {1}. Texture coordinates are required " +
							  "for calculating tangent frames.", mesh.Name, i));

							channels.Add<Vector2>(textureCoordinateChannelName, null);
						}

						CalculateTangentFrames(
						  geometry,
						  textureCoordinateChannelName,
						  tangentsMissing ? tangentChannelName : null,
						  binormalsMissing ? binormalChannelName : null);
					}
				}
			}
		}


		// Build a SubmeshInfo for each GeometryContent.
		private SubmeshInfo[] BuildSubmeshInfos(MeshContent mesh, List<MeshContent> inputMorphs)
		{
			bool hasMorphTargets = (inputMorphs != null && inputMorphs.Count > 0);

			// A lookup table that maps each material to its index.
			// The key is the name of the XML file (string) or the local material (MaterialContent).
			var materialLookupTable = new Dictionary<object, int>();

			int numberOfSubmeshes = mesh.Geometry.Count;
			var submeshInfos = new SubmeshInfo[numberOfSubmeshes];
			for (int i = 0; i < numberOfSubmeshes; i++)
			{
				var geometry = mesh.Geometry[i];

				// Build morph targets for current submesh.
				List<DRMorphTargetContent> morphTargets = null;
				if (hasMorphTargets)
				{
					morphTargets = BuildMorphTargets(geometry, inputMorphs, i);
					if (morphTargets != null && morphTargets.Count > 0)
					{
						// When morph targets are used remove the "BINORMAL" channel. (Otherwise,
						// the number of vertex attributes would exceed the limit. Binormals need
						// to be reconstructed from normal and tangent in the vertex shader.)
						string binormalName = VertexChannelNames.Binormal(0);
						bool containsTangentFrames = geometry.Vertices.Channels.Remove(binormalName);

						if (containsTangentFrames)
						{
							// A submesh cannot use vertex colors and tangents at the same time.
							// This would also exceed the vertex attribute limit.
							string colorName = VertexChannelNames.Color(0);
							if (geometry.Vertices.Channels.Contains(colorName))
								geometry.Vertices.Channels.Remove(colorName);
						}
					}
				}

				var submeshInfo = new SubmeshInfo
				{
					Geometry = geometry,
					OriginalIndex = i,
					VertexBuffer = geometry.Vertices.CreateVertexBuffer(),
					MorphTargets = morphTargets
				};
				submeshInfo.VertexBufferIndex = GetVertexBufferIndex(submeshInfo.VertexBuffer.VertexDeclaration);

				// Get material file or local material.
				object material = geometry.Material;
				if (material == null)
				{
					var message = string.Format(CultureInfo.InvariantCulture, "Mesh \"{0}\" does not have a material.", mesh);
					throw new InvalidContentException(message, mesh.Identity);
				}

				int materialIndex;
				if (!materialLookupTable.TryGetValue(material, out materialIndex))
				{
					materialIndex = materialLookupTable.Count;
					materialLookupTable.Add(material, materialIndex);
				}

				submeshInfo.MaterialIndex = materialIndex;
				submeshInfo.Material = material;

				submeshInfos[i] = submeshInfo;
			}

			return submeshInfos;
		}


		// Returns the index of the vertex buffer (in _vertexBuffers) for the given vertex declaration.
		// If there is no matching vertex buffer, a new vertex buffer is added to _vertexBuffers.
		private int GetVertexBufferIndex(VertexDeclarationContent vertexDeclaration)
		{
			for (int i = 0; i < _vertexBuffers.Count; i++)
			{
				var otherVertexDeclaration = _vertexBuffers[i];

				// Compare vertex element count.
				if ((otherVertexDeclaration.Elements.Count != vertexDeclaration.VertexElements.Count))
					continue;

				int? vertexStride = vertexDeclaration.VertexStride;
				int? otherVertexStride = otherVertexDeclaration.VertexStride;

				// Compare vertex strides.
				if (vertexStride.GetValueOrDefault() != otherVertexStride.GetValueOrDefault())
					continue;

				// Check if either both have a vertex stride or not.
				if (vertexStride.HasValue == otherVertexStride.HasValue)
					continue;

				// Compare each vertex element structure.
				bool matchFound = true;
				for (int j = 0; j < otherVertexDeclaration.Elements.Count; j++)
				{
					var vertexElement = vertexDeclaration.VertexElements[j];
					var otherVertexElement = otherVertexDeclaration.Elements[j];
					if (vertexElement.VertexElementUsage != otherVertexElement.Usage ||
						vertexElement.VertexElementFormat != otherVertexElement.Format ||
						vertexElement.UsageIndex != otherVertexElement.UsageIndex)
					{
						matchFound = false;
						break;
					}
				}

				if (matchFound)
					return i;
			}

			// An identical vertex declaration has not been found.
			// --> Add vertex declaration to list.

			var newVertexBuffer = new DRVertexBufferContent();
			for (var i = 0; i < vertexDeclaration.VertexElements.Count; ++i)
			{
				var vertexElement = vertexDeclaration.VertexElements[i];

				newVertexBuffer.Elements.Add(new DRVertexElement(vertexElement.VertexElementUsage, vertexElement.VertexElementFormat, vertexElement.UsageIndex));
			}

			_vertexBuffers.Add(newVertexBuffer);

			return _vertexBuffers.Count - 1;
		}


		private List<DRSubmeshContent> BuildSubmeshes(MeshContent mesh, SubmeshInfo[] submeshInfos)
		{
			var submeshes = new List<DRSubmeshContent>(mesh.Geometry.Count);
			for (int i = 0; i < submeshInfos.Length; i++)
			{
				var submeshInfo = submeshInfos[i];
				var geometry = submeshInfo.Geometry;

				// Append vertices to one of the _vertexBuffers.
				DRVertexBufferContent vertexBuffer = null;
				int vertexCount = 0;
				int vertexOffset = 0;
				if (submeshInfo.VertexBuffer.VertexData.Length > 0)
				{
					vertexBuffer = _vertexBuffers[submeshInfo.VertexBufferIndex];
					if (vertexBuffer.VertexStride == 0)
					{
						string message = string.Format(CultureInfo.InvariantCulture, "Vertex declaration of \"{0}\" does not have a vertex stride.", mesh);
						throw new InvalidContentException(message, mesh.Identity);
					}

					vertexCount = submeshInfo.Geometry.Vertices.VertexCount;
					vertexOffset = vertexBuffer.VertexCount;
					vertexBuffer.Write(vertexBuffer.SizeInBytes, submeshInfo.VertexBuffer.VertexData);
				}

				// Append indices to _indices.
				int startIndex = 0;
				int primitiveCount = 0;
				if (geometry.Indices.Count > 0)
				{
					startIndex = _indices.Count;
					primitiveCount = geometry.Indices.Count / 3;
					_indices.AddRange(geometry.Indices);
				}

				// Create Submesh.
				DRSubmeshContent submesh = new DRSubmeshContent
				{
					VertexCount = vertexCount,
					StartIndex = startIndex,
					PrimitiveCount = primitiveCount,
					VertexBufferIndex = submeshInfo.VertexBufferIndex,
					StartVertex = vertexOffset,
					MorphTargets = submeshInfo.MorphTargets,
				};
				submeshes.Add(submesh);
			}

			return submeshes;
		}


		private Shape BuildBoundingShape(DRMeshNodeContent meshNode)
		{
			Shape boundingShape = Shape.Empty;

			var meshEx = (MeshNodeEx)meshNode.UserData;
			var mesh = meshEx.InputMesh;
			if (mesh.Positions.Count > 0)
			{
				if (_modelDescription != null && _modelDescription.BoundingBoxEnabled)
				{
					// We assume that the AABB is given in the local space.
					Vector3 aabbMinimum = (Vector3)_modelDescription.BoundingBoxMinimum;
					Vector3 aabbMaximum = (Vector3)_modelDescription.BoundingBoxMaximum;
					Vector3 center = (aabbMaximum + aabbMinimum) / 2;
					Vector3 extent = aabbMaximum - aabbMinimum;
					if (center.IsNumericallyZero())
						boundingShape = new BoxShape(extent);
					else
						boundingShape = new TransformedShape(new BoxShape(extent), new Pose(center));
				}
				else
				{
					// Best fit bounding shape.
					//boundingShape = ComputeBestFitBoundingShape(mesh);

					// Non-rotated bounding shape. This is usually larger but contains no rotations. 
					// (TransformedShapes with rotated children cannot be used with non-uniform scaling.)
					boundingShape = ComputeAxisAlignedBoundingShape(mesh);
				}
			}

			return boundingShape;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private static Shape ComputeBestFitBoundingShape(MeshContent mesh)
		{
			List<Vector3> points = mesh.Positions.Select(position => (Vector3)position).ToList();

			var boundingShape = GeometryHelper.CreateBoundingShape(points);

			//  // Compute minimal sphere.
			//  Vector3 center;
			//  float radius;
			//  GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
			//  SphereShape sphere = new SphereShape(radius);
			//  float sphereVolume = sphere.GetVolume();

			//  // Compute minimal box.
			//  Vector3 boxExtent;
			//  Pose boxPose;
			//  GeometryHelper.ComputeBoundingBox(points, out boxExtent, out boxPose);
			//  var box = new BoxShape(boxExtent);
			//  float boxVolume = box.GetVolume();

			//  // Return the object with the smallest volume.
			//  // A TransformedShape is used if the shape needs to be translated or rotated.
			//  if (sphereVolume < boxVolume)
			//  {
			//    if (center.IsNumericallyZero)
			//      boundingShape = sphere;
			//    else
			//      boundingShape = new TransformedShape(new GeometricObject(sphere, new Pose(center)));
			//  }
			//  else
			//  {
			//    if (!boxPose.HasTranslation && !boxPose.HasRotation)
			//      boundingShape = box;
			//    else
			//      boundingShape = new TransformedShape(new GeometricObject(box, boxPose));
			//  }
			//}
			//else
			//{
			//  boundingShape = Shape.Empty;
			//}

			return boundingShape;
		}


		private static Shape ComputeAxisAlignedBoundingShape(MeshContent mesh)
		{
			Debug.Assert(mesh.Positions.Count > 0);

			List<Vector3> points = mesh.Positions.Select(position => (Vector3)position).ToList();

			var boundingShape = GeometryHelper.CreateBoundingShape(points);

			// Compute minimal sphere.
			Vector3 center;
			float radius;
			GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
			SphereShape sphere = new SphereShape(radius);
			float sphereVolume = sphere.GetVolume();

			// Compute minimal AABB.
			BoundingBox aabb = new BoundingBox(points[0], points[0]);
			for (int i = 1; i < points.Count; i++)
				aabb.Grow(points[i]);
			var boxPose = new Pose(aabb.Center());
			var box = new BoxShape(aabb.Extent());
			float boxVolume = box.GetVolume();

			// Return the object with the smallest volume.
			// A TransformedShape is used if the shape needs to be translated.
			if (sphereVolume < boxVolume)
			{
				if (center.IsNumericallyZero())
					boundingShape = sphere;
				else
					boundingShape = new TransformedShape(sphere, new Pose(center));
			}
			else
			{
				if (!boxPose.HasTranslation)
					boundingShape = box;
				else
					boundingShape = new TransformedShape(box, boxPose);
			}

			return boundingShape;
		}
		#endregion
	}
}
