// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace DigitalRise.ModelStorage.Occluder
{
	/// <summary>
	/// Stores the processed data for an <strong>Occluder</strong> asset.
	/// </summary>
	public class DROccluderContent
	{
		/// <summary>
		/// Gets or sets the triangle vertices.
		/// </summary>
		/// <value>The triangle vertices.</value>
		public List<Vector3> Vertices { get; set; }


		/// <summary>
		/// Gets or sets the triangle indices.
		/// </summary>
		/// <value>The triangle indices.</value>
		public List<int> Indices { get; set; }
	}
}
