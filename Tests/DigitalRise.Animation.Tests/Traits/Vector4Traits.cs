using Microsoft.Xna.Framework;
using NUnit.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;


namespace DigitalRise.Animation.Traits.Tests
{
	[TestFixture]
	public class Vector4TraitsTest
	{
		[Test]
		public void IdentityTest()
		{
			var traits = Vector4Traits.Instance;
			var value = new Vector4(-1, -2, 3, 1);
			Assert.AreEqual(value, traits.Add(value, traits.Identity()));
			Assert.AreEqual(value, traits.Add(traits.Identity(), value));
		}


		[Test]
		public void MultiplyTest()
		{
			var traits = Vector4Traits.Instance;
			var value = new Vector4(-1, -2, 3, 1);
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)Vector4.Zero, (Vector4)traits.Multiply(value, 0)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)value, (Vector4)traits.Multiply(value, 1)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(value + value), (Vector4)traits.Multiply(value, 2)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(value + value + value), (Vector4)traits.Multiply(value, 3)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(-value), (Vector4)traits.Multiply(value, -1)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(-value - value), (Vector4)traits.Multiply(value, -2)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(-value - value - value), (Vector4)traits.Multiply(value, -3)));
		}


		[Test]
		public void FromByTest()
		{
			// IAnimationValueTraits<T> is used in a from-by animation to a add a relative offset to
			// the start value.

			var traits = Vector4Traits.Instance;
			var from = new Vector4(-1, -2, 3, 1);
			var by = new Vector4(4, -5, 6, 1);

			var to = traits.Add(from, by);
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(by + from), (Vector4)to));

			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)from, (Vector4)traits.Add(to, traits.Inverse(by))));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)by, (Vector4)traits.Add(traits.Inverse(from), to)));
		}


		[Test]
		public void CycleOffsetTest()
		{
			// IAnimationValueTraits<T> is used in a cyclic animation to a add the cycle offset in
			// each iteration.

			var traits = Vector4Traits.Instance;
			var first = new Vector4(1, 2, 3, 1);    // Animation value of first key frame.
			var last = new Vector4(-4, 5, -6, 5);   // Animation value of last key frame.
			var cycleOffset = traits.Add(traits.Inverse(first), last);

			// Cycle offset should be the difference between last and first key frame.
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)last, (Vector4)traits.Add(first, cycleOffset)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)last, (Vector4)(cycleOffset + first)));

			// Check multiple cycles (post-loop).
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)last, (Vector4)traits.Add(first, traits.Multiply(cycleOffset, 1))));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(cycleOffset + cycleOffset + last), (Vector4)traits.Add(first, traits.Multiply(cycleOffset, 3))));

			// Check multiple cycles (pre-loop).
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)first, (Vector4)traits.Add(last, traits.Multiply(cycleOffset, -1))));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(first - cycleOffset - cycleOffset), (Vector4)traits.Add(last, traits.Multiply(cycleOffset, -3))));
		}


		[Test]
		public void InterpolationTest()
		{
			var traits = Vector4Traits.Instance;
			var value0 = new Vector4(1, 2, 3, 1);
			var value1 = new Vector4(-4, 5, -6, 5);
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)value0, (Vector4)traits.Interpolate(value0, value1, 0.0f)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)value1, (Vector4)traits.Interpolate(value0, value1, 1.0f)));
			Assert.IsTrue(MathHelper.AreNumericallyEqual((Vector4)(0.25f * value0 + 0.75f * value1), (Vector4)traits.Interpolate(value0, value1, 0.75f)));
		}
	}
}
