// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRise.ConverterBase.SceneGraph
{
	/// <summary>
	/// Stores processing data for a <strong>LodGroupNode</strong>.
	/// </summary>
	public class DRLodGroupNodeContent : DRSceneNodeContent
	{
		/// <summary>
		/// Gets or sets the levels of detail (LODs).
		/// </summary>
		/// <value>The levels of detail (LODs).</value>
		public List<DRSceneNodeContent> Levels { get; private set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="DRLodGroupNodeContent"/> class.
		/// </summary>
		public DRLodGroupNodeContent()
		{
			Levels = new List<DRSceneNodeContent>();
		}
	}
}
