using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.ComponentModel;

namespace DigitalRise.Data.Materials
{
	internal class DefaultGBufferBinding : BatchEffectBinding
	{
		private static DefaultGBufferBinding _gBuffer, _gBufferSkinning, _gBufferNormal;
		public EffectParameter SpecularPower { get; private set; }
		public EffectParameter NormalTexture { get; private set; }

		public static DefaultGBufferBinding GBuffer
		{
			get
			{
				if (_gBuffer == null)
				{
					_gBuffer = new DefaultGBufferBinding("Materials/GBuffer");
				}

				return _gBuffer;
			}
		}

		public static DefaultGBufferBinding GBufferSkinning
		{
			get
			{
				if (_gBufferSkinning == null)
				{
					_gBufferSkinning = new DefaultGBufferBinding("Materials/GBufferSkinned");
				}

				return _gBufferSkinning;
			}
		}

		public static DefaultGBufferBinding GBufferNormal
		{
			get
			{
				if (_gBufferNormal == null)
				{
					_gBufferNormal = new DefaultGBufferBinding("Materials/GBufferNormal");
				}

				return _gBufferNormal;
			}
		}

		private DefaultGBufferBinding(string path) : base(path)
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);
			SpecularPower = effect.Parameters["SpecularPower"];
			NormalTexture = effect.Parameters["NormalTexture"];
		}
	}

	internal class DefaultShadowMapBinding : BatchEffectBinding
	{
		private static DefaultShadowMapBinding _shadowMap, _shadowMapSkinning;
		public EffectParameter SpecularPower { get; private set; }

		public static DefaultShadowMapBinding ShadowMap
		{
			get
			{
				if (_shadowMap == null)
				{
					_shadowMap = new DefaultShadowMapBinding("Materials/ShadowMap");
				}

				return _shadowMap;
			}
		}

		public static DefaultShadowMapBinding ShadowMapSkinning
		{
			get
			{
				if (_shadowMapSkinning == null)
				{
					_shadowMapSkinning = new DefaultShadowMapBinding("Materials/ShadowMapSkinned");
				}

				return _shadowMapSkinning;
			}
		}

		private DefaultShadowMapBinding(string path) : base(path)
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);
			SpecularPower = effect.Parameters["SpecularPower"];
		}
	}

	internal class DefaultMaterialBinding : BatchEffectBinding
	{
		private static DefaultMaterialBinding _material, _materialSkinning;

		public EffectParameter DiffuseColor { get; private set; }
		public EffectParameter SpecularColor { get; private set; }
		public EffectParameter DiffuseTexture { get; private set; }
		public EffectParameter SpecularTexture { get; private set; }

		public static DefaultMaterialBinding Material
		{
			get
			{
				if (_material == null)
				{
					_material = new DefaultMaterialBinding("Materials/Material");
				}

				return _material;
			}
		}

		public static DefaultMaterialBinding MaterialSkinning
		{
			get
			{
				if (_materialSkinning == null)
				{
					_materialSkinning = new DefaultMaterialBinding("Materials/MaterialSkinned");
				}

				return _materialSkinning;
			}
		}

		private DefaultMaterialBinding(string path) : base(path)
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			DiffuseColor = effect.Parameters["DiffuseColor"];
			SpecularColor = effect.Parameters["SpecularColor"];
			DiffuseTexture = effect.Parameters["DiffuseTexture"];
			SpecularTexture = effect.Parameters["SpecularTexture"];
		}
	}

	public partial class DefaultMaterial : INamedObject, IMaterial, IHasExternalAssets
	{
		private bool _skinning;
		private Texture2D _diffuseTexture, _specularTexture, _normalTexture;

		private DefaultGBufferBinding _gBufferBinding;
		private DefaultShadowMapBinding _shadowMapBinding;
		private DefaultMaterialBinding _materialBinding;

		public string Name { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public BatchEffectBinding EffectGBuffer
		{
			get
			{
				if (_gBufferBinding == null)
				{
					if (Skinning)
					{
						_gBufferBinding = DefaultGBufferBinding.GBufferSkinning;
					} else if (NormalTexture != null)
					{
						_gBufferBinding = DefaultGBufferBinding.GBufferNormal;
					} else
					{
						_gBufferBinding = DefaultGBufferBinding.GBuffer;
					}
				}

				return _gBufferBinding;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public BatchEffectBinding EffectShadowMap
		{
			get
			{
				if (_shadowMapBinding == null)
				{
					_shadowMapBinding = Skinning ? DefaultShadowMapBinding.ShadowMapSkinning : DefaultShadowMapBinding.ShadowMap;
				}

				return _shadowMapBinding;
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public BatchEffectBinding EffectMaterial
		{
			get
			{
				if (_materialBinding == null)
				{
					_materialBinding = Skinning ? DefaultMaterialBinding.MaterialSkinning : DefaultMaterialBinding.Material;
				}

				return _materialBinding;
			}
		}

		public Color DiffuseColor { get; set; } = Color.White;
		public Color SpecularColor { get; set; }

		[JsonIgnore]
		public Texture2D DiffuseTexture
		{
			get => _diffuseTexture;

			set
			{
				if (value == _diffuseTexture)
				{
					return;
				}

				_diffuseTexture = value;
				Invalidate();
			}
		}

		[Browsable(false)]
		public string DiffuseTexturePath { get; set; }

		[JsonIgnore]
		public Texture2D SpecularTexture
		{
			get => _specularTexture;

			set
			{
				if (value == _specularTexture)
				{
					return;
				}

				_specularTexture = value;
				Invalidate();
			}
		}

		[Browsable(false)]
		public string SpecularTexturePath { get; set; }

		[DefaultValue(250.0f)]
		public float SpecularPower { get; set; } = 250.0f;

		[JsonIgnore]
		public Texture2D NormalTexture
		{
			get => _normalTexture;

			set
			{
				if (value == _normalTexture)
				{
					return;
				}

				_normalTexture = value;
				Invalidate();
			}
		}

		[Browsable(false)]
		public string NormalTexturePath { get; set; }


		[Browsable(false)]
		[JsonIgnore]
		public bool Skinning
		{
			get => _skinning;

			set
			{
				if (value == _skinning)
				{
					return;
				}

				_skinning = value;
				Invalidate();
			}
		}

		public void Load(AssetManager assetManager)
		{
			if (!string.IsNullOrEmpty(DiffuseTexturePath))
			{
				DiffuseTexture = assetManager.LoadTexture2D(DR.GraphicsDevice, DiffuseTexturePath);
			}

			if (!string.IsNullOrEmpty(SpecularTexturePath))
			{
				SpecularTexture = assetManager.LoadTexture2D(DR.GraphicsDevice, SpecularTexturePath);
			}

			if (!string.IsNullOrEmpty(NormalTexturePath))
			{
				NormalTexture = assetManager.LoadTexture2D(DR.GraphicsDevice, NormalTexturePath);
			}
		}

		public void SetGBufferParameters()
		{
			_gBufferBinding.SpecularPower.SetValue(SpecularPower);

			if (NormalTexture != null)
			{
				_gBufferBinding.NormalTexture?.SetValue(NormalTexture);
			}
			else
			{
				_gBufferBinding.NormalTexture?.SetValue((Texture2D)null);
			}
		}

		public void SetShadowMapParameters()
		{
		}

		public void SetMaterialParameters()
		{
			_materialBinding.DiffuseColor.SetValue(DiffuseColor.ToVector3());
			_materialBinding.SpecularColor.SetValue(SpecularColor.ToVector3());

			if (DiffuseTexture != null)
			{
				_materialBinding.DiffuseTexture.SetValue(DiffuseTexture);
			}
			else
			{
				_materialBinding.DiffuseTexture.SetValue(Resources.DefaultTexture2DWhite);
			}

			if (SpecularTexture != null)
			{
				_materialBinding.SpecularTexture.SetValue(SpecularTexture);
			}
			else
			{
				_materialBinding.SpecularTexture.SetValue(Resources.DefaultTexture2DWhite);
			}
		}

		private void Invalidate()
		{
			_gBufferBinding = null;
			_shadowMapBinding = null;
			_materialBinding = null;
		}

		public IMaterial Clone()
		{
			return new DefaultMaterial
			{
				Name = Name,
				SpecularPower = SpecularPower,
				DiffuseColor = DiffuseColor,
				SpecularColor = SpecularColor,
				DiffuseTexture = DiffuseTexture,
				DiffuseTexturePath = DiffuseTexturePath,
				SpecularTexture = SpecularTexture,
				SpecularTexturePath = SpecularTexturePath,
				NormalTexture = NormalTexture,
				NormalTexturePath = NormalTexturePath,
				Skinning = Skinning
			};
		}
	}
}
