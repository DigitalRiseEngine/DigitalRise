﻿// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRise.ConverterBase.SceneGraph
{
	/// <summary>
	/// Writes a <see cref="DRLodGroupNodeContent"/> to binary format that can be read by the 
	/// <strong>LodGroupNodeReader</strong> to load a <strong>LodGroupNode</strong>.
	/// </summary>
	[ContentTypeWriter]
	public class DRLodGroupNodeWriter : ContentTypeWriter<DRLodGroupNodeContent>
	{
		/// <summary>
		/// Gets the assembly qualified name of the runtime target type.
		/// </summary>
		/// <param name="targetPlatform">The target platform.</param>
		/// <returns>The qualified name.</returns>
		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return "DigitalRise.Graphics.SceneGraph.LodGroupNode, DigitalRise.Graphics, Version=1.2.0.0";
		}


		/// <summary>
		/// Gets the assembly qualified name of the runtime loader for this type.
		/// </summary>
		/// <param name="targetPlatform">Name of the platform.</param>
		/// <returns>Name of the runtime loader.</returns>
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "DigitalRise.Graphics.Content.LodGroupNodeReader, DigitalRise.Graphics, Version=1.2.0.0";
		}


		/// <summary>
		/// Compiles a strongly typed object into binary format.
		/// </summary>
		/// <param name="output">The content writer serializing the value.</param>
		/// <param name="value">The value to write.</param>
		protected override void Write(ContentWriter output, DRLodGroupNodeContent value)
		{
			// SceneNode properties (base class).
			output.WriteRawObject<DRSceneNodeContent>(value);

			// LodGroupNode properties.
			output.Write(value.Levels.Count);
			foreach (var node in value.Levels)
			{
				output.Write(node.LodDistance);
				output.WriteObject(node);
			}
		}
	}
}
