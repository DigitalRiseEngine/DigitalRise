using AssetManagementBase;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Meshes;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;

namespace DigitalRise.SceneGraph.Primitives
{
	public abstract class PrimitiveMeshNode : MeshNodeBase, IUpdateableNode
	{
		private bool _isLeftHanded;
		private Mesh _mesh;
		private float _uScale = 1.0f;
		private float _vScale = 1.0f;

		protected override Mesh RenderMesh
		{
			get
			{
				if (_mesh == null)
				{
					_mesh = CreateMesh();

					foreach(var submesh in _mesh.Submeshes)
					{
						submesh.Material = Material;
					}
				}

				return _mesh;
			}
		}

		private bool IsMeshDirty => _mesh == null;

		public IMaterial Material { get; set; } = new DefaultMaterial();

		public bool IsLeftHanded
		{
			get => _isLeftHanded;

			set
			{
				if (value == _isLeftHanded)
				{
					return;
				}

				_isLeftHanded = value;
				InvalidateMesh();
			}
		}

		public float UScale
		{
			get => _uScale;

			set
			{
				if (Numeric.AreEqual(value, _uScale))
				{
					return;
				}

				_uScale = value;
				InvalidateMesh();
			}
		}

		public float VScale
		{
			get => _vScale;

			set
			{
				if (Numeric.AreEqual(value, _vScale))
				{
					return;
				}

				_vScale = value;
				InvalidateMesh();
			}
		}

		public PrimitiveMeshNode()
		{
		}

		protected abstract Mesh CreateMesh();

		public void InvalidateMesh()
		{
			_mesh = null;
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

		public void Update(GameTime gameTime)
		{
			if (!IsMeshDirty)
			{
				return;
			}

			Shape = new BoxShape(RenderMesh.BoundingBox.Volume());
		}
	}
}