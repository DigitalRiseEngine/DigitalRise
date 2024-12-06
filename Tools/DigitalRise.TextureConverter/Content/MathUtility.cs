using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace DigitalRise.TextureConverter
{
	internal static class MathUtility
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct SingleToUInt32
		{
			[FieldOffset(0)]
			internal float Single;
			[FieldOffset(0)]
			internal UInt32 UInt32;
		}

		/// <summary>Represents the mathematical constant π.</summary>
		public const float Pi = 3.1415926535897932384626433832795f;

		// Epsilon values from other projects: EpsilonF = 1e-7; EpsilonD = 9e-16;
		// According to our unit tests the double epsilon is to small. 
		// Following epsilon values were appropriate for typical game applications and 3D simulations.
		private static float _epsilonF = 1e-5f;
		private static float _epsilonFSquared = 1e-5f * 1e-5f;


		/// <summary>
		/// Gets or sets the tolerance value used for comparison of <see langword="float"/> values.
		/// </summary>
		/// <value>The epsilon for single-precision floating-point.</value>
		/// <remarks>
		/// This value can be changed to set a new value for all subsequent comparisons.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative or 0.
		/// </exception>
		public static float EpsilonF
		{
			get { return _epsilonF; }
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("value", "The tolerance value must be greater than 0.");

				_epsilonF = value;
				_epsilonFSquared = value * value;
			}
		}

		/// <summary>
		/// Gets the squared tolerance value used for comparison of <see langword="float"/> values.
		/// (<see cref="EpsilonF"/> * <see cref="EpsilonF"/>).
		/// </summary>
		/// <value>The squared epsilon for single-precision floating-point.</value>
		public static float EpsilonFSquared
		{
			get { return _epsilonFSquared; }
		}


		/// <summary>
		/// Creates the smallest bitmask that is greater than or equal to the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// A bitmask where the left bits are 0 and the right bits are 1. The value of the bitmask
		/// is ≥ <paramref name="value"/>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This result can also be interpreted as finding the smallest x such that 2<sup>x</sup> &gt; 
		/// <paramref name="value"/> and returning 2<sup>x</sup> - 1.
		/// </para>
		/// <para>
		/// Another useful application: Bitmask(x) + 1 returns the next power of 2 that is greater than 
		/// x.
		/// </para>
		/// </remarks>
		public static uint Bitmask(uint value)
		{
			// Example:                 value = 10000000 00000000 00000000 00000000
			value |= (value >> 1);   // value = 11000000 00000000 00000000 00000000
			value |= (value >> 2);   // value = 11110000 00000000 00000000 00000000
			value |= (value >> 4);   // value = 11111111 00000000 00000000 00000000
			value |= (value >> 8);   // value = 11111111 11111111 00000000 00000000
			value |= (value >> 16);  // value = 11111111 11111111 11111111 11111111
			return value;
		}


		/// <summary>
		/// Determines whether the specified value is a power of two.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="value"/> is a power of two; otherwise, 
		/// <see langword="false"/>.
		/// </returns>
		public static bool IsPowerOf2(int value)
		{
			// See http://stackoverflow.com/questions/600293/how-to-check-if-a-number-is-a-power-of-2
			return (value != 0) && (value & (value - 1)) == 0;
		}


		/// <summary>
		/// Returns the smallest power of two that is greater than the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// The smallest power of two (2<sup>x</sup>) that is greater than <paramref name="value"/>.
		/// </returns>
		/// <remarks>
		/// For example, <c>NextPowerOf2(7)</c> is <c>8</c> and <c>NextPowerOf2(8)</c> is <c>16</c>.
		/// </remarks>
		public static uint NextPowerOf2(uint value)
		{
			return Bitmask(value) + 1;
		}

		/// <summary>
		/// Returns a value indicating whether the specified number is <see cref="float.NaN"/>.
		/// </summary>
		/// <param name="value">A single-precision floating-point number.</param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="value"/> evaluates to <see cref="float.NaN"/>;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// The standard CLR <see cref="float.IsNaN"/> function is slower than this wrapper, so please
		/// make sure to use this <see cref="IsNaN(float)"/> in performance sensitive code.
		/// </remarks>
		public static bool IsNaN(float value)
		{
			// IEEE 754: 
			//   msb means most significant bit
			//   lsb means least significant bit
			//    1    8              23             ... widths
			//   +-+-------+-----------------------+
			//   |s|  exp  |          man          |
			//   +-+-------+-----------------------+
			//      msb lsb msb                 lsb  ... order
			//  
			//  If exp = 255 and man != 0, then value is NaN regardless of s.
			//
			// => If the argument is any value in the range 0x7f800001 through 0x7fffffff or in the range 
			// 0xff800001 through 0xffffffff, the result will be NaN.
			SingleToUInt32 t = new SingleToUInt32 { Single = value };

			UInt32 exp = t.UInt32 & 0x7f800000;
			UInt32 man = t.UInt32 & 0x007fffff;

			return exp == 0x7f800000 && man != 0;
		}

		/// <overloads>
		/// <summary>
		/// Determines whether two values are equal (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Determines whether two values are equal (regarding the tolerance <see cref="EpsilonF"/>).
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>
		/// <see langword="true"/> if the specified values are equal (within the tolerance); otherwise, 
		/// <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// <strong>Important:</strong> When at least one of the parameters is a 
		/// <see cref="Single.NaN"/> the result is undefined. Such cases should be handled explicitly
		/// by the calling application.
		/// </remarks>
		public static bool AreEqual(float value1, float value2) =>
		  AreEqual(value1, value2, EpsilonF);


		/// <summary>
		/// Determines whether two values are equal (regarding a specific tolerance).
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <param name="epsilon">The tolerance value.</param>
		/// <returns>
		/// <see langword="true"/> if the specified values are equal (within the tolerance); otherwise,
		/// <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// <strong>Important:</strong> When at least one of the parameters is a 
		/// <see cref="Single.NaN"/> the result is undefined. Such cases should be handled explicitly by
		/// the calling application.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="epsilon"/> is negative or 0.
		/// </exception>
		public static bool AreEqual(float value1, float value2, float epsilon)
		{
			if (epsilon <= 0.0f)
				throw new ArgumentOutOfRangeException("epsilon", "Epsilon value must be greater than 0.");

			// Infinity values have to be handled carefully because the check with the epsilon tolerance
			// does not work there. Check for equality in case they are infinite:
			if (value1 == value2)
				return true;

			float delta = value1 - value2;
			return (-epsilon < delta) && (delta < epsilon);
		}

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
		public static bool IsZero(float value)
		{
			return (-_epsilonF < value) && (value < _epsilonF);
		}

		/// <summary>
		/// Determines whether a value is zero (regarding a specific tolerance).
		/// </summary>
		/// <param name="value">The value to test.</param>
		/// <param name="epsilon">The tolerance value.</param>
		/// <returns>
		/// <see langword="true"/> if the specified value is zero (within the tolerance); otherwise, 
		/// <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// A value is zero if |x| &lt; epsilon.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="epsilon"/> is negative or 0.
		/// </exception>
		public static bool IsZero(float value, float epsilon)
		{
			if (epsilon <= 0.0f)
				throw new ArgumentOutOfRangeException("epsilon", "Epsilon value must be greater than 0.");

			return (-epsilon < value) && (value < epsilon);
		}

		public static bool AreNumericallyEqual(Vector4 vector1, Vector4 vector2, float epsilon)
		{
			return AreEqual(vector1.X, vector2.X, epsilon)
					&& AreEqual(vector1.Y, vector2.Y, epsilon)
					&& AreEqual(vector1.Z, vector2.Z, epsilon)
					&& AreEqual(vector1.W, vector2.W, epsilon);
		}

		/// <overloads>
		/// <summary>
		/// Determines whether two vectors are equal (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Determines whether two vectors are equal (regarding the tolerance 
		/// <see cref="MathUtility.EpsilonF"/>).
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>
		/// <see langword="true"/> if the vectors are equal (within the tolerance 
		/// <see cref="MathUtility.EpsilonF"/>); otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// The two vectors are compared component-wise. If the differences of the components are less
		/// than <see cref="MathUtility.EpsilonF"/> the vectors are considered as being equal.
		/// </remarks>
		public static bool AreNumericallyEqual(Vector4 vector1, Vector4 vector2) =>
			AreNumericallyEqual(vector1, vector2, EpsilonF);

		/// <overloads>
		/// <summary>
		/// Determines whether a value is less than another value (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Determines whether a value is less than another value (regarding the tolerance 
		/// <see cref="EpsilonF"/>).
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="value1"/> &lt; <paramref name="value2"/> and the
		/// difference between <paramref name="value1"/> and <paramref name="value2"/> is greater than
		/// or equal to the epsilon tolerance; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool IsLess(float value1, float value2)
		{
			return (value1 < value2) && !AreEqual(value1, value2);
		}

		/// <overloads>
		/// <summary>
		/// Determines whether a value is greater than another value (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Determines whether a value is greater than another value (regarding the tolerance 
		/// <see cref="EpsilonF"/>).
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>
		/// <see langword="true"/> if the difference between <paramref name="value1"/> and 
		/// <paramref name="value2"/> is greater than or equal to the epsilon tolerance and 
		/// <paramref name="value1"/> &gt; <paramref name="value2"/>; otherwise, 
		/// <see langword="false"/>.
		/// </returns>
		public static bool IsGreater(float value1, float value2)
		{
			return (value1 > value2) && !AreEqual(value1, value2);
		}

		/// <summary>
		/// Tries to normalize the vector.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the vector was normalized; otherwise, <see langword="false"/> if 
		/// the vector could not be normalized. (The length is numerically zero.)
		/// </returns>
		public static bool TryNormalize(this ref Vector3 a)
		{
			float lengthSquared = a.LengthSquared();
			if (IsZero(lengthSquared, EpsilonFSquared))
				return false;

			float length = MathF.Sqrt(lengthSquared);

			float scale = 1.0f / length;
			a.X *= scale;
			a.Y *= scale;
			a.Z *= scale;

			return true;
		}

		/// <summary>
		/// Returns a vector with the vector components clamped to the range [min, max].
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="min">The min limit.</param>
		/// <param name="max">The max limit.</param>
		/// <returns>A vector with clamped components.</returns>
		/// <remarks>
		/// This operation is carried out per component. Component values less than 
		/// <paramref name="min"/> are set to <paramref name="min"/>. Component values greater than 
		/// <paramref name="max"/> are set to <paramref name="max"/>.
		/// </remarks>
		public static Vector3 Clamp(Vector3 vector, float min, float max)
		{
			return new Vector3(MathHelper.Clamp(vector.X, min, max),
								MathHelper.Clamp(vector.Y, min, max),
								MathHelper.Clamp(vector.Z, min, max));
		}

		/// <summary>
		/// Returns a vector with the vector components clamped to the range [min, max].
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="min">The min limit.</param>
		/// <param name="max">The max limit.</param>
		/// <returns>A vector with clamped components.</returns>
		/// <remarks>
		/// This operation is carried out per component. Component values less than 
		/// <paramref name="min"/> are set to <paramref name="min"/>. Component values greater than 
		/// <paramref name="max"/> are set to <paramref name="max"/>.
		/// </remarks>
		public static Vector4 Clamp(Vector4 vector, float min, float max)
		{
			return new Vector4(MathHelper.Clamp(vector.X, min, max),
								MathHelper.Clamp(vector.Y, min, max),
								MathHelper.Clamp(vector.Z, min, max),
								MathHelper.Clamp(vector.W, min, max));
		}

		public static Vector3 XYZ(this Vector4 v) => new Vector3(v.X, v.Y, v.Z);

		/// <summary>
		/// Swaps the content of two variables.
		/// </summary>
		/// <typeparam name="T">The type of the objects.</typeparam>
		/// <param name="obj1">First variable.</param>
		/// <param name="obj2">Second variable.</param>
		public static void Swap<T>(ref T obj1, ref T obj2)
		{
			T temp = obj1;
			obj1 = obj2;
			obj2 = temp;
		}
	}
}
