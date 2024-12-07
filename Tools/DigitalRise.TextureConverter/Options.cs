using DigitalRise.ConverterBase.Textures;
using System.ComponentModel;

namespace DigitalRise.TextureConverter
{
	public class Options
	{
		public string InputFile { get; set; }

		public string OutputFile { get; set; }

		public bool GenerateMipmaps { get; set; } = true;

		/// <summary>
		/// Gets or sets the gamma of the input texture.
		/// </summary>
		/// <value>The gamma of the input texture. The default value is 2.2.</value>
		[DefaultValue(2.2f)]
		public float InputGamma { get; set; } = 2.2f;


		/// <summary>
		/// Gets or sets the gamma of the output texture.
		/// </summary>
		/// <value>The gamma of the output texture. The default value is 2.2.</value>
		[DefaultValue(2.2f)]
		public float OutputGamma { get; set; } = 2.2f;

		public bool PremultiplyAlpha { get; set; } = true;

		/// <summary>
		/// Gets or sets a value indicating whether the texture is resized to the next largest power of 
		/// two.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if resizing is enabled; <see langword="false"/> otherwise.
		/// </value>
		/// <remarks>
		/// Typically used to maximize compatibility with a graphics card because many graphics cards 
		/// do not support a material size that is not a power of two. If 
		/// <see cref="ResizeToPowerOfTwo"/> is enabled, textures are resized to the next largest power 
		/// of two.
		/// </remarks>
		public bool ResizeToPowerOfTwo { get; set; }

		/// <summary>
		/// Gets or sets the texture format of output.
		/// </summary>
		/// <value>The texture format of the output.</value>
		/// <remarks>
		/// The input format can either be left unchanged from the source asset, converted to a 
		/// corresponding <see cref="Color"/>, or compressed using the appropriate 
		/// <see cref="DRTextureFormat.Dxt"/> format.
		/// </remarks>
		[DefaultValue(DRTextureFormat.Color)]
		public DRTextureFormat Format { get; set; } = DRTextureFormat.Color;

		/// <summary>
		/// Gets or sets the reference alpha value, which is used in the alpha test.
		/// </summary>
		/// <value>The reference alpha value, which is used in the alpha test.</value>
		[DefaultValue(0.9f)]
		public float ReferenceAlpha { get; set; } = 0.9f;

		/// <summary>
		/// Gets or sets a value indicating whether the alpha of the lower mipmap levels should be 
		/// scaled to achieve the same alpha test coverage as in the source image.
		/// </summary>
		/// <value>
		/// <see langword="true"/> to scale the alpha values of the lower mipmap levels; otherwise, 
		/// <see langword="false"/>.
		/// </value>
		public bool ScaleAlphaToCoverage { get; set; }

		public override string ToString() =>
			$"{InputFile}, Format={Format}, GenerateMipMaps={GenerateMipmaps}, InputGamma={InputGamma}, OutputGamma={OutputGamma}, PremultiplyAlpha={PremultiplyAlpha}, " +
			$"ResizeToPowerOfTwo={ResizeToPowerOfTwo}, ReferenceAlpha={ReferenceAlpha}";
	}
}
