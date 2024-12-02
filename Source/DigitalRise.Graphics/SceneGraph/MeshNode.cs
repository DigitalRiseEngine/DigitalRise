// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Morphing;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Shapes;
using Newtonsoft.Json;
using System;
using System.ComponentModel;


namespace DigitalRise.SceneGraph
{
	/// <summary>
	/// Represents an instance of a mesh in a 3D scene.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Graphics.Mesh"/> describes the geometry and materials of a 3D object. A 
	/// <see cref="MeshNode"/> is used to position a mesh in a 3D scene. The mesh node defines its 
	/// position and orientation. Multiple mesh nodes can reference the same mesh, hence it is 
	/// possible to render the same mesh multiple times in a scene.
	/// </para>
	/// <para>
	/// <strong>Materials:</strong> Each mesh has one or more materials (see property
	/// <see cref="Graphics.Mesh.Materials"/>). When a mesh node is created from a mesh, a new 
	/// material instance (see class <see cref="MaterialInstance"/>) is created for each material.
	/// Each mesh node can override certain material properties defined in the base mesh. See 
	/// <see cref="MaterialInstance"/> for more details.
	/// </para>
	/// <para>
	/// <strong>Important:</strong> When the referenced mesh is changed, the mesh node can become
	/// invalid. Do not add or remove materials or submeshes to or from a mesh as long as the mesh is 
	/// referenced by any mesh nodes. When the affected mesh nodes are rendered they can cause 
	/// exceptions or undefined behavior.
	/// </para>
	/// <para>
	/// <strong>Cloning:</strong> When a <see cref="MeshNode"/> is cloned the 
	/// <see cref="MaterialInstances"/> are cloned (deep copy). But the <see cref="Mesh"/> and the
	/// <see cref="Skeleton"/> are only copied by reference (shallow copy). The original and the 
	/// cloned mesh node will reference the same <see cref="Graphics.Mesh"/> and the same
	/// <see cref="Skeleton"/>.
	/// </para>
	/// </remarks>
	public class MeshNode : MeshNodeBase
	{
		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		protected override Mesh RenderMesh => _mesh;

		/// <summary>
		/// Gets or sets the mesh.
		/// </summary>
		/// <value>The mesh.</value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is <see langword="null"/>.
		/// </exception>
		[Browsable(false)]
		[JsonIgnore]
		[Category("Graphics")]
		public Mesh Mesh
		{
			get { return _mesh; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				_mesh = value;

				SetHasAlpha();

				// Ensure that MorphWeights are set. (Required for correct rendering.)
				MorphWeights = value.HasMorphTargets() ? new MorphWeightCollection(value) : null;

				if (value == null)
				{
					Shape = Shape.Empty;
				}
				else
				{
					Shape = value.BoundingBox.CreateShape();
				}

				// Invalidate OccluderData.
				RenderData = null;
			}
		}
		private Mesh _mesh;

		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshNode"/> class.
		/// </summary>
		internal MeshNode()
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="MeshNode"/> class.
		/// </summary>
		/// <param name="mesh">The <see cref="Mesh"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="mesh"/> is <see langword="null"/>. 
		/// </exception>
		public MeshNode(Mesh mesh)
		{
			if (mesh == null)
				throw new ArgumentNullException("mesh");

			Mesh = mesh;
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing, bool disposeData)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					if (disposeData && _mesh != null)
						_mesh.Dispose();

					// The SkeletonPose may be shared between MeshNodes and therefore cannot
					// be recycled automatically.
				}

				base.Dispose(disposing, disposeData);
			}
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <summary>
		/// Checks if the MaterialInstances contain an InstanceAlpha parameter binding and stores the
		/// result in the SceneNode flags.
		/// </summary>
		protected void SetHasAlpha()
		{
			/*foreach (var materialInstance in MaterialInstances)
			{
				foreach (var effectBinding in materialInstance.EffectBindings)
				{
					foreach (var parameterBinding in effectBinding.ParameterBindings)
					{
						if (ReferenceEquals(parameterBinding.Description.Semantic, DefaultEffectParameterSemantics.InstanceAlpha)
							&& parameterBinding is ConstParameterBinding<float>)
						{
							// Found an InstanceAlpha parameter binding.
							SetFlag(SceneNodeFlags.HasAlpha);
							return;
						}
					}
				}
			}*/

			// No suitable InstanceAlpha parameter found.
			ClearFlag(SceneNodeFlags.HasAlpha);
		}

		#region ----- Cloning -----

		/// <inheritdoc cref="SceneNode.Clone"/>
		public new MeshNode Clone()
		{
			return (MeshNode)base.Clone();
		}


		/// <inheritdoc/>
		protected override SceneNode CreateInstanceCore()
		{
			return new MeshNode();
		}


		/// <inheritdoc/>
		protected override void CloneCore(SceneNode source)
		{
			// Clone SceneNode properties.
			base.CloneCore(source);

			// Clone MeshNode properties.
			var sourceTyped = (MeshNode)source;
			Mesh = sourceTyped.Mesh.Clone();
		}

		#endregion

		#endregion
	}
}
