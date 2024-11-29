using AssetManagementBase;
using DigitalRise.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace DigitalRise.Data.Materials
{
	partial class DefaultMaterial
	{
		internal static DefaultMaterial FromXml(AssetManager assetManager, string xml)
		{
			var material = new DefaultMaterial();

			// Parse XML file.
			var document = XDocument.Parse(xml);

			var materialElement = document.Root;
			if (materialElement == null || materialElement.Name != "Material")
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Root element \"<Material>\" is missing in XML.");
				throw new Exception(message);
			}

			// Override material name, if attribute is set.
			material.Name = (string)materialElement.Attribute("Name") ?? material.Name;

			// Create effect bindings for all render passes.
			foreach (var passElement in materialElement.Elements("Pass"))
			{
				// Skip this pass if the graphics profile does not match the actual target profile.
				string profile = (string)passElement.Attribute("Profile") ?? "ANY";
				string profileLower = profile.ToUpperInvariant();
				if (profileLower == "REACH")
				{
					throw new Exception("Reach profile isn't supported.");
				}
				else if (profileLower != "HIDEF" && profileLower != "ANY")
				{
					string message = XmlHelper.GetExceptionMessage(passElement, "Unknown profile: \"{0}\". Allowed values are \"HiDef\" or \"Any\"", profile);
					throw new Exception(message);
				}

				// ----- Parameters
				foreach (var parameterElement in passElement.Elements("Parameter"))
				{
					string name = parameterElement.GetMandatoryAttribute("Name");
					switch (name)
					{
						case "SpecularPower":
							material.SpecularPower = parameterElement.ToParameterValue<float>();
							break;
						case "DiffuseColor":
							material.DiffuseColor = new Color(parameterElement.ToParameterValue<Vector3>());
							break;
						case "SpecularColor":
							material.SpecularColor = new Color(parameterElement.ToParameterValue<Vector3>());
							break;

						default:
							throw new Exception($"Unknown parameter {name}");
					}
				}

				// ----- Textures
				foreach (var textureElement in passElement.Elements("Texture"))
				{
					string name = textureElement.GetMandatoryAttribute("Name");

					string fileName = textureElement.GetMandatoryAttribute("File");

					var texture = assetManager.LoadTexture(DR.GraphicsDevice, fileName);

					switch (name)
					{
						case "DiffuseTexture":
							material.DiffuseTexture = (Texture2D)texture;
							break;

						case "SpecularTexture":
							material.SpecularTexture = (Texture2D)texture;
							break;

						default:
							throw new Exception($"Unknown texture parameter {name}");
					}
					// Texture processor parameters.
					// TODO: Do something with those params
					/*
					var colorKeyAttribute = textureElement.Attribute("ColorKey");
					bool colorKeyEnabled = colorKeyAttribute != null;
					Color colorKeyColor = colorKeyAttribute.ToColor(Color.Magenta);
					bool generateMipmaps = (bool?)textureElement.Attribute("GenerateMipmaps") ?? true;
					float inputGamma = (float?)textureElement.Attribute("InputGamma") ?? 2.2f;
					float outputGamma = (float?)textureElement.Attribute("OutputGamma") ?? 2.2f;
					bool premultiplyAlpha = (bool?)textureElement.Attribute("PremultiplyAlpha") ?? true;
					bool resizeToPowerOfTwo = (bool?)textureElement.Attribute("ResizeToPowerOfTwo") ?? false;
					float referenceAlpha = (float?)textureElement.Attribute("ReferenceAlpha") ?? 0.9f;
					bool scaleAlphaToCoverage = (bool?)textureElement.Attribute("ScaleAlphaToCoverage") ?? false;*/
				}
			}

			return material;
		}
	}
}
