﻿// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#if ANIMATION
using DigitalRise.Animation.Character;
#endif

namespace DigitalRise.ConverterBase.SceneGraph
{
	/// <summary>
	/// Processes a game asset mesh to a model content that is optimal for runtime.
	/// </summary>
	public partial class DRModelProcessor
	{
		// Notes:
		// - Mesh instancing: 
		//   The XNA content pipeline does not directly support instanced meshes. If instanced
		//   meshes are added in the future, we have to modify the DRModelProcessor to make 
		//   sure that instanced meshes are not processed twice (DRMeshNodeContent.InputMesh 
		//   can be shared.)
		// - Name "DRModelProcessor":
		//   XNA will show an error if it finds two processors with the same name in a content 
		//   project. Therefore, this processor cannot be simply named "ModelProcessor".

		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		// Input
		private NodeContent _input;

		// The model (= root node of the scene).
		private DRModelNodeContent _model;

		// Skeleton and animations
		private BoneContent _rootBone;
#if ANIMATION
    private Skeleton _skeleton;
    private Dictionary<string, SkeletonKeyFrameAnimation> _animations;
#endif

		// Vertex and index buffers
		private List<VertexBufferContent> _vertexBuffers;     // One vertex buffer for each VertexDeclaration.
		private IndexCollection _indices;                     // One index buffer for everything.
		private VertexBufferContent _morphTargetVertexBuffer; // One vertex buffer for all morph targets.
		int[][] _vertexReorderMaps;                           // Vertex reorder map to match morph target with base mesh.

		// Processed Materials:
		// If material is defined in external XML file:
		//   Key: string
		//   Value: ExternalReference<DRMaterialContent>
		// If local material is used:
		//   Key: MaterialContent
		//   Value: DRMaterialContent
		private Dictionary<object, object> _materials;
		#endregion

		#region Properties

		public Action<string> Logger { get; set; }

		public bool GenerateTangentFrames { get; set; }
		public bool SwapWindingOrder { get; set; }
		public bool PremultiplyVertexColors { get; set; }
		public AnimationDescription Animation { get; set; }

		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <summary>
		/// Converts mesh content to model content.
		/// </summary>
		/// <param name="input">The root node content.</param>
		/// <returns>The model content.</returns>
		public DRModelNodeContent Process(NodeContent input)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			// The content processor may write text files. We want to use invariant culture number formats.
			// TODO: Do not set Thread.CurrentThread.CurrentCulture. Make sure that all read/write operations explicitly use InvariantCulture.
			var originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			try
			{
				// Uncomment this to launch and attach a debugger.
				//System.Diagnostics.Debugger.Launch();

				// Uncomment this to visualize the content tree.
				//ContentHelper.PrintContentTree(input, context);

				// The model was imported.
				_input = input;

				ValidateInput();

				// Try to find skeleton root bone.
				_rootBone = MeshHelper.FindSkeleton(input);
				if (_rootBone != null)
				{
#if ANIMATION
          MergeAnimationFiles();
#endif
					BakeTransforms(input);
#if ANIMATION
          BuildSkeleton();
          BuildAnimations();
#endif
				}

				BuildSceneGraph();
				BuildMeshes();
				BuildOccluders();
				CombineLodGroups();
				ValidateOutput();

				_model.Name = _input.Name;
			}
			finally
			{
				// Clean up.
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}

			return _model;
		}


		/// <summary>
		/// Bakes all node transforms of all skinned meshes into the geometry so that each node's
		/// transform is Identity. (Only bones and morph targets keep their transforms.)
		/// </summary>
		/// <param name="node">The node.</param>
		private static void BakeTransforms(NodeContent node)
		{
			if (node is BoneContent)
				return;
			if (ContentHelper.IsMorphTarget(node))
				return;

			if (ContentHelper.IsSkinned(node))
			{
				// Bake all transforms in this subtree.
				BakeAllTransforms(node);
			}
			else
			{
				// Bake transforms of skinned meshes.
				foreach (NodeContent child in node.Children)
					BakeTransforms(child);
			}
		}


		/// <summary>
		/// Bakes all node transforms in the specified subtree into the mesh geometry so that each
		/// node's transform is Identity. (Only bones and morph targets keep their transforms.)
		/// </summary>
		/// <param name="node">The node.</param>
		private static void BakeAllTransforms(NodeContent node)
		{
			if (node is BoneContent)
				return;
			if (ContentHelper.IsMorphTarget(node))
				return;

			if (node.Transform != Matrix.Identity)
			{
				Microsoft.Xna.Framework.Content.Pipeline.Graphics.MeshHelper.TransformScene(node, node.Transform);
				node.Transform = Matrix.Identity;
			}

			foreach (NodeContent child in node.Children)
				BakeAllTransforms(child);
		}


		#endregion
	}
}
