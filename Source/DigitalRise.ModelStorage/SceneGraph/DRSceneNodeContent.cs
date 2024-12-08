﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.ComponentModel;
using DigitalRise.Linq;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using MathHelper = DigitalRise.Mathematics.MathHelper;


namespace DigitalRise.ModelStorage.SceneGraph
{
	/// <summary>
	/// Stores the processed data for a <strong>SceneNode</strong> asset.
	/// </summary>
	public class DRSceneNodeContent : INamedObject
	{
		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets the parent of this node.
		/// </summary>
		/// <value>The parent of this node.</value>
		[Browsable(false)]
		[JsonIgnore]
		public DRSceneNodeContent Parent { get; set; }


		/// <summary>
		/// Gets or sets the children of this node.
		/// </summary>
		/// <value>The children of this node.</value>
		public List<DRSceneNodeContent> Children { get; set; }


		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		public Vector3 Translation { get; set; }

		public Quaternion Rotation { get; set; } = Quaternion.Identity;

		public Vector3 Scale { get; set; } = Vector3.One;

		[Browsable(false)]
		[JsonIgnore]
		public bool IsTransformed
		{
			get
			{
				return !Translation.IsNumericallyZero() ||
					!MathHelper.AreNumericallyEqual(Rotation, Quaternion.Identity) ||
					!MathHelper.AreNumericallyEqual(Scale, Vector3.One);
			}
		}

		/// <summary>
		/// Gets or sets the maximum distance up to which the scene node is rendered.
		/// </summary>
		/// <value>The <i>view-normalized</i> distance. The default value is 0 (= no limit).</value>
		public float MaxDistance { get; set; }


		/// <summary>
		/// Gets or sets the LOD level.
		/// </summary>
		/// <value>The LOD level. The default value is 0.</value>
		public int LodLevel { get; set; }       // Only relevant for DRLodGroupNodeContent.


		/// <summary>
		/// Gets or sets the LOD distance.
		/// </summary>
		/// <value>The LOD distance. The default value is 0.</value>
		public float LodDistance { get; set; }  // Only relevant for DRLodGroupNodeContent.


		/// <summary>
		/// Gets or sets a user-defined tag object.
		/// </summary>
		/// <value>User-defined tag object.</value>
		[Browsable(false)]
		[JsonIgnore]
		public object UserData { get; set; }
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		#region ----- Traversal -----

		/// <summary>
		/// Gets the children of the given scene node.
		/// </summary>
		/// <returns>
		/// The children of scene node or an empty <see cref="IEnumerable{T}"/> if 
		/// <see cref="DRSceneNodeContent.Children"/> is <see langword="null"/>.
		/// </returns>
		public IEnumerable<DRSceneNodeContent> GetChildren()
		{
			return Children ?? LinqHelper.Empty<DRSceneNodeContent>();
		}


		/// <summary>
		/// Gets the root node.
		/// </summary>
		/// <returns>The root node.</returns>
		public DRSceneNodeContent GetRoot()
		{
			var node = this;
			while (node.Parent != null)
				node = node.Parent;

			return node;
		}


		/// <summary>
		/// Gets the ancestors of the given scene node.
		/// </summary>
		/// <returns>The ancestors of this scene node.</returns>
		public IEnumerable<DRSceneNodeContent> GetAncestors()
		{
			return TreeHelper.GetAncestors(this, node => node.Parent);
		}


		/// <summary>
		/// Gets the scene node and its ancestors scene.
		/// </summary>
		/// <returns>The scene node and its ancestors of the scene.</returns>
		public IEnumerable<DRSceneNodeContent> GetSelfAndAncestors()
		{
			return TreeHelper.GetSelfAndAncestors(this, node => node.Parent);
		}


		/// <overloads>
		/// <summary>
		/// Gets the descendants of the given node.
		/// </summary>
		/// </overloads>
		/// <summary>
		/// Gets the descendants of the given node using a depth-first search.
		/// </summary>
		/// <returns>
		/// The descendants of this node in depth-first order.
		/// </returns>
		public IEnumerable<DRSceneNodeContent> GetDescendants()
		{
			return TreeHelper.GetDescendants(this, node => node.GetChildren(), true);
		}


		/// <summary>
		/// Gets the descendants of the given node using a depth-first or a breadth-first search.
		/// </summary>
		/// <param name="depthFirst">
		/// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
		/// otherwise a breadth-first search will be made.
		/// </param>
		/// <returns>
		/// The descendants of this node.
		/// </returns>
		public IEnumerable<DRSceneNodeContent> GetDescendants(bool depthFirst)
		{
			return TreeHelper.GetDescendants(this, node => node.GetChildren(), depthFirst);
		}


		/// <overloads>
		/// <summary>
		/// Gets the subtree (the given node and all of its descendants).
		/// </summary>
		/// </overloads>
		/// <summary>
		/// Gets the subtree (the given node and all of its descendants) using a depth-first 
		/// search.
		/// </summary>
		/// <returns>
		/// The subtree (the given node and all of its descendants) in depth-first order.
		/// </returns>
		public IEnumerable<DRSceneNodeContent> GetSubtree()
		{
			return TreeHelper.GetSubtree(this, node => node.GetChildren(), true);
		}


		/// <summary>
		/// Gets the subtree (the given node and all of its descendants) using a depth-first or a 
		/// breadth-first search.
		/// </summary>
		/// <param name="depthFirst">
		/// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
		/// otherwise a breadth-first search will be made.
		/// </param>
		/// <returns>
		/// The subtree (the given node and all of its descendants).
		/// </returns>
		public IEnumerable<DRSceneNodeContent> GetSubtree(bool depthFirst)
		{
			return TreeHelper.GetSubtree(this, node => node.GetChildren(), depthFirst);
		}


		/// <summary>
		/// Gets the leaves of the scene node.
		/// </summary>
		/// <returns>The leaves of the scene node.</returns>
		public IEnumerable<DRSceneNodeContent> GetLeaves()
		{
			return TreeHelper.GetLeaves(this, node => node.GetChildren());
		}
		#endregion

		#endregion
	}
}
