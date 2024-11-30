using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics.Algebra;
using NUnit.Framework;
using NUnit.Utils;


namespace DigitalRise.Graphics.Tests
{
	[TestFixture]
	public class OrthographicViewVolumeTest
	{
		[Test]
		public void SetViewVolumeTest()
		{
			OrthographicViewVolume projection = new OrthographicViewVolume();
			projection.Set(4, 3, 2, 10);

			OrthographicViewVolume camera2 = new OrthographicViewVolume();
			camera2.SetWidthAndHeight(4, 3);
			camera2.Near = 2;
			camera2.Far = 10;

			OrthographicViewVolume camera3 = new OrthographicViewVolume
			{
				Left = -2,
				Right = 2,
				Bottom = -1.5f,
				Top = 1.5f,
				Near = 2,
				Far = 10,
			};

			Matrix44F expected = Matrix44F.CreateOrthographic(4, 3, 2, 10);
			AssertExt.AreNumericallyEqual(expected, projection.Projection);
			AssertExt.AreNumericallyEqual(expected, camera2.Projection);
			AssertExt.AreNumericallyEqual(expected, camera3.Projection);
		}

		[Test]
		public void SetViewVolumeOffCenterTest()
		{
			OrthographicViewVolume projection = new OrthographicViewVolume();
			projection.SetOffCenter(0, 4, 1, 4, 2, 10);

			OrthographicViewVolume camera2 = new OrthographicViewVolume();
			camera2.Set(0, 4, 1, 4);
			camera2.Near = 2;
			camera2.Far = 10;

			ViewVolume camera3 = new OrthographicViewVolume
			{
				Left = 0,
				Right = 4,
				Bottom = 1,
				Top = 4,
				Near = 2,
				Far = 10,
			};

			Matrix44F expected = Matrix44F.CreateOrthographicOffCenter(0, 4, 1, 4, 2, 10);
			AssertExt.AreNumericallyEqual(expected, projection.Projection);
			AssertExt.AreNumericallyEqual(expected, camera2.Projection);
			AssertExt.AreNumericallyEqual(expected, camera3.Projection);
		}
	}
}