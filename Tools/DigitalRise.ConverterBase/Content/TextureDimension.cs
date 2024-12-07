// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRise.ConverterBase.Content
{
	/// <summary>
	/// Defines the dimension of a texture.
	/// </summary>
	public enum TextureDimension
	{
		/// <summary>A 1-dimensional texture.</summary>
		Texture1D,

		/// <summary>A 2-dimensional texture.</summary>
		Texture2D,

		/// <summary>A 3-dimensional (volume) texture.</summary>
		Texture3D,

		/// <summary>A cube map texture.</summary>
		TextureCube
	}
}
