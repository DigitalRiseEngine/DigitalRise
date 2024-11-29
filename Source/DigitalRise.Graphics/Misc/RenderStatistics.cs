namespace DigitalRise.Misc
{
	public struct RenderStatistics
	{
		public int EffectsSwitches;
		public int DrawCalls;
		public int VerticesDrawn;
		public int PrimitivesDrawn;

		public void Reset()
		{
			EffectsSwitches = 0;
			DrawCalls = 0;
			VerticesDrawn = 0;
			PrimitivesDrawn = 0;
		}
	}
}
