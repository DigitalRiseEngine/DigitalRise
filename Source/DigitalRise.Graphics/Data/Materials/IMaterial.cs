namespace DigitalRise.Data.Materials
{
	public interface IMaterial: INamedObject
	{
		public BatchEffectBinding EffectGBuffer { get; }
		public BatchEffectBinding EffectShadowMap { get; }
		public BatchEffectBinding EffectMaterial { get; }

		void SetGBufferParameters();
		void SetShadowMapParameters();
		void SetMaterialParameters();

		IMaterial Clone();
	}
}
