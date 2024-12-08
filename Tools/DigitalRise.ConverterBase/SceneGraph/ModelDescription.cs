// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DigitalRise.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRise.ConverterBase.SceneGraph
{
	// The model description is stored in an XML file. The XML file has the same
	// name as the model asset. Example: "Dude.fbx" --> "Dude.xml" or "Dude.drmdl"
	// If the XML file is missing, the model is built using the materials included
	// in the model file ("local materials").

	internal class ModelDescription : ContentItem
	{
		public string FileName { get; set; }
		public string Importer { get; set; }
		public float RotationX { get; set; }
		public float RotationY { get; set; }
		public float RotationZ { get; set; }
		public float Scale { get; set; }
		public bool GenerateTangentFrames { get; set; }
		public bool SwapWindingOrder { get; set; }
		public bool BoundingBoxEnabled { get; set; }
		public Vector3 BoundingBoxMinimum { get; set; }
		public Vector3 BoundingBoxMaximum { get; set; }
		public bool PremultiplyVertexColors { get; set; }
		public List<MeshDescription> Meshes { get; set; }
		public AnimationDescription Animation { get; set; }
		public float MaxDistance { get; set; }


		/// <summary>
		/// Prevents a default instance of the <see cref="ModelDescription"/> class from being created.
		/// </summary>
		private ModelDescription()
		{
		}


		/// <summary>
		/// Checks whether the imported model matches the model description.
		/// </summary>
		/// <param name="input">The root node content.</param>
		/// <param name="logger"></param>
		public void Validate(NodeContent input, Action<string> logger)
		{
			foreach (var meshDescription in Meshes)
			{
				// Check if there is a mesh for this mesh name.
				if (!string.IsNullOrEmpty(meshDescription.Name)
					&& TreeHelper.GetSubtree(input, n => n.Children)
								 .OfType<MeshContent>()
								 .All(mc => mc.Name != meshDescription.Name))
				{
					logger?.Invoke(string.Format(
					  "Model description (.drmdl file) contains description for mesh '{0}' which was not found in the asset.",
					  meshDescription.Name));
				}
			}
		}


		public MeshDescription GetMeshDescription(string name)
		{
			if (name == null)
				name = string.Empty;

			// Search for mesh description by name.
			var meshDescription = Meshes.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			if (meshDescription != null)
				return meshDescription;

			// Search for mesh description without a name. Use as fallback.
			meshDescription = Meshes.FirstOrDefault(m => string.IsNullOrEmpty(m.Name));

			return meshDescription;
		}
	}
}
