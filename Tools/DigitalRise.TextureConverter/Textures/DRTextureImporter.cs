// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
using DigitalRise.TextureConverter.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Path = System.IO.Path;


namespace DigitalRise.TextureConverter.Textures
{
	/// <summary>
	/// Provides methods for reading texture files for use in the Content Pipeline. 
	/// </summary>
	[ContentImporter(".image_file_extension",  // Do not set file extension, otherwise it conflicts with XNA TextureImporter.
					 DisplayName = "Texture - DigitalRise Graphics",
					 DefaultProcessor = "DRTextureProcessor")]
	public class DRTextureImporter : TextureImporter
	{
		/// <summary>
		/// Called by the XNA Framework when importing an texture file to be used as a game asset. This
		/// is the method called by the XNA Framework when an asset is to be imported into an object
		/// that can be recognized by the Content Pipeline.
		/// </summary>
		/// <param name="filename">Name of a game asset file.</param>
		/// <param name="context">
		/// Contains information for importing a game asset, such as a logger interface.
		/// </param>
		/// <returns>Resulting game asset.</returns>
		public override TextureContent Import(string filename, ContentImporterContext context)
		{
			string extension = Path.GetExtension(filename);
			if (extension != null)
			{
				Texture texture = null;
				if (extension.Equals(".DDS", StringComparison.OrdinalIgnoreCase))
				{
					using (var stream = File.OpenRead(filename))
						texture = DdsHelper.Load(stream, DdsFlags.ForceRgb | DdsFlags.ExpandLuminance);
				}
				else if (extension.Equals(".TGA", StringComparison.OrdinalIgnoreCase))
				{
					using (var stream = File.OpenRead(filename))
						texture = TgaHelper.Load(stream);
				}

				if (texture != null)
				{
					// Convert DigitalRise Texture to XNA TextureContent.
					var identity = new ContentIdentity(filename, "DigitalRise");
					return TextureHelper.ToContent(texture, identity);
				}
			}

			return base.Import(filename, context);
		}
	}
}
