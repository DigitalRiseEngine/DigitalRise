// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace DigitalRise.Mathematics.Interpolation
{
  /// <summary>
  /// Provides methods for interpolation and approximation of values.
  /// </summary>
  public static class InterpolationHelper
  {
    //--------------------------------------------------------------
    #region Lerp
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Performs a linear interpolation between two values.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs a linear interpolation between two values (single-precision).
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The linear interpolation of two values.</returns>
    /// <remarks>
    /// <para>
    /// <c>Lerp(a, b, 0)</c> returns <c>a</c>. <c>Lerp(a, b, 1)</c> returns <c>b</c>.
    /// </para>
    /// <para>
    /// The interpolation <paramref name="parameter"/> can lie outside of the interval [0, 1]. 
    /// Outside of this interval the method actually performs an extrapolation instead of an 
    /// interpolation. 
    /// </para>
    /// </remarks>
    public static float Lerp(float source, float target, float parameter)
    {
      return source + (target - source) * parameter;
    }


    /// <summary>
    /// Performs a linear interpolation between two values (double-precision).
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The linear interpolation of two values.</returns>
    /// <remarks>
    /// <para>
    /// <c>Lerp(a, b, 0)</c> returns <c>a</c>. <c>Lerp(a, b, 1)</c> returns <c>b</c>.
    /// </para>
    /// <para>
    /// The interpolation <paramref name="parameter"/> can lie outside of the interval [0, 1]. 
    /// Outside of this interval the method actually performs an extrapolation instead of an 
    /// interpolation. 
    /// </para>
    /// </remarks>
    public static double Lerp(double source, double target, double parameter)
    {
      return source + (target - source) * parameter;
    }


    /// <summary>
    /// Performs a linear interpolation between two <see cref="Vector2"/>.
    /// </summary>
    /// <param name="source">The source vector.</param>
    /// <param name="target">The target vector.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The linear interpolation of two vectors.</returns>
    /// <remarks>
    /// <para>
    /// <c>Lerp(a, b, 0)</c> returns <c>a</c>. <c>Lerp(a, b, 1)</c> returns <c>b</c>.
    /// </para>
    /// <para>
    /// The interpolation <paramref name="parameter"/> can lie outside of the interval [0, 1]. 
    /// Outside of this interval the method actually performs an extrapolation instead of an 
    /// interpolation. 
    /// </para>
    /// </remarks>
    public static Vector2 Lerp(Vector2 source, Vector2 target, float parameter)
    {
      // result = source * (1 - weight) + target * weight
      source.X += (target.X - source.X) * parameter;
      source.Y += (target.Y - source.Y) * parameter;
      return source;
    }


    /// <summary>
    /// Performs a linear interpolation between two <see cref="Vector3"/>.
    /// </summary>
    /// <param name="source">The source vector.</param>
    /// <param name="target">The target vector.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The linear interpolation of two vectors.</returns>
    /// <remarks>
    /// <para>
    /// <c>Lerp(a, b, 0)</c> returns <c>a</c>. <c>Lerp(a, b, 1)</c> returns <c>b</c>.
    /// </para>
    /// <para>
    /// The interpolation <paramref name="parameter"/> can lie outside of the interval [0, 1]. 
    /// Outside of this interval the method actually performs an extrapolation instead of an 
    /// interpolation. 
    /// </para>
    /// </remarks>
    public static Vector3 Lerp(Vector3 source, Vector3 target, float parameter)
    {
      // result = source * (1 - weight) + target * weight
      source.X += (target.X - source.X) * parameter;
      source.Y += (target.Y - source.Y) * parameter;
      source.Z += (target.Z - source.Z) * parameter;
      return source;
    }


    /// <summary>
    /// Performs a linear interpolation between two <see cref="Vector4"/>.
    /// </summary>
    /// <param name="source">The source vector.</param>
    /// <param name="target">The target vector.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The linear interpolation of two vectors.</returns>
    /// <remarks>
    /// <para>
    /// <c>Lerp(a, b, 0)</c> returns <c>a</c>. <c>Lerp(a, b, 1)</c> returns <c>b</c>.
    /// </para>
    /// <para>
    /// The interpolation <paramref name="parameter"/> can lie outside of the interval [0, 1]. 
    /// Outside of this interval the method actually performs an extrapolation instead of an 
    /// interpolation. 
    /// </para>
    /// </remarks>
    public static Vector4 Lerp(Vector4 source, Vector4 target, float parameter)
    {
      // result = source * (1 - weight) + target * weight
      source.X += (target.X - source.X) * parameter;
      source.Y += (target.Y - source.Y) * parameter;
      source.Z += (target.Z - source.Z) * parameter;
      source.W += (target.W - source.W) * parameter;
      return source;
    }


    /// <summary>
    /// Performs a linear interpolation of two <see cref="Quaternion"/>.
    /// </summary>
    /// <param name="source">The start quaternion.</param>
    /// <param name="target">The end quaternion.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.</param>
    /// <returns>The linear interpolation of the two quaternions.</returns>
    /// <remarks>
    /// <para>
    /// <c>Lerp(a, b, 0)</c> returns <c>a</c>. <c>Lerp(a, b, 1)</c> returns <c>b</c>.
    /// </para>
    /// <para>
    /// Linear interpolation usually creates non-normalized quaternions. This method automatically 
    /// renormalizes the result.
    /// </para>
    /// <para>
    /// The interpolation <paramref name="parameter"/> can lie outside of the interval [0, 1]. 
    /// Outside of this interval the method actually performs an extrapolation instead of an 
    /// interpolation. 
    /// </para>
    /// </remarks>
    public static Quaternion Lerp(Quaternion source, Quaternion target, float parameter)
    {
      // Get angle between quaternions
      float cosθ = Quaternion.Dot(source, target);

      // Invert one quaternion if we would move along the long arc of interpolation.
      if (cosθ < 0)
      {
        target.W = -target.W;
        target.X = -target.X;
        target.Y = -target.Y;
        target.Z = -target.Z;
      }

      Quaternion result;
      result.W = source.W + (target.W - source.W) * parameter;
      result.X = source.X + (target.X - source.X) * parameter;
      result.Y = source.Y + (target.Y - source.Y) * parameter;
      result.Z = source.Z + (target.Z - source.Z) * parameter;

      // Linear interpolation creates non-normalized quaternions. We need to 
      // re-normalize the result.
      result.Normalize();
      return result;
    }

    #endregion


    //--------------------------------------------------------------
    #region Step
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Performs a step interpolation between two values.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs a step interpolation between two values.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <param name="interpolation">The step interpolation type.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The step interpolation of two values.</returns>
    /// <remarks>
    /// <para>
    /// This interpolation returns either <paramref name="source"/> or <paramref name="target"/>
    /// depending on the <paramref name="parameter"/> and the step type 
    /// (<paramref name="interpolation"/>).
    /// </para>
    /// </remarks>
    public static T Step<T>(T source, T target, float parameter, StepInterpolation interpolation)
    {
      if (interpolation == StepInterpolation.Centered)
        return (parameter < 0.5f) ? source : target;
      else if (interpolation == StepInterpolation.Left)
        return (parameter <= 0f) ? source : target;
      else
        return (parameter < 1f) ? source : target;
    }


    /// <summary>
    /// Performs a step interpolation between two values.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <param name="interpolation">The step interpolation type.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The step interpolation of two values.</returns>
    /// <remarks>
    /// <para>
    /// This interpolation returns either <paramref name="source"/> or <paramref name="target"/>
    /// depending on the <paramref name="parameter"/> and the step type 
    /// (<paramref name="interpolation"/>).
    /// </para>
    /// </remarks>
    public static T Step<T>(T source, T target, double parameter, StepInterpolation interpolation)
    {
      if (interpolation == StepInterpolation.Centered)
        return (parameter < 0.5) ? source : target;
      else if (interpolation == StepInterpolation.Left)
        return (parameter <= 0) ? source : target;
      else
        return (parameter < 1) ? source : target;
    }
    #endregion


    //--------------------------------------------------------------
    #region CosineInterpolation
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Performs a cosine interpolation between two values.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs a cosine interpolation between two values (single-precision).
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The cosine interpolation of two values.</returns>
    /// <remarks>
    /// <c>CosineInterpolation(a, b, 0)</c> returns <c>a</c>. <c>CosineInterpolation(a, b, 1)</c> 
    /// returns <c>b</c>.
    /// </remarks>
    public static float CosineInterpolation(float source, float target, float parameter)
    {
      float u = (1.0f - (float)Math.Cos(parameter * Math.PI)) / 2.0f;
      return source * (1.0f - u) + target * u;
    }


    /// <summary>
    /// Performs a cosine interpolation between two values (double-precision).
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The cosine interpolation of two values.</returns>
    /// <remarks>
    /// <c>CosineInterpolation(a, b, 0)</c> returns <c>a</c>. <c>CosineInterpolation(a, b, 1)</c> 
    /// returns <c>b</c>.
    /// </remarks>
    public static double CosineInterpolation(double source, double target, double parameter)
    {
      double u = (1.0 - Math.Cos(parameter * Math.PI)) / 2.0;
      return source * (1.0 - u) + target * u;
    }


    /// <summary>
    /// Performs a cosine interpolation between two <see cref="Vector2"/>.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The cosine interpolation of two values.</returns>
    /// <remarks>
    /// <c>CosineInterpolation(a, b, 0)</c> returns <c>a</c>. <c>CosineInterpolation(a, b, 1)</c> 
    /// returns <c>b</c>.
    /// </remarks>
    public static Vector2 CosineInterpolation(Vector2 source, Vector2 target, float parameter)
    {
      float u = (1.0f - (float)Math.Cos(parameter * Math.PI)) / 2.0f;
      return source * (1.0f - u) + target * u;
    }


    /// <summary>
    /// Performs a cosine interpolation between two <see cref="Vector3"/>.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The cosine interpolation of two values.</returns>
    /// <remarks>
    /// <c>CosineInterpolation(a, b, 0)</c> returns <c>a</c>. <c>CosineInterpolation(a, b, 1)</c> 
    /// returns <c>b</c>.
    /// </remarks>
    public static Vector3 CosineInterpolation(Vector3 source, Vector3 target, float parameter)
    {
      float u = (1.0f - (float)Math.Cos(parameter * Math.PI)) / 2.0f;
      return source * (1.0f - u) + target * u;
    }


    /// <summary>
    /// Performs a cosine interpolation between two <see cref="Vector4"/>.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The cosine interpolation of two values.</returns>
    /// <remarks>
    /// <c>CosineInterpolation(a, b, 0)</c> returns <c>a</c>. <c>CosineInterpolation(a, b, 1)</c> 
    /// returns <c>b</c>.
    /// </remarks>
    public static Vector4 CosineInterpolation(Vector4 source, Vector4 target, float parameter)
    {
      float u = (1.0f - (float)Math.Cos(parameter * Math.PI)) / 2.0f;
      return source * (1.0f - u) + target * u;
    }


    #endregion


    //--------------------------------------------------------------
    #region Polyomial Interpolation
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Computes the function value for <paramref name="x"/> using polynomial interpolation.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes the function value for <paramref name="x"/> using polynomial interpolation (single-precision).
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="x">The <i>x</i> value.</param>
    /// <returns>The interpolated value <i>y = P(x)</i>.</returns>
    /// <remarks>
    /// Through any <i>N</i> points there is a unique polynomial <i>P(x)</i> of order <i>N-1</i>.
    /// This method constructs the interpolating polynomial through the given points using 
    /// <i>Neville's algorithm</i>. And computes the function value for the given function 
    /// argument <i>x</i>.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static float PolynomialInterpolation(IList<Vector2> points, float x)
    {
      float error;
      return PolynomialInterpolation(points, x, out error);
    }


    /// <summary>
    /// Computes the function value for <paramref name="x"/> using polynomial interpolation 
    /// (single-precision).
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="x">The <i>x</i> value.</param>
    /// <param name="error">The error estimate.</param>
    /// <returns>The interpolated value <i>y = P(x)</i>.</returns>
    /// <remarks>
    /// Through any <i>N</i> points there is a unique polynomial <i>P(x)</i> of order <i>N-1</i>.
    /// This method constructs the interpolating polynomial through the given points using
    /// <i>Neville's algorithm</i>. And computes the function value for the given function
    /// argument <i>x</i>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of points should be greater than 1.
    /// </exception>
    /// <exception cref="MathematicsException">
    /// 2 points have identical x-values (within roundoff tolerance).
    /// </exception>
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public static float PolynomialInterpolation(IList<Vector2> points, float x, out float error)
    {
      if (points == null)
        throw new ArgumentNullException("points");
      if (points.Count == 0)
        throw new ArgumentException("The number of points should be greater than 1.");

      // see Numerical Recipes p. 113, function polint().

      int n = points.Count;
      float[] c = new float[n];
      float[] d = new float[n];
      float dif = Math.Abs(x - points[0].X);
      int ns = 0;
      for (int i = 0; i < n; i++)
      {
        float dift = Math.Abs(x - points[i].X);
        if (dift < dif)
        {
          ns = i;
          dif = dift;
        }
        c[i] = points[i].Y;
        d[i] = points[i].Y;
      }

      float y = points[ns].Y;
      error = 0;
      ns--;

      for (int m = 1; m < n; m++)
      {
        for (int i = 0; i < n - m; i++)
        {
          float ho = points[i].X - x;
          float hp = points[i + m].X - x;
          float w = c[i + 1] - d[i];
          float den = ho - hp;
          if (den == 0.0)
            throw new MathematicsException("2 points have identical x values (within roundoff tolerance).");

          den = w / den;
          d[i] = hp * den;
          c[i] = ho * den;
        }
        float dy = (2 * (ns + 1) < (n - m))
                    ? c[ns + 1]
                    : d[ns--];
        error = dy;
        y += dy;
      }

      return y;
    }

    #endregion


    //--------------------------------------------------------------
    #region Slerp
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Performs a spherical linear interpolation of the two quaternions.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs a spherical linear interpolation of the two <see cref="Quaternion"/>.
    /// </summary>
    /// <param name="source">The start quaternion.</param>
    /// <param name="target">The end quaternion.</param>
    /// <param name="parameter">
    /// The interpolation factor that lies in the interval [0,1]; also known as <i>interpolation
    /// factor</i> or <i>weight of the target value</i>.
    /// </param>
    /// <returns>The spherical linear interpolation of the two quaternions.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> The method assumes the quaternions <paramref name="source"/> and 
    /// <paramref name="target"/> are unit quaternions (i.e. they are normalized).
    /// </para>
    /// <para>
    /// Rotations specified by unit quaternions can be interpolated with an operation
    /// known as <i>spherical linear interpolation (SLERP)</i>. The operation is defined
    /// as:
    /// </para>
    /// <para>
    /// slerp(<i>q<sub>0</sub></i>, <i>q<sub>1</sub></i>, <i>t</i>) = (<i>q<sub>0</sub></i> sin(<i>θ</i>(1-<i>t</i>)) + <i>q<sub>1</sub></i> sin(<i>θt</i>)) / sin<i>θ</i>
    /// where
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <i>q<sub>0</sub></i>, <i>q<sub>1</sub></i> are unit quaternions representing start and 
    /// destination rotation,
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <i>t</i> is the interpolation factor which lies in the interval [0, 1], and
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <i>θ</i> is the angle between <i>q<sub>0</sub></i> and <i>q<sub>1</sub></i>, which can be 
    /// determined by the dot product of <i>q<sub>0</sub></i> and <i>q<sub>1</sub></i>.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The interpolation <paramref name="parameter"/> can lie outside of the 
    /// interval [0, 1]. Outside of this interval the method actually performs an
    /// extrapolation instead of an interpolation. 
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Quaternion Slerp(Quaternion source, Quaternion target, float parameter)
    {
      // Get angle between quaternions
      float cosθ = Quaternion.Dot(source, target);

      //Debug.WriteLineIf(cosθ > 1.0, "SLERP: cosθ of source and target quaternions is > 1.0. source and target quaternions need to be normalized.");

      // Invert one quaternion if we would move along the long arc of interpolation.
      if (cosθ < 0)
      {
        target.W = -target.W;
        target.X = -target.X;
        target.Y = -target.Y;
        target.Z = -target.Z;
        cosθ = -cosθ;
      }

      float f1;
      float f2;

      // Check if the θ >> 0°.
      // θ ≥ 1° => cosθ ≤ 0.9998
      if (cosθ <= 0.9998f)
      {
        // θ > 0 
        // => sin(θ) ≠ 0
        // We can SLERP safely.

        // First, we need to derive θ and sinθ.

        // Original calculation
        //  float θ = (float) Math.Acos(cosθ);
        //  float sinθ = (float) Math.Sin(θ);

        // Optimized calculation (from "Slerping Clock Cycles", Id Software, Paper found on Intel.com)
        float sinθSquare = 1.0f - cosθ * cosθ;
        float sinθ = (float)Math.Sqrt(sinθSquare);
        float θ = (float)Math.Atan2(sinθ, cosθ);

        float sinθInverse = 1.0f / sinθ;

        f1 = (float)Math.Sin((1.0f - parameter) * θ) * sinθInverse;
        f2 = (float)Math.Sin(parameter * θ) * sinθInverse;
      }
      else
      {
        // θ ≈ 0°
        // Mathematically:    limes  (sin ax / sin x) = a
        //                   x --> 0
        // Logically: The two quaternions are so close together that we can
        //            use linear interpolation.
        // LERP instead of SLERP.

        f1 = 1.0f - parameter;
        f2 = parameter;
      }

      Quaternion result;
      result.W = source.W * f1 + target.W * f2;
      result.X = source.X * f1 + target.X * f2;
      result.Y = source.Y * f1 + target.Y * f2;
      result.Z = source.Z * f1 + target.Z * f2;
      return result;
    }

    #endregion


    //--------------------------------------------------------------
    #region Squad
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Interpolates between quaternions using spherical quadrangle interpolation.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Interpolates between quaternions using spherical quadrangle interpolation (single-precision).
    /// </summary>
    /// <param name="q">The source quaternion (<i>q<sub>n</sub></i>).</param>
    /// <param name="a">The first intermediate quaternion (<i>a<sub>n</sub></i>).</param>
    /// <param name="b">The second intermediate quaternion (<i>a<sub>n+1</sub></i>).</param>
    /// <param name="p">The target quaternion (<i>q<sub>n+1</sub></i>).</param>
    /// <param name="t">The interpolation parameter t.</param>
    /// <returns>The interpolated quaternion.</returns>
    /// <remarks>
    /// <para>
    /// The <i>spherical quadrangle interpolation (SQUAD) </i> is a spline-based interpolation 
    /// of rotations (unit quaternion). This operation is also known as 
    /// <i>spherical cubic interpolation</i>.
    /// </para>
    /// <para>
    /// If <i>q<sub>n</sub></i> is a sequence of <i>N</i> quaternions (<i>n</i> = 0 to <i>N</i>-1),
    /// then the smooth interpolation is given by:
    /// </para>
    /// <para>
    /// squad(<i>q<sub>n</sub></i>, <i>a<sub>n</sub></i>, <i>a<sub>n+1</sub></i>, <i>q<sub>n+1</sub></i>, <i>t</i>) 
    ///   = slerp(slerp(<i>q<sub>n</sub></i>, <i>q<sub>n+1</sub></i>, <i>t</i>), 
    ///           slerp(<i>a<sub>n</sub></i>, <i>a<sub>n+1</sub></i>, <i>t</i>), 
    ///           2<i>t</i>(1-<i>t</i>))
    /// where
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <i>q<sub>n</sub></i>, <i>q<sub>n+1</sub></i> represent start and destination rotation,
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <i>t</i> is the interpolation parameter which lies in the interval [0, 1], and
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <para>
    /// <i>a<sub>n</sub></i>, <i>a<sub>n+1</sub></i> are intermediate quaternions which can be 
    /// determined with:
    /// </para>
    /// <para>
    ///   <i>a<sub>n</sub></i> = 
    ///     <i>q<sub>n</sub></i> e<sup>-(ln(<i>q<sub>n</sub><sup>-1</sup></i><i>q<sub>n-1</sub></i>) + ln(<i>q<sub>n</sub><sup>-1</sup></i><i>q<sub>n+1</sub></i>))/4</sup>
    /// </para>
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to interpolate quaternions using 
    /// <see cref="Squad(Quaternion,Quaternion,Quaternion,Quaternion,float)"/>:
    /// <code>
    ///   // Given:
    ///   Quaternion q0, q1, q2, q3;  // A sequence of quaternions
    ///   float t;                     // A interpolation parameter
    /// 
    ///   // We want to interpolate between q1 and q2 by an interpolation factor t
    ///   Quaternion q, a, b, p;
    ///   Quaternion.SquadSetup(q0, q1, q2, q3, out q, out a, out b, out p);
    ///   Quaternion result = Quaternion.Squad(q, a, b, p, t);
    /// </code>
    /// </example>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Quaternion Squad(Quaternion q, Quaternion a, Quaternion b, Quaternion p, float t)
    {
      return Slerp(Slerp(q, p, t),
                   Slerp(a, b, t),
                   2 * t * (1 - t));
    }


    /// <overloads>
    /// <summary>
    /// Calculates the parameters for a spline-based quaternion interpolation.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Calculates the parameters for a spline-based quaternion interpolation (single-precision).
    /// </summary>
    /// <param name="q0">The previous quaternion (<i>q<sub>n-1</sub></i>).</param>
    /// <param name="q1">The source quaternion (<i>q<sub>n</sub></i>).</param>
    /// <param name="q2">The target quaternion (<i>q<sub>n+1</sub></i>).</param>
    /// <param name="q3">The subsequent quaternion (<i>q<sub>n+2</sub></i>).</param>
    /// <param name="q">The source quaternion (<i>q<sub>n</sub></i>).</param>
    /// <param name="a">The first intermediate quaternion (<i>a<sub>n</sub></i>).</param>
    /// <param name="b">The second intermediate quaternion (<i>a<sub>n+1</sub></i>).</param>
    /// <param name="p">The target quaternion (<i>q<sub>n+1</sub></i>).</param>
    /// <remarks>
    /// Given a sequence of quaternions, this method calculates the intermediate quaternions
    /// that are required by the method 
    /// <see cref="Squad(Quaternion,Quaternion,Quaternion,Quaternion,float)"/> to perform a 
    /// smooth spline-based interpolation. See 
    /// <see cref="Squad(Quaternion,Quaternion,Quaternion,Quaternion,float)"/> to find out more.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static void SquadSetup(Quaternion q0, Quaternion q1, Quaternion q2, Quaternion q3,
                                  out Quaternion q, out Quaternion a, out Quaternion b, out Quaternion p)
    {
      q = q1;
      p = q2;

      Quaternion q1Inv = q1.Inverse();
      Quaternion q2Inv = q2.Inverse();

      a = q1 * (((q1Inv * q0).Ln() + (q1Inv * q2).Ln()) * -0.25f).Exp();
      b = q2 * (((q2Inv * q1).Ln() + (q2Inv * q3).Ln()) * -0.25f).Exp();
    }

    #endregion


    //--------------------------------------------------------------
    #region Critically Damped Ease-In/Ease-Out Smoothing
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Updates position and velocity according to a critically damped spring.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Updates position and velocity according to a critically damped spring (single-precision).
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="velocity">The velocity.</param>
    /// <param name="targetPosition">The target position.</param>
    /// <param name="deltaTime">The size of the timestep.</param>
    /// <param name="smoothTime">
    /// <paramref name="smoothTime"/> is parameter that determines how fast the 
    /// <paramref name="targetPosition"/> is reached. <paramref name="smoothTime"/> is equal to the 
    /// time that is required to reach the target when at maximum speed.
    /// </param>
    /// <remarks>
    /// <para>
    /// At the beginning of the method <paramref name="position"/> and <paramref name="velocity"/>
    /// are the current position and velocity of an object. The object should move to the 
    /// <paramref name="targetPosition"/>. At the end of the method <paramref name="position"/>
    /// and <paramref name="velocity"/> are updated to the new position after 
    /// <paramref name="deltaTime"/> has elapsed.
    /// </para>
    /// <para>
    /// The position and velocity are changed according to the movement of a critically damped 
    /// spring. This generates a smooth movement that can be used to interpolate values.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    public static void CriticallyDampedSmoothing(ref float position, ref float velocity, float targetPosition, float deltaTime, float smoothTime)
    {
      // see Critically Damped Ease-In/Ease-Out Smoothing, Game Programming Gems 4, pp. 95

      float ω = 2.0f / smoothTime;
      float x = ω * deltaTime;
      float exp = 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x); // e^(-ω * t)
      float change = position - targetPosition;                    // (y - yd)

      float temp = (velocity + ω * change) * deltaTime;            // (y0' + ω (y - yd)) Δt
      velocity = (velocity - ω * temp) * exp;                      // y' = (y0' - ω * (y0' + ω (y - yd)) Δt) * e^(-ω * t)
      position = targetPosition + (change + temp) * exp;           // y = yd + ((y0 - yd) + (y0' + ω (y - yd)) Δt) * e^(-ω * t)
    }


    /// <summary>
    /// Updates position and velocity according to a critically damped spring (double-precision).
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="velocity">The velocity.</param>
    /// <param name="targetPosition">The target position.</param>
    /// <param name="deltaTime">The size of the timestep.</param>
    /// <param name="smoothTime">
    /// <paramref name="smoothTime"/> is parameter that determines how fast the 
    /// <paramref name="targetPosition"/> is reached. <paramref name="smoothTime"/> is equal to the 
    /// time that is required to reach the target when at maximum speed.
    /// </param>
    /// <remarks>
    /// <para>
    /// At the beginning of the method <paramref name="position"/> and <paramref name="velocity"/>
    /// are the current position and velocity of an object. The object should move to the 
    /// <paramref name="targetPosition"/>. At the end of the method <paramref name="position"/>
    /// and <paramref name="velocity"/> are updated to the new position after 
    /// <paramref name="deltaTime"/> has elapsed.
    /// </para>
    /// <para>
    /// The position and velocity are changed according to the movement of a critically damped 
    /// spring. This generates a smooth movement that can be used to interpolate values.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public static void CriticallyDampedSmoothing(ref double position, ref double velocity, double targetPosition, double deltaTime, double smoothTime)
    {
      // see Critically Damped Ease-In/Ease-Out Smoothing, Game Programming Gems 4, pp. 95

      double ω = 2.0 / smoothTime;
      double x = ω * deltaTime;
      double exp = 1.0 / (1.0 + x + 0.48 * x * x + 0.235 * x * x * x); // e^(-ω * t)
      double change = position - targetPosition;                    // (y - yd)

      double temp = (velocity + ω * change) * deltaTime;            // (y0' + ω (y - yd)) Δt
      velocity = (velocity - ω * temp) * exp;                      // y' = (y0' - ω * (y0' + ω (y - yd)) Δt) * e^(-ω * t)
      position = targetPosition + (change + temp) * exp;           // y = yd + ((y0 - yd) + (y0' + ω (y - yd)) Δt) * e^(-ω * t)
    }
    #endregion


    //--------------------------------------------------------------
    #region Smooth Step Functions
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Computes the Hermite smooth step function.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes the Hermite smooth step function (single-precision).
    /// </summary>
    /// <param name="x">The argument x.</param>
    /// <returns>The function value y.</returns>
    /// <remarks>
    /// <para>
    /// This functions can be visualized as an S-shaped curve based on a Hermite base function: 
    /// y = x<sup>2</sup> ( 3 - 2x ). (This polynomial is one of the base functions of the cubic 
    /// Hermite spline.)
    /// </para>
    /// <para>
    /// The function value y goes from 0 to 1 as x goes from 0 to 1. y = 0 for x &lt; 0 and y = 1 
    /// for x &gt; 1.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static float HermiteSmoothStep(float x)
    {
      x = MathHelper.Clamp(x, 0, 1);
      return x * x * (3 - 2 * x);
    }


    /// <summary>
    /// Computes the Hermite smooth step function (double-precision).
    /// </summary>
    /// <param name="x">The argument x.</param>
    /// <returns>The function value y.</returns>
    /// <remarks>
    /// <para>
    /// This functions can be visualized as an S-shaped curve based on a Hermite base function: 
    /// y = x<sup>2</sup> ( 3 - 2x ). (This polynomial is one of the base functions of the cubic 
    /// Hermite spline.)
    /// </para>
    /// <para>
    /// The function value y goes from 0 to 1 as x goes from 0 to 1. y = 0 for x &lt; 0 and y = 1 
    /// for x &gt; 1.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static double HermiteSmoothStep(double x)
    {
      x = MathHelper.Clamp(x, 0, 1);
      return x * x * (3 - 2 * x);
    }


    /// <overloads>
    /// <summary>
    /// Computes a constant acceleration ease-in/ease-out smooth step function.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes a constant acceleration ease-in/ease-out smooth step function (single-precision).
    /// </summary>
    /// <param name="x">The argument x.</param>
    /// <returns>The function value y.</returns>
    /// <remarks>
    /// <para>
    /// This functions can be visualized as an S-shaped curve based on a constant-acceleration
    /// ease-in/ease-out behavior.
    /// </para>
    /// <para>
    /// The function value y goes from 0 to 1 as x goes from 0 to 1. y = 0 for x &lt; 0 and y = 1 
    /// for x &gt; 1.
    /// </para>
    /// <para>
    /// To understand how the function works, let's assume that x is time and y is the distance.
    /// The function starts at x = 0 with 0 velocity and constant acceleration. The velocity increases
    /// until x = 0.5. Then the velocity decreases with constant acceleration and at x = 1 the 
    /// velocity is 0.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static float EaseInOutSmoothStep(float x)
    {
      // See Book Games Programming Gems, Chapter Interpolation Methods.

      if (x <= 0)
        return 0;

      if (x >= 1)
        return 1;

      if (x < 0.5)
        return 2 * x * x;

      return 1 - 2 * (1 - x) * (1 - x);
    }


    /// <summary>
    /// Computes a constant acceleration ease-in/ease-out smooth step function (double-precision).
    /// </summary>
    /// <param name="x">The argument x.</param>
    /// <returns>The function value y.</returns>
    /// <remarks>
    /// <para>
    /// This functions can be visualized as an S-shaped curve based on a constant-acceleration
    /// ease-in/ease-out behavior.
    /// </para>
    /// <para>
    /// The function value y goes from 0 to 1 as x goes from 0 to 1. y = 0 for x &lt; 0 and y = 1 
    /// for x &gt; 1.
    /// </para>
    /// <para>
    /// To understand how the function works, let's assume that x is time and y is the distance.
    /// The function starts at x = 0 with 0 velocity and constant acceleration. The velocity increases
    /// until x = 0.5. Then the velocity decreases with constant acceleration and at x = 1 the 
    /// velocity is 0.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static double EaseInOutSmoothStep(double x)
    {
      // See Book Games Programming Gems, Chapter Interpolation Methods.

      if (x <= 0)
        return 0;

      if (x >= 1)
        return 1;

      if (x < 0.5)
        return 2 * x * x;

      return 1 - 2 * (1 - x) * (1 - x);
    }
    #endregion
  }
}
