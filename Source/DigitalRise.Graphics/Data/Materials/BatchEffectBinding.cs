using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise.Data.Materials
{
	public class BatchEffectBinding: EffectWrapper
	{
		private static int _lastBatchId = 0;

		public int BatchId { get; }

		public EffectParameter World { get; private set; }
		public EffectParameter View { get; private set; }
		public EffectParameter Projection { get; private set; }
		public EffectParameter CameraNear { get; private set; }
		public EffectParameter CameraFar { get; private set; }
		public EffectParameter ViewportSize { get; private set; }
		public EffectParameter SceneNodeType { get; private set; }
		public EffectParameter Bones { get; private set; }

		public EffectParameter NormalsFittingTexture {  get; private set; }
		public EffectParameter LightBuffer0 { get; private set; }
		public EffectParameter LightBuffer1 { get; private set; }

		public BatchEffectBinding(Effect effect): base(effect)
		{
			BatchId = _lastBatchId;
			++_lastBatchId;
		}

		internal BatchEffectBinding(string localPath): base(localPath)
		{
			BatchId = _lastBatchId;
			++_lastBatchId;
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			World = effect.Parameters["World"];
			View = effect.Parameters["View"];
			Projection = effect.Parameters["Projection"];
			CameraNear = effect.Parameters["CameraNear"];
			CameraFar = effect.Parameters["CameraFar"];
			ViewportSize = effect.Parameters["ViewportSize"];
			SceneNodeType = effect.Parameters["SceneNodeType"];
			NormalsFittingTexture = effect.Parameters["NormalsFittingTexture"];
			Bones = effect.Parameters["Bones"];
			LightBuffer0 = effect.Parameters["LightBuffer0"];
			LightBuffer1 = effect.Parameters["LightBuffer1"];
		}
	}
}
