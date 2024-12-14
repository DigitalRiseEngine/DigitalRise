using System.Collections.Generic;

namespace DigitalRise.ModelStorage
{
	public class AnimationClipContent
	{
		public string Name { get; set; }
		public List<AnimationChannelContent> Channels { get; set; } = new List<AnimationChannelContent>();
	}
}
