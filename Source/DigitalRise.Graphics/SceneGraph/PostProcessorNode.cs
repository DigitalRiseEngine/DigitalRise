using DigitalRise.Attributes;
using DigitalRise.Geometry.Shapes;
using DigitalRise.PostProcessing;
using DigitalRise.PostProcessing.Processing;

namespace DigitalRise.SceneGraph
{
	[EditorInfo("PostProcessor")]
	public class PostProcessorNode: SceneNode
	{
		[EditorOption(typeof(Blur))]
		[EditorOption(typeof(DownsampleFilter))]
		[EditorOption(typeof(UpsampleFilter))]
		public PostProcessor Processor { get; set; }

		public PostProcessorNode()
		{
			Shape = Shape.Infinite;
		}
	}
}
