namespace DigitalRise.Misc
{
	public struct RenderStatistics
	{
		public int EffectsSwitches;
		public int DrawCalls;
		public int VerticesDrawn;
		public int PrimitivesDrawn;
		public int RenderTargetSwitches;

		public void Reset()
		{
			EffectsSwitches = 0;
			DrawCalls = 0;
			VerticesDrawn = 0;
			PrimitivesDrawn = 0;
			RenderTargetSwitches = 0;
		}
	}
}
