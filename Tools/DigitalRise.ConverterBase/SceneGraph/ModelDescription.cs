// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DigitalRise.ConverterBase.Animations;
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

	public class ModelDescription
	{
		public string FileName { get; set; }
		public bool GenerateTangentFrames { get; set; }
		public bool SwapWindingOrder { get; set; }
		public bool BoundingBoxEnabled { get; set; }
		public Vector3 BoundingBoxMinimum { get; set; }
		public Vector3 BoundingBoxMaximum { get; set; }
		public bool PremultiplyVertexColors { get; set; }
		public List<MeshDescription> Meshes { get; set; }
		public AnimationDescription Animation { get; set; }
		public float MaxDistance { get; set; }


		public ModelDescription()
		{
		}


		/// <summary>
		/// Loads the model description (XML file).
		/// </summary>
		/// <param name="fileName">The .</param>
		/// <param name="logger"></param>
		/// <returns>The model description, or <see langword="null"/>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fileName"/> or <paramref name="context"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InvalidContentException">
		/// The model description (.drmdl file) is invalid.
		/// </exception>
		public static ModelDescription Load(string fileName, Action<string> logger)
		{
			if (fileName == null)
				throw new ArgumentNullException("sourceFileName");
			if (fileName.Length == 0)
				throw new ArgumentException("File name must not be empty.", "sourceFileName");

			XDocument document;
			try
			{
				document = XDocument.Load(fileName, LoadOptions.SetLineInfo);
			}
			catch (Exception exception)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Could not load '{0}': {1}", fileName, exception.Message);
				throw new Exception(message);
			}

			var modelElement = document.Root;
			if (modelElement == null || modelElement.Name != "Model")
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Root element \"<Model>\" is missing in XML.");
				throw new Exception(message);
			}

			// Model attributes.
			var modelDescription = new ModelDescription
			{
				FileName = (string)modelElement.Attribute("File") ?? (string)modelElement.Attribute("FileName"),
				GenerateTangentFrames = (bool?)modelElement.Attribute("GenerateTangentFrames") ?? false,
				SwapWindingOrder = (bool?)modelElement.Attribute("SwapWindingOrder") ?? false,
				PremultiplyVertexColors = (bool?)modelElement.Attribute("PremultiplyVertexColors") ?? true,
				MaxDistance = (float?)modelElement.Attribute("MaxDistance") ?? 0.0f
			};

			var aabbMinimumAttribute = modelElement.Attribute("BoundingBoxMinimum");
			var aabbMaximumAttribute = modelElement.Attribute("BoundingBoxMaximum");
			if (aabbMinimumAttribute != null && aabbMaximumAttribute != null)
			{
				modelDescription.BoundingBoxEnabled = true;
				modelDescription.BoundingBoxMinimum = aabbMinimumAttribute.ToVector3(Vector3.Zero);
				modelDescription.BoundingBoxMaximum = aabbMaximumAttribute.ToVector3(Vector3.One);
			}

			// Mesh elements.
			modelDescription.Meshes = new List<MeshDescription>();
			foreach (var meshElement in modelElement.Elements("Mesh"))
			{
				var meshDescription = new MeshDescription
				{
					Name = (string)meshElement.Attribute("Name") ?? string.Empty,
					GenerateTangentFrames = (bool?)meshElement.Attribute("GenerateTangentFrames") ?? modelDescription.GenerateTangentFrames,
					MaxDistance = (float?)meshElement.Attribute("MaxDistance") ?? modelDescription.MaxDistance,
					LodDistance = (float?)meshElement.Attribute("LodDistance") ?? 0.0f,

					Submeshes = new List<SubmeshDescription>()
				};
				foreach (var submeshElement in meshElement.Elements("Submesh"))
				{
					var submeshDescription = new SubmeshDescription
					{
						GenerateTangentFrames = (bool?)meshElement.Attribute("GenerateTangentFrames") ?? meshDescription.GenerateTangentFrames
					};

					meshDescription.Submeshes.Add(submeshDescription);
				}

				modelDescription.Meshes.Add(meshDescription);
			}

			// Animations element.
			var animationsElement = modelElement.Element("Animations");
			if (animationsElement != null)
			{
				var animationDescription = new AnimationDescription();
				animationDescription.MergeFiles = (string)animationsElement.Attribute("MergeFiles");
				animationDescription.Splits = AnimationSplitter.ParseAnimationSplitDefinitions(animationsElement);
				animationDescription.ScaleCompression = (float?)animationsElement.Attribute("ScaleCompression") ?? -1;
				animationDescription.RotationCompression = (float?)animationsElement.Attribute("RotationCompression") ?? -1;
				animationDescription.TranslationCompression = (float?)animationsElement.Attribute("TranslationCompression") ?? -1;
				animationDescription.AddLoopFrame = (bool?)animationsElement.Attribute("AddLoopFrame");

				modelDescription.Animation = animationDescription;
			}

			return modelDescription;
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
					logger?.Invoke(string.Format( "Model description (.drmdl file) contains description for mesh '{0}' which was not found in the asset.",
						meshDescription.Name));
				}
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static bool IsModelDescriptionFile(string fileName)
		{
			try
			{
				XDocument document = XDocument.Load(fileName, LoadOptions.SetLineInfo);
				if (document.Root != null && document.Root.Name == "Model")
					return true;
			}
			catch
			{
			}

			return false;
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
