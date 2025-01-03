// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRise.Mathematics.Statistics
{
  /// <summary>
  /// Creates random values using an approximate Gaussian distribution (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// Gaussian distribution is also known as Normal distribution. 
  /// </para>
  /// <para>
  /// The random values generated by this class follow only an approximate Gaussian distribution. 
  /// The distribution curve can be imagined as a typical Gaussian bell curve within +/- 3 standard 
  /// deviations. All random values lie in the interval 
  /// [<see cref="ExpectedValue"/> - 3 * <see cref="StandardDeviation"/>, 
  ///  <see cref="ExpectedValue"/> + 3 * <see cref="StandardDeviation"/>]. No random values outside 
  /// the +/- 3 standard deviation interval are returned. 
  /// </para>
  /// <para>
  /// This approximation is faster and makes the random values more controllable for game 
  /// applications. For example, if in a game tree heights are determined using a real Gaussian
  /// distribution with an expected value of 10m and a standard deviation of 1m, then most trees 
  /// will have a height near 10m. But it would also be possible - unlikely but possible - that a 
  /// tree with height 30m is generated. This would look very odd. Therefore, it is desirable that 
  /// the created random values do not exceed 3 standard deviations.
  /// </para>
  /// </remarks>
  public class FastGaussianDistributionF : Distribution<float>
  {
    // See Game Programming Gems 7, for an explanation.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the expected value.
    /// </summary>
    /// <value>The expected value. The default is 0.</value>
    public float ExpectedValue { get; set; }


    /// <summary>
    /// Gets or sets the standard deviation.
    /// </summary>
    /// <value>The standard deviation. The default is 1.</value>
    public float StandardDeviation { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FastGaussianDistributionF"/> class.
    /// </summary>
    public FastGaussianDistributionF()
    {
      StandardDeviation = 1;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FastGaussianDistributionF"/> class.
    /// </summary>
    /// <param name="expectedValue">The expected value.</param>
    /// <param name="standardDeviation">The standard deviation.</param>
    public FastGaussianDistributionF(float expectedValue, float standardDeviation)
    {
      ExpectedValue = expectedValue;
      StandardDeviation = standardDeviation;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override float Next(Random random)
    {
      if (random == null)
        throw new ArgumentNullException("random");

      float r0 = random.NextFloat(-1, 1);
      float r1 = random.NextFloat(-1, 1);
      float r2 = random.NextFloat(-1, 1);
      float r = r0 + r1 + r2;

      // Apply standard deviation scaling.
      r *= StandardDeviation;

      // Apply expected value offset.
      r += ExpectedValue;

      return r;
    }
    #endregion
  }
}
