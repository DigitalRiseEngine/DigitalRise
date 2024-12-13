// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Data.Modelling;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;


namespace DigitalRise.Data.Meshes
{
	/// <summary>
	/// Represents a mesh of a 3D model.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A mesh represents the geometry and materials of a 3D object that can be rendered. A mesh 
	/// is divided into <see cref="Submesh"/>es. Each 
	/// <see cref="Submesh"/> describes a batch of primitives (usually triangles) that use one 
	/// material and can be rendered with a single draw call.
	/// </para>
	/// <para>
	/// The mesh can be rendered by creating a <see cref="MeshNode"/> and adding it to a 3D scene.
	/// </para>
	/// <para>
	/// <strong>Morph Target Animation:</strong> Submeshes may include morph targets (see
	/// <see cref="Submesh.MorphTargets"/>). The extension method
	/// <see cref="MeshHelper.GetMorphTargetNames"/> can be used to get a list of all morph targets
	/// included in a mesh. The current <see cref="MeshNode.MorphWeights"/> are stored in the
	/// <see cref="MeshNode"/>.
	/// </para>
	/// <para>
	/// <strong>Skeletal Animation:</strong> The mesh may contain a <see cref="Skeleton"/>, which can
	/// be used to animate (deform) the mesh. The current <see cref="MeshNode.SkeletonPose"/> is
	/// stored in the <see cref="MeshNode"/>. The property <see cref="MeshNode.SkeletonPose"/> can be
	/// animated. A set of key frame animations can be stored in <see cref="Animations"/>.
	/// </para>
	/// <para>
	/// <strong>Bounding shape:</strong> The bounding shape of the mesh is usually created by the 
	/// content pipeline and stored in the <see cref="BoundingShape"/> property. It is not updated
	/// automatically when the vertex buffer changes. The user who changes the vertex buffer is 
	/// responsible for updating or replacing the shape stored in <see cref="BoundingShape"/>.
	/// If the mesh can be deformed on the GPU (e.g. using mesh skinning), then the bounding shape
	/// must be large enough to contain all possible deformations.
	/// </para>
	/// <para>
	/// The properties of the bounding shape can be changed at any time. But it is not allowed to 
	/// replace the bounding shape while the <see cref="Mesh"/> is in use, i.e. referenced by a 
	/// scene node.
	/// </para>
	/// <para>
	/// For example, if the bounding shape is a <see cref="SphereShape"/>, the radius of the sphere 
	/// can be changed at any time. But it is not allowed to replace the <see cref="SphereShape"/> 
	/// with a <see cref="BoxShape"/> as long as the mesh is used in a scene. Replacing the 
	/// bounding shape will not raise any exceptions, but the mesh may no longer be rendered 
	/// correctly.
	/// </para>
	/// <para>
	/// <strong>Cloning:</strong> Meshes are currently <strong>not</strong> cloneable.
	/// </para>
	/// </remarks>
	/// <seealso cref="MeshNode"/>
	/// <seealso cref="Submesh"/>
	public class Mesh : IDisposable
	{
		// Note: Meshes are not cloneable because meshes/submeshes from one model usually
		// share vertex and index buffers. Therefore, it makes little sense to duplicate 
		// the shared buffers for clones. 

		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private string[] _cachedMorphTargetNames;
		private BoundingBox? _boundingBox;
		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		public BoundingBox BoundingBox
		{
			get
			{
				if (_boundingBox == null)
				{
					var bb = new BoundingBox();
					foreach(var submesh in Submeshes)
					{
						bb = BoundingBox.CreateMerged(bb, submesh.BoundingBox);
					}

					_boundingBox = bb;
				}

				return _boundingBox.Value;
			}
		}


		/// <summary>
		/// Gets the collection of <see cref="Submesh"/>es that make up this mesh. Each submesh is 
		/// composed of a set of primitives that share the same material. 
		/// </summary>
		/// <value>The <see cref="Submesh"/>es that make up this mesh.</value>
		[Category("Common")]
		public ObservableCollection<Submesh> Submeshes { get; } = new ObservableCollection<Submesh>();


		/// <summary>
		/// Gets or sets the occluder that is rendered into the occlusion buffer.
		/// </summary>
		/// <value>The occluder. The default value is <see langword="null"/>.</value>
		[Category("Misc")]
		public Occluder Occluder { get; set; }


		/// <summary>
		/// Gets or sets a user-defined object.
		/// </summary>
		/// <value>A user-defined object.</value>
		/// <remarks>
		/// This property is intended for application-specific data and is not used by the mesh itself. 
		/// </remarks>
		[Category("Misc")]
		public object UserData { get; set; }
		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="Mesh"/> class.
		/// </summary>
		public Mesh()
		{
			Submeshes.CollectionChanged += Submeshes_CollectionChanged;
		}

		public Mesh(Submesh submesh) : this()
		{
			Submeshes.Add(submesh);
		}

		private void Submeshes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (Submesh n in args.NewItems)
				{
					n.Mesh = this;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (Submesh n in args.OldItems)
				{
					n.Mesh = null;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var w in Submeshes)
				{
					w.Mesh = null;
				}
			}

			_boundingBox = null;
		}

		/// <summary>
		/// Releases all resources used by an instance of the <see cref="Mesh"/> class.
		/// </summary>
		/// <remarks>
		/// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
		/// <see langword="true"/>, and then suppresses finalization of the instance.
		/// </remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Releases the unmanaged resources used by an instance of the <see cref="Mesh"/> class and
		/// optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">
		/// <see langword="true"/> to release both managed and unmanaged resources; 
		/// <see langword="false"/> to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Dispose managed resources.
				foreach (var submesh in Submeshes)
					submesh.Dispose();

				UserData.SafeDispose();
			}
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		#region ----- Morph Targets -----

		internal void InvalidateMorphTargetNames()
		{
			_cachedMorphTargetNames = null;
		}


		internal string[] GetMorphTargetNames()
		{
			if (_cachedMorphTargetNames != null)
				return _cachedMorphTargetNames;

			// Get the names of all morph targets.
			var names = new List<string>();
			foreach (var submesh in Submeshes)
				if (submesh.MorphTargets != null)
					foreach (var morphTarget in submesh.MorphTargets)
						names.Add(morphTarget.Name);

			// Sort names in ascending order.
			names.Sort(String.CompareOrdinal);

			// Remove duplicates.
			for (int i = names.Count - 1; i > 0; i--)
				if (names[i] == names[i - 1])
					names.RemoveAt(i);

			_cachedMorphTargetNames = names.ToArray();
			return _cachedMorphTargetNames;
		}


		internal bool HasMorphTargets()
		{
			if (_cachedMorphTargetNames != null && _cachedMorphTargetNames.Length > 0)
				return true;

			foreach (var submesh in Submeshes)
				if (submesh.HasMorphTargets)
					return true;

			return false;
		}
		#endregion

		public Mesh Clone()
		{
			Mesh mesh = new Mesh
			{
				Occluder = Occluder,
				UserData = UserData
			};

			foreach (var submesh in Submeshes)
			{
				mesh.Submeshes.Add(submesh.Clone());
			}

			return mesh;
		}

		#endregion
	}
}
