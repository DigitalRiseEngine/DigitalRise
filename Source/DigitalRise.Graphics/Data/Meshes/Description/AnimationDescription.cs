// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRise.Data.Meshes.Description
{
	internal class AnimationDescription
	{
		public string MergeFiles { get; set; }
		public float ScaleCompression { get; set; }
		public float RotationCompression { get; set; }
		public float TranslationCompression { get; set; }
		public bool? AddLoopFrame { get; set; }
	}
}
