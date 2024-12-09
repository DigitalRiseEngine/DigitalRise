// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.ModelStorage.Occluder;


namespace DigitalRise.ModelStorage.SceneGraph
{
	/// <summary>
	/// Stores processing data for an <strong>OccluderNode</strong>.
	/// </summary>
	public class DROccluderNodeContent : DRSceneNodeContent
	{
		/// <summary>
		/// Gets or sets the occluder.
		/// </summary>
		/// <value>The occluder.</value>
		public DROccluderContent Occluder { get; set; }
	}
}
