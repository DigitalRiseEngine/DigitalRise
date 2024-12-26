using AssetManagementBase;
using DigitalRise.Attributes;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Primitives.Objects;
using DigitalRise.SceneGraph;
using DigitalRise.Geometry.Shapes;
using Plane = DigitalRise.Data.Meshes.Primitives.Objects.Plane;
using DigitalRise.SceneGraph.Occlusion;
using System.Diagnostics;
using DigitalRise.Rendering.Deferred;

namespace DigitalRise.Graphics.SceneGraph
{
	[EditorInfo("Primitive")]
	public class PrimitiveMeshNode : SceneNode, IOcclusionProxy
	{
		[EditorOption(typeof(Box))]
		[EditorOption(typeof(Capsule))]
		[EditorOption(typeof(Cone))]
		[EditorOption(typeof(Cylinder))]
		[EditorOption(typeof(Disc))]
		[EditorOption(typeof(GeoSphere))]
		[EditorOption(typeof(Plane))]
		[EditorOption(typeof(Sphere))]
		[EditorOption(typeof(Teapot))]
		[EditorOption(typeof(Torus))]
		public BasePrimitive Primitive { get; set; }

		public IMaterial Material { get; set; } = new DefaultMaterial();

		public override bool IsRenderable => true;

		private Mesh RenderMesh => Primitive?.Mesh;

		public PrimitiveMeshNode()
		{
			Shape = Shape.Infinite;
			CastsShadows = true;
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			var hasExternalAssets = Material as IHasExternalAssets;
			if (hasExternalAssets != null)
			{
				hasExternalAssets.Load(assetManager);
			}
		}

		protected override void CloneCore(SceneNode source)
		{
			base.CloneCore(source);

			var src = (PrimitiveMeshNode)source;

			Material = src.Material.Clone();
			if (src.Primitive != null)
			{
				Primitive = src.Primitive.Clone();
			}
			else
			{
				Primitive = null;
			}
		}

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

		public override void BatchJobs(IRenderList list)
		{
			base.BatchJobs(list);

			if (Material == null)
			{
				return;
			}

			var mesh = RenderMesh;
			if (mesh == null)
			{
				return;
			}

			foreach (var submesh in mesh.Submeshes)
			{
				list.AddJob(submesh, Material, CalculateGlobalTransform());
			}
		}

		#endregion
	}
}