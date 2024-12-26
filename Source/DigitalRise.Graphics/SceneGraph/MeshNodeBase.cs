// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Morphing;
using DigitalRise.Rendering.Deferred;
using DigitalRise.SceneGraph.Occlusion;
using Newtonsoft.Json;

namespace DigitalRise.SceneGraph
{
	public abstract class MeshNodeBase : SceneNode, IOcclusionProxy
	{
		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets the mesh.
		/// </summary>
		/// <value>The mesh.</value>
		protected abstract Mesh RenderMesh { get; }

		/// <summary>
		/// Gets or sets the weights of the morph targets.
		/// </summary>
		/// <value>
		/// The weights of the morph targets. The default value depends on whether the mesh includes
		/// morph targets. If the mesh includes morph targets an empty 
		/// <see cref="MorphWeightCollection"/> (all weights are 0) is set by default; otherwise, 
		/// <see langword="null"/>.
		/// </value>
		/// <remarks>
		/// <para>
		/// A <see cref="MorphWeightCollection"/> is required, if the <see cref="RenderMesh"/> includes morph
		/// targets; otherwise, rendering may fail.
		/// </para>
		/// <para>
		/// The <see cref="MeshNode"/> does not verify whether the <see cref="MorphWeights"/> is
		/// compatible with the <see cref="RenderMesh"/>. The <see cref="MorphWeights"/> may include other
		/// morph targets than the <see cref="RenderMesh"/>. In this case only the morph targets that match
		/// are applied to the mesh during morph target animation.
		/// </para>
		/// </remarks>
		[Browsable(false)]
		[JsonIgnore]
		[Category("Animation")]
		public MorphWeightCollection MorphWeights
		{
			get { return _morphWeights; }
			set
			{
				if (RenderMesh.HasMorphTargets())
				{
					if (value == null)
						throw new GraphicsException("MorphWeights cannot be null because the mesh includes morph targets.");
				}
				else
				{
					if (value != null)
						throw new GraphicsException("MorphWeights must be null because the mesh does not include morph targets.");
				}

				_morphWeights = value;

				// Assign MorphWeights to EffectBindings. (Required for rendering.)
/*				int numberOfSubmeshes = RenderMesh.Submeshes.Count;
				for (int i = 0; i < numberOfSubmeshes; i++)
				{
					var submesh = RenderMesh.Submeshes[i];
					if (submesh.HasMorphTargets)
						foreach (var materialInstanceBinding in MaterialInstances[submesh.MaterialIndex].EffectBindings)
							materialInstanceBinding.MorphWeights = value;
				}*/
			}
		}
		private MorphWeightCollection _morphWeights;

		public override bool IsRenderable => true;

		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		protected MeshNodeBase()
		{
			// This internal constructor is called when loaded from an asset.
			// The mesh (shared resource) will be set later by using a fix-up code 
			// defined in MeshNodeReader.
			// When all fix-ups are executed, OnAssetLoaded (see below) is called.

			CastsShadows = true;
		}

		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		#region ----- Cloning -----

		/// <inheritdoc/>
		protected override void CloneCore(SceneNode source)
		{
			// Clone SceneNode properties.
			base.CloneCore(source);

			// Clone MeshNode properties.
			var sourceTyped = (MeshNodeBase)source;

			if (sourceTyped.MorphWeights != null)
				MorphWeights = sourceTyped.MorphWeights.Clone();
		}
		#endregion

		#region ----- IOcclusionProxy -----

		/// <inheritdoc/>
		bool IOcclusionProxy.HasOccluder
		{
			get { return RenderMesh.Occluder != null; }
		}


		/// <inheritdoc/>
		void IOcclusionProxy.UpdateOccluder()
		{
			Debug.Assert(((IOcclusionProxy)this).HasOccluder, "Check IOcclusionProxy.HasOccluder before calling UpdateOccluder().");

			// The occluder data is created when needed and cached in RenderData.
			var data = RenderData as OccluderData;
			if (data == null)
			{
				data = new OccluderData(RenderMesh.Occluder);
				RenderData = data;
				IsDirty = true;
			}

			if (IsDirty)
			{
				data.Update(RenderMesh.Occluder, PoseWorld, ScaleWorld);
				IsDirty = false;
			}
		}


		/// <inheritdoc/>
		OccluderData IOcclusionProxy.GetOccluder()
		{
			Debug.Assert(((IOcclusionProxy)this).HasOccluder, "Call IOcclusionProxy.HasOccluder before calling GetOccluder().");
			Debug.Assert(!IsDirty, "Call IOcclusionProxy.HasOccluder and UpdateOccluder() before calling GetOccluder().");

			return (OccluderData)RenderData;
		}

		#endregion

		public override void BatchJobs(IRenderList list)
		{
			base.BatchJobs(list);

			var mesh = RenderMesh;
			if (mesh == null)
			{
				return;
			}

			foreach(var submesh in mesh.Submeshes)
			{
				if (submesh.Material == null)
				{
					continue;
				}

				list.AddJob(submesh, submesh.Material, CalculateGlobalTransform());
			}
		}

		#endregion
	}
}
