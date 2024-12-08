// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.ConverterBase.Occluder;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRise.ConverterBase.SceneGraph
{
	/// <summary>
	/// Stores processing data for an <strong>OccluderNode</strong>.
	/// </summary>
	public class DROccluderNodeContent : DRSceneNodeContent
	{
		/// <summary>
		/// Gets or sets the imported <see cref="MeshContent"/>.
		/// </summary>
		/// <value>The imported <see cref="MeshContent"/>.</value>
		[ContentSerializerIgnore]
		public MeshContent InputMesh { get; set; }  // Only relevant for processing.


		/// <summary>
		/// Gets or sets the occluder.
		/// </summary>
		/// <value>The occluder.</value>
		[ContentSerializer(ElementName = "Occluder", SharedResource = true)]
		public DROccluderContent Occluder { get; set; }
	}
}
