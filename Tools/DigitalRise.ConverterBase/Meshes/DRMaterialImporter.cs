// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace DigitalRise.ConverterBase.Meshes
{
	/// <summary>
	/// Imports a material definition (.drmat file).
	/// </summary>
	[ContentImporter(".drmat", DisplayName = "Material - DigitalRise Graphics", DefaultProcessor = "DRMaterialProcessor")]
	public class DRMaterialImporter : ContentImporter<DRMaterialContent>
	{
		/// <summary>
		/// Called by the framework when importing a game asset. This is the method called by XNA when
		/// an asset is to be imported into an object that can be recognized by the Content Pipeline.
		/// </summary>
		/// <param name="filename">Name of a game asset file.</param>
		/// <param name="context">
		/// Contains information for importing a game asset, such as a logger interface.
		/// </param>
		/// <returns>Resulting game asset.</returns>
		public override DRMaterialContent Import(string filename, ContentImporterContext context)
		{
			string name = Path.GetFileNameWithoutExtension(filename);
			var identity = new ContentIdentity(filename);
			var definition = XDocument.Load(filename, LoadOptions.SetLineInfo);

			return new DRMaterialContent
			{
				Name = name,
				Identity = identity,
				Definition = definition
			};
		}
	}
}
