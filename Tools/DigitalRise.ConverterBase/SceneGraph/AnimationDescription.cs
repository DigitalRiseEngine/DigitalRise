// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.ConverterBase.Animations;
using System.Collections.Generic;


namespace DigitalRise.ConverterBase.SceneGraph
{
	public class AnimationDescription
	{
		public string MergeFiles { get; set; }
		public List<AnimationSplitDefinition> Splits { get; set; }
		public float ScaleCompression { get; set; }
		public float RotationCompression { get; set; }
		public float TranslationCompression { get; set; }
		public bool? AddLoopFrame { get; set; }
	}
}
