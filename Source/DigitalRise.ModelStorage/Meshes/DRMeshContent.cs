
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Animation.Character;
using DigitalRise.Geometry.Shapes;
using DigitalRise.ModelStorage.Occluder;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace DigitalRise.ModelStorage.Meshes
{
	/// <summary>
	/// Stores the processed data for a <strong>Mesh</strong> asset.
	/// </summary>
	public class DRMeshContent : INamedObject
	{
		public List<DRVertexBufferContent> VertexBuffers { get; set; }

		/// <summary>
		/// Gets or sets the bounding shape for this mesh.
		/// </summary>
		/// <value>The bounding shape for this mesh.</value>
		public Shape BoundingShape { get; set; }


		/// <summary>
		/// Gets or sets the submeshes associated with this mesh.
		/// </summary>
		/// <value>The submeshes associated with this mesh.</value>
		public List<DRSubmeshContent> Submeshes { get; set; }


		/// <summary>
		/// Gets or sets the mesh name.
		/// </summary>
		/// <value>The mesh name.</value>
		public string Name { get; set; }


		/// <summary>
		/// Gets or sets the occluder.
		/// </summary>
		/// <value>The occluder.</value>
		public DROccluderContent Occluder { get; set; }


		/// <summary>
		/// Gets or sets the skeleton.
		/// </summary>
		/// <value>The skeleton.</value>
		public Skeleton Skeleton { get; set; }


		/// <summary>
		/// Gets or sets the animations.
		/// </summary>
		/// <value>The animations. Can be <see langword="null"/> if there are no animations.</value>
		public Dictionary<string, SkeletonKeyFrameAnimation> Animations { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public List<Vector3> Positions { get; } = new List<Vector3>();

		/// <summary>
		/// Gets or sets a user-defined object.
		/// </summary>
		/// <value>User-defined object.</value>
		[Browsable(false)]
		[JsonIgnore]
		public object UserData { get; set; }
	}
}
