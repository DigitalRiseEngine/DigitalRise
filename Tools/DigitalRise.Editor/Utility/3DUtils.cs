using DigitalRise.Geometry.Shapes;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;

namespace DigitalRise.Utility
{
	internal static class _3DUtils
	{
		private static readonly BoxShape _boxShapeOneSize = new BoxShape(1, 1, 1);

		public static Shape GetPickBox(this SceneNode obj)
		{
			Shape result = null;
			if (obj != null)
			{
				result = obj.Shape;
			}

			if (obj is LightNode || obj is CameraNode)
			{
				result = _boxShapeOneSize;
			}

			return result;
		}
	}
}