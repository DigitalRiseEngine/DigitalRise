// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRise.ConverterBase.Animations
{
	/// <summary>
	/// Defines a region in the original (merged) animation.
	/// </summary>
	public class AnimationSplitDefinition
	{
		/// <summary>
		/// The name of the animation.
		/// </summary>
		public string Name;

		/// <summary>
		/// The start time.
		/// </summary>
		public TimeSpan StartTime;

		/// <summary>
		/// The end time.
		/// </summary>
		public TimeSpan EndTime;

		/// <summary>
		/// Gets or sets a value indicating whether to add a loop frame at the end of the animation.
		/// </summary>
		/// <value>
		/// <see langword="true"/> to add a loop frame; otherwise, <see langword="false"/>. The default
		/// value is <see langword="false"/>.
		/// </value>
		public bool? AddLoopFrame { get; set; }
	}
}
