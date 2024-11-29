// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRise.Data.Meshes;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;


namespace DigitalRise.Rendering.Deferred
{
	internal sealed class LightRenderData : IDisposable
	{
		// The Clip submesh and its world matrix.
		public Submesh ClipSubmesh;
		public Matrix44F ClipMatrix;


		public void UpdateClipSubmesh(LightNode node)
		{
			var clip = node.Clip;
			Debug.Assert(clip != null);

			// We have to update the submesh if it is null or disposed.
			//   Submesh == null                            --> Update
			//   Submesh != null && VertexBuffer.IsDisposed --> Update
			//   Submesh != null && VertexBuffer == null    --> This is the EmptyShape. No updated needed.
			if (ClipSubmesh == null || (ClipSubmesh.VertexBuffer != null && ClipSubmesh.VertexBuffer.IsDisposed))
			{
				clip.Shape.GetSubmesh(out ClipSubmesh, out ClipMatrix);

				// Add transform of Clip.
				ClipMatrix = clip.Pose * Matrix44F.CreateScale(clip.Scale) * ClipMatrix;
			}
		}


		public void Dispose()
		{
			ClipSubmesh = null;
		}
	}
}
