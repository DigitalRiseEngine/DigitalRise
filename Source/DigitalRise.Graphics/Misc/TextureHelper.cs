// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Misc
{
	/// <summary>
	/// Provides helper methods for textures.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class provides several default textures (e.g. <see cref="GetDefaultTexture2DWhite"/>).
	/// These default textures are only created once per graphics device and are reused. These
	/// textures must not be modified.
	/// </para>
	/// </remarks>
	internal static class TextureHelper
	{
		/// <summary>
		/// Determines whether the specified surface format is a floating-point format.
		/// </summary>
		/// <param name="format">The surface format.</param>
		/// <returns>
		/// <see langword="true"/> if the specified format is a floating-point format; otherwise, 
		/// <see langword="false"/>.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Invalid format specified.
		/// </exception>
		public static bool IsFloatingPointFormat(SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Color:
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Dxt1:
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
				case SurfaceFormat.NormalizedByte2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Alpha8:
					return false;

				case SurfaceFormat.Single:
				case SurfaceFormat.Vector2:
				case SurfaceFormat.Vector4:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.HdrBlendable:
					return true;

				default:
					throw new ArgumentOutOfRangeException("format");
			}
		}
	}
}
