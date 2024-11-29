using DigitalRise.Misc;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRise.Data.Materials
{
	public class EffectWrapper
	{
		private Effect _effect;

		public Effect Effect
		{
			get
			{
				if (_effect != null)
				{
					if (!DR.EffectsSource.IsEffectValid(_effect))
					{
						var oldEffect = _effect;
						_effect = DR.EffectsSource.UpdateEffect(_effect);
						BindParameters(_effect);

						if (_effect != oldEffect)
						{
							oldEffect.Dispose();
						}
					}
				}

				return _effect;
			}
		}

		public EffectTechniqueCollection Techniques => _effect.Techniques;

		public EffectTechnique CurrentTechnique
		{
			get => _effect.CurrentTechnique;
			set => _effect.CurrentTechnique = value;
		}



		public EffectWrapper(Effect effect)
		{
			_effect = effect ?? throw new ArgumentNullException(nameof(effect));
			BindParameters(effect);
		}

		internal EffectWrapper(string stockEffectPath): this(StockEffects.LoadEffect(stockEffectPath))
		{
		}

		protected virtual void BindParameters(Effect effect)
		{
		}

		public void Validate()
		{
			Effect.Validate();
		}
	}
}
