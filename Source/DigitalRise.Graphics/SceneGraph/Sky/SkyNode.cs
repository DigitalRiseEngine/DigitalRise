// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Geometry.Shapes;
using DigitalRise.SceneGraph;


namespace DigitalRise.SceneGraph.Sky
{
	/// <summary>
	/// Renders graphics in the background, such as distant mountains, sky, stars, etc.
	/// </summary>
	public abstract class SkyNode : SceneNode
	{
		// Regarding naming: 
		// "Sky [...] refers to everything that lies a certain distance above the surface 
		// of Earth, including the atmosphere and the rest of outer space." -- Wikipedia


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets the draw order.
		/// </summary>
		/// <value>
		/// The draw order. The value must be in the range [0, 65535]. The default value is 0.
		/// </value>
		/// <remarks>
		/// This property defines the order in which <see cref="SkyNode"/>s should be drawn.
		/// Nodes with a lower <see cref="DrawOrder"/> should be drawn first. Nodes with a higher
		/// <see cref="DrawOrder"/> should be drawn last. This means that usually nodes with a higher
		/// draw order cover nodes with a lower draw order.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is out of range.
		/// </exception>
		public int DrawOrder
		{
			get { return _drawOrder; }
			set
			{
				if (_drawOrder < 0 || _drawOrder > ushort.MaxValue)
					throw new ArgumentOutOfRangeException("value", "The draw order must be in the range [0, 65535].");

				_drawOrder = value;
			}
		}
		private int _drawOrder;

		public override bool IsRenderable => true;

		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="SkyNode" /> class.
		/// </summary>
		protected SkyNode()
		{
			Shape = Shape.Infinite;
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <inheritdoc/>
		protected override void CloneCore(SceneNode source)
		{
			// Clone SceneNode properties.
			base.CloneCore(source);

			// Clone SkyNode properties.
			var sourceTyped = (SkyNode)source;
			DrawOrder = sourceTyped.DrawOrder;
		}
		#endregion
	}
}
