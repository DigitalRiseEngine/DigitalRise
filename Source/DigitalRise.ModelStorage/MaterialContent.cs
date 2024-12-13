using Microsoft.Xna.Framework;

namespace DigitalRise.ModelStorage
{
	public class MaterialContent
	{
		public string Name { get; set; }
		public Color AmbientColor { get; set; }
		public Color DiffuseColor { get; set; }
		public Color EmissiveColor { get; set; }
		public Color ReflectiveColor { get; set; }
		public Color SpecularColor { get; set; }
		public Color TransparentColor { get; set; }
		public TextureSlotContent AmbientTexture { get; set; }
		public TextureSlotContent AmbientOcclusionTexture { get; set; }
		public TextureSlotContent DiffuseTexture { get; set; }
		public TextureSlotContent EmissiveTexture { get; set; }
		public TextureSlotContent HeightTexture { get; set; }
		public TextureSlotContent LightMapTexture { get; set; }
		public TextureSlotContent NormalTexture { get; set; }
		public TextureSlotContent OpacityTexture { get; set; }
		public TextureSlotContent ReflectionTexture { get; set; }
		public TextureSlotContent SpecularTexture { get; set; }
		public float BumpScaling { get; set; }
		public float Shininess { get; set; }
		public float ShininessStrength { get; set; }
		public float Opacity { get; set; }
		public float Reflectivity { get; set; }
		public float TransparencyFactor { get; set; }
		public bool IsTwoSided { get; set; }
		public bool IsWireFrame { get; set; }
	}
}
