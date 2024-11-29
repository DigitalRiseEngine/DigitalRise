using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;


namespace DigitalRise.Graphics.Tests
{
  [TestFixture]
  public class GraphicsHelperTest
  {
    private GraphicsDevice _graphicsDevice0;
    private GraphicsDevice _graphicsDevice1;

    [SetUp]
    public void SetUp()
    {
      var parameters = new PresentationParameters
      {
        BackBufferWidth = 1280,
        BackBufferHeight = 720,
        BackBufferFormat = SurfaceFormat.Color,
        DepthStencilFormat = DepthFormat.Depth24Stencil8,
        PresentationInterval = PresentInterval.Immediate,
        IsFullScreen = false
      };

      _graphicsDevice0 = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
      _graphicsDevice1 = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
    }

    [TearDown]
    public void TearDown()
    {
      _graphicsDevice0.Dispose();
      _graphicsDevice1.Dispose();
    }
  }
}