using DigitalRise.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise.Misc
{
	internal enum RenderTarget2DViewportSizedType
	{
		Normal,
		Half
	}


	internal class RenderTarget2DViewportSized
	{
		private RenderTarget2D _renderTarget;

		public bool? Mipmap { get; private set; }
		public SurfaceFormat? SurfaceFormat { get; private set; }
		public DepthFormat? DepthStencilFormat { get; private set; }
		public int? MultiSampleCount { get; private set; }
		public RenderTargetUsage? Usage { get; private set; }
		public RenderTarget2DViewportSizedType Type { get; private set; }

		public RenderTarget2D Target => _renderTarget;

		public RenderTarget2DViewportSized(bool? mipmap = null, SurfaceFormat? surfaceFormat = null, DepthFormat? depthFormat = null,
			int? multisampleCount = null, RenderTargetUsage usage = RenderTargetUsage.DiscardContents, RenderTarget2DViewportSizedType type = RenderTarget2DViewportSizedType.Normal)
		{
			Mipmap = mipmap;
			SurfaceFormat = surfaceFormat;
			DepthStencilFormat = depthFormat;
			MultiSampleCount = multisampleCount;
			Usage = usage;
			Type = type;
		}

		public void Update(RenderContext context)
		{
			var viewport = DR.GraphicsDevice.Viewport;

			var width = viewport.Width;
			var height = viewport.Height;

			switch (Type)
			{
				case RenderTarget2DViewportSizedType.Half:
					width /= 2;
					height /= 2;
					break;
			}

			if (_renderTarget == null || _renderTarget.Width != width || _renderTarget.Height != height)
			{
				Reset(context);

				var renderFormat = new RenderTargetFormat(width, height, Mipmap, SurfaceFormat, DepthStencilFormat, MultiSampleCount, Usage);
				_renderTarget = context.RenderTargetPool.Obtain2D(renderFormat);
			}
		}

		public void Reset(RenderContext context)
		{
			if (_renderTarget != null)
			{
				context.RenderTargetPool.Recycle(_renderTarget);
				_renderTarget = null;
			}
		}
	}
}
