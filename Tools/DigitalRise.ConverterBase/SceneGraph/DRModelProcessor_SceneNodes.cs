// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using DigitalRise.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using DigitalRise.ModelStorage.SceneGraph;


namespace DigitalRise.ConverterBase.SceneGraph
{
	partial class DRModelProcessor
	{
		// Initialize _model.
		private void BuildSceneGraph()
		{
			var root = BuildSceneGraph(_input, null);
			if (root == null)
			{
				// Just for safety. (Should not occur in practice.)
				throw new InvalidOperationException("Invalid root node.");
			}

			_model = new DRModelNodeContent();

			// In most cases the root node is an empty node, which can be ignored.
			if (root.GetType() == typeof(DRSceneNodeContent) &&
				!root.IsTransformed &&
				root.UserData == null)
			{
				// Throw away root, only use children.
				if (root.Children != null)
				{
					_model.Children = root.Children;
					foreach (var child in _model.Children)
						child.Parent = _model;
				}
			}
			else
			{
				_model.Children = new List<DRSceneNodeContent> { root };
				root.Parent = _model;
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		private static DRSceneNodeContent BuildSceneGraph(NodeContent node, DRSceneNodeContent parent)
		{
			CheckForCyclicReferences(node);

			DRSceneNodeContent sceneNode;
			if (node is BoneContent)
			{
				// ----- BoneContent
				// Bones do not show up in the scene graph.
				sceneNode = null;
			}
			else if (node is MeshContent)
			{
				// ----- MeshContent
				var mesh = (MeshContent)node;
				string morphTargetName;
				if (ContentHelper.IsMorphTarget(mesh, out morphTargetName))
				{
					// ----- Morph Targets
					// Morph targets are stored in the parent mesh, they do not show up in
					// the scene graph. Children of morph targets are ignored!
					mesh.Name = morphTargetName;
					AddMorphTarget(parent, mesh);
					sceneNode = null;
				}
				else if (ContentHelper.IsOccluder(mesh))
				{
					// ----- OccluderNode
					var meshEx = new MeshNodeEx
					{
						InputMesh = mesh
					};
					sceneNode = new DROccluderNodeContent { UserData = meshEx };
				}
				else
				{
					// ----- MeshNode
					var meshEx = new MeshNodeEx
					{
						InputMesh = mesh
					};
					sceneNode = new DRMeshNodeContent { UserData = meshEx };
				}
			}
			else
			{
				// ----- Empty/unsupported node
				sceneNode = new DRSceneNodeContent();
			}

			if (sceneNode != null)
			{
				sceneNode.Name = node.Name;
				Vector3 translation, scale;
				Quaternion rotation;

				node.Transform.Decompose(out scale, out rotation, out translation);
				sceneNode.Translation = translation;
				sceneNode.Scale = scale;
				sceneNode.Rotation = rotation;
				if (node.Children.Count > 0)
				{
					// Process children.
					sceneNode.Children = new List<DRSceneNodeContent>();

					// Recursively add children.
					foreach (var childNode in node.Children)
					{
						var childSceneNode = BuildSceneGraph(childNode, sceneNode);
						if (childSceneNode != null)
						{
							childSceneNode.Parent = sceneNode;
							sceneNode.Children.Add(childSceneNode);
						}
					}
				}
			}

			return sceneNode;
		}


		// Check for unallowed cyclic references.
		private static void CheckForCyclicReferences(NodeContent node)
		{
			if (TreeHelper.GetAncestors(node, n => n.Parent).Contains(node))
			{
				string message = String.Format(
				  CultureInfo.InvariantCulture,
				  "Cyclic reference (node \"{0}\") found in node hierarchy.",
				  node);
				throw new InvalidOperationException(message);
			}
		}
	}
}
