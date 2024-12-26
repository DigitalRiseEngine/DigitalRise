// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRise.SceneGraph
{
	/// <summary>
	/// Flags used in scene nodes.
	/// </summary>
	[Flags]
	internal enum SceneNodeFlags
	{
		None = 0,
		IsDisposed = 1 << 0,
		IsBoundingBoxDirty = 1 << 1,
		IsScaleWorldDirty = 1 << 2,
		IsPoseWorldDirty = 1 << 3,
		HasLastScaleWorld = 1 << 4,
		HasLastPoseWorld = 1 << 5,
		IsDirty = 1 << 6,       // General purpose flag. Usage depends on scene node type.
		IsEnabled = 1 << 7,
		IsStatic = 1 << 8,
		CastsShadows = 1 << 9,
		IsShadowCasterCulled = 1 << 10,
		HasAlpha = 1 << 11,     // Does the node have an Alpha value that can be changed?

		// Following flags share the same bit. Only one can be used per SceneNode type.
		IsAlphaSet = 1 << 12,   // Is the current Alpha value != 1?
		InvertClip = 1 << 12,   // LightNode.InvertClip

		IsDirtyScene = 1 << 13, // Like is IsDirty, but resetting is controlled by Scene.

		// Upper 16 bit are reserved for SceneNode.UserFlag.
		// Possible extensions: IsReflected, IsSelected, etc.
	}
}
