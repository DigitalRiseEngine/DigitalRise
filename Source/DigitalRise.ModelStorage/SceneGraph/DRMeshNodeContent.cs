// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.ModelStorage.Meshes;


namespace DigitalRise.ModelStorage.SceneGraph
{
	/// <summary>
	/// Stores processing data for a <strong>MeshNode</strong>.
	/// </summary>
	public class DRMeshNodeContent : DRSceneNodeContent
	{
		/// <summary>
		/// Gets or sets the mesh.
		/// </summary>
		/// <value>The mesh.</value>
		public DRMeshContent Mesh { get; set; }

		public DRIndexBufferContent IndexBuffer { get; set; }
	}
}
