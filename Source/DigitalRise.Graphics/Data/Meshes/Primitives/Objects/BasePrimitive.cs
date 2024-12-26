using DigitalRise.Mathematics;
using Newtonsoft.Json;
using System.ComponentModel;

namespace DigitalRise.Data.Meshes.Primitives.Objects
{
	public abstract class BasePrimitive
	{
		private Mesh _mesh;
		private bool _isLeftHanded;
		private float _uScale = 1.0f;
		private float _vScale = 1.0f;

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

		[Browsable(false)]
		[JsonIgnore]
		public bool IsDirty => Mesh == null;


		internal Mesh Mesh
		{
			get
			{
				Update();

				return _mesh;
			}
		}


		protected void InvalidateMesh()
		{
			_mesh = null;
		}

		private void Update()
		{
			if (_mesh != null)
			{
				return;
			}

			_mesh = CreateMesh();
		}


		public BasePrimitive Clone()
		{
			var result = CreateInstanceCore();

			result.CloneCore(this);

			return result;
		}

		protected virtual void CloneCore(BasePrimitive source)
		{
			IsLeftHanded = source.IsLeftHanded;
			UScale = source.UScale;
			VScale = source.VScale;
		}

		protected abstract Mesh CreateMesh();
		protected abstract BasePrimitive CreateInstanceCore();

		public override string ToString() => GetType().Name;
	}
}
