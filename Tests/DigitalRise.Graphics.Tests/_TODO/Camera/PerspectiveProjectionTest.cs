using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using NUnit.Framework;
using NUnit.Utils;


namespace DigitalRise.Graphics.Tests
{
	[TestFixture]
	public class PerspectiveViewVolumeTest
	{
		[Test]
		public void GetWidthAndHeightTest()
		{
			float width, height;
			PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(90), 1, 1, out width, out height);
			AssertExt.AreNumericallyEqual(2, width);
			AssertExt.AreNumericallyEqual(2, height);

			PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, out width, out height);
			AssertExt.AreNumericallyEqual(2.0528009f, width);
			AssertExt.AreNumericallyEqual(1.1547005f, height);

			// We are pretty confident that the ViewVolume.CreateViewVolumeXxx() works. 
			// Use ViewVolume.CreateViewVolumeXxx() to test GetWidthAndHeight().
			Matrix44F projection = Matrix44F.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);
			Matrix44F projection2 = Matrix44F.CreatePerspective(width, height, 1, 10);
			AssertExt.AreNumericallyEqual(projection, projection2);
		}

		[Test]
		public void SetViewVolumeTest()
		{
			PerspectiveViewVolume projection = new PerspectiveViewVolume();
			projection.Set(4, 3, 2, 10);

			PerspectiveViewVolume projection2 = new PerspectiveViewVolume();
			projection2.SetWidthAndHeight(4, 3);
			projection2.Near = 2;
			projection2.Far = 10;

			ViewVolume projection3 = new PerspectiveViewVolume
			{
				Left = -2,
				Right = 2,
				Bottom = -1.5f,
				Top = 1.5f,
				Near = 2,
				Far = 10,
			};

			Matrix44F expected = Matrix44F.CreatePerspective(4, 3, 2, 10);
			AssertExt.AreNumericallyEqual(expected, projection.Projection);
			AssertExt.AreNumericallyEqual(expected, projection2.Projection);
			AssertExt.AreNumericallyEqual(expected, projection3.Projection);
		}

		[Test]
		public void SetViewVolumeFieldOfViewTest()
		{
			PerspectiveViewVolume projection = new PerspectiveViewVolume();
			projection.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);

			PerspectiveViewVolume projection2 = new PerspectiveViewVolume();
			projection2.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f);
			projection2.Near = 1;
			projection2.Far = 10;

			ViewVolume projection3 = new PerspectiveViewVolume
			{
				Left = -2.0528009f / 2.0f,
				Right = 2.0528009f / 2.0f,
				Bottom = -1.1547005f / 2.0f,
				Top = 1.1547005f / 2.0f,
				Near = 1,
				Far = 10,
			};

			Matrix44F expected = Matrix44F.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);
			AssertExt.AreNumericallyEqual(expected, projection.Projection);
			AssertExt.AreNumericallyEqual(expected, projection2.Projection);
			AssertExt.AreNumericallyEqual(expected, projection3.Projection);
		}

		[Test]
		public void SetViewVolumeOffCenterTest()
		{
			PerspectiveViewVolume projection = new PerspectiveViewVolume();
			projection.SetOffCenter(0, 4, 1, 4, 2, 10);

			PerspectiveViewVolume projection2 = new PerspectiveViewVolume();
			projection2.Set(0, 4, 1, 4);
			projection2.Near = 2;
			projection2.Far = 10;

			Matrix44F expected = Matrix44F.CreatePerspectiveOffCenter(0, 4, 1, 4, 2, 10);
			AssertExt.AreNumericallyEqual(expected, projection.Projection);
			AssertExt.AreNumericallyEqual(expected, projection2.Projection);
		}
	}
}
