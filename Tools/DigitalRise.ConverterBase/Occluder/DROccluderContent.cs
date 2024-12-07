﻿// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.ConverterBase.SceneGraph;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace DigitalRise.ConverterBase.Occluder
{
	/// <summary>
	/// Stores the processed data for an <strong>Occluder</strong> asset.
	/// </summary>
	public class DROccluderContent : DRSceneNodeContent
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