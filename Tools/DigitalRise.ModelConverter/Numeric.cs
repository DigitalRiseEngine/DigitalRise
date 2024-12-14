namespace DigitalRise.ModelConverter
{
	internal static class Numeric
	{
		private static float EpsilonF = 1e-5f;

		/// <overloads>
		/// <summary>
		/// Determines whether a value is zero (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Determines whether a <paramref name="value"/> is zero (regarding the tolerance 
		/// <see cref="EpsilonF"/>).
		/// </summary>
		/// <param name="value">The value to test.</param>
		/// <returns>
		/// <see langword="true"/> if the specified value is zero (within the tolerance); otherwise, 
		/// <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// A value is zero if |x| &lt; <see cref="EpsilonF"/>.
		/// </remarks>
		public static bool IsZero(this float value)
		{
			return (-EpsilonF < value) && (value < EpsilonF);
		}
	}
}
