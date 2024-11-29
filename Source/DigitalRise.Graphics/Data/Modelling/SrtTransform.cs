/*
 * Pose.cs
 * Author: Bruno Evangelista
 * Copyright (c) 2008 Bruno Evangelista. All rights reserved.
 *
 * THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace DigitalRise.Data.Modelling
{
	/// <summary>
	/// Specifies how translations, orientations and scales are interpolated between keyframes.
	/// </summary>
	public enum InterpolationMode
	{
		/// <summary>
		/// Does not use interpolation.
		/// </summary>
		None,

		/// <summary>
		/// Linear interpolation. Supported on translations and scales.
		/// </summary>
		Linear,

		/// <summary>
		/// Cubic interpolation. Supported on translations and scales.
		/// </summary>
		Cubic,

		/// <summary>
		/// Spherical interpolation. Only supported on orientations.
		/// </summary>
		Spherical
	};

	[Serializable]
	public struct SrtTransform : IEquatable<SrtTransform>
	{
		public Vector3 Scale;
		public Quaternion Rotation;
		public Vector3 Translation;

		public static readonly SrtTransform Identity;

		static SrtTransform()
		{
			Identity.Rotation = Quaternion.Identity;
			Identity.Translation = Vector3.Zero;
			Identity.Scale = Vector3.One;
		}

		public SrtTransform(Matrix m)
		{
			m.Decompose(out Scale, out Rotation, out Translation);
		}

		public Matrix ToMatrix() => Mathematics.MathHelper.CreateTransform(Translation, Scale, Rotation);

		///<summary>	
		/// Interpolates between 2 poses using the specified algorithm
		///</summary>	
		///<param name="pose1">First pose</param>
		///<param name="pose2">Second pose</param>	
		///<param name="amount">Amount of blendign between pose 1 and pose 2</param>	
		///<param name="translationInterpolation">How to blend the translation</param>	
		///<param name="orientationInterpolation">How to blend the orientation</param>
		///<param name="scaleInterpolation">How to blend the scale</param>
		///<returns>The interpolated pose</returns>
		///<exception cref="ArgumentException">If any of the Blend types are not supported</exception>
		public static SrtTransform Interpolate(SrtTransform pose1, SrtTransform pose2, float amount,
			InterpolationMode translationInterpolation, InterpolationMode orientationInterpolation,
			InterpolationMode scaleInterpolation)
		{
			SrtTransform resultPose;

			if (amount < 0 || amount > 1)
				throw new ArgumentException("Amount must be between 0.0 and 1.0 inclusive.");

			switch (translationInterpolation)
			{
				case InterpolationMode.None:
					resultPose.Translation = pose1.Translation;
					break;

				case InterpolationMode.Linear:
					Vector3.Lerp(ref pose1.Translation, ref pose2.Translation, amount,
						out resultPose.Translation);
					break;

				case InterpolationMode.Cubic:
					Vector3.SmoothStep(ref pose1.Translation, ref pose2.Translation, amount,
						out resultPose.Translation);
					break;

				default:
					throw new ArgumentException("Translation interpolation method not supported");
			}

			switch (orientationInterpolation)
			{
				case InterpolationMode.None:
					resultPose.Rotation = pose1.Rotation;
					break;

				case InterpolationMode.Linear:
					Quaternion.Lerp(ref pose1.Rotation, ref pose2.Rotation, amount,
						out resultPose.Rotation);
					break;

				case InterpolationMode.Spherical:
					Quaternion.Slerp(ref pose1.Rotation, ref pose2.Rotation, amount,
						out resultPose.Rotation);
					break;

				default:
					throw new ArgumentException("Orientation interpolation method not supported");
			}

			switch (scaleInterpolation)
			{
				case InterpolationMode.None:
					resultPose.Scale = pose1.Scale;
					break;

				case InterpolationMode.Linear:
					Vector3.Lerp(ref pose1.Scale, ref pose2.Scale, amount,
						out resultPose.Scale);
					break;

				case InterpolationMode.Cubic:
					Vector3.SmoothStep(ref pose1.Scale, ref pose2.Scale, amount,
						out resultPose.Scale);
					break;

				default:
					throw new ArgumentException("Scale interpolation method not supported");
			}

			return resultPose;
		}

		#region IEquatable<Pose> Members

		public override int GetHashCode()
		{
			return Translation.GetHashCode() + Rotation.GetHashCode() + Scale.GetHashCode();
		}

		public override string ToString()
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			return string.Format(currentCulture,
				"{{Translation:{0}\n Orientation:{1}\n Scale:{2}\n}}", new object[]
				{ Translation.ToString(), Rotation.ToString(), Scale.ToString() });
		}

		public bool Equals(SrtTransform other)
		{
			return Translation == other.Translation &&
				Rotation == other.Rotation &&
				Scale == other.Scale;
		}

		public override bool Equals(object obj)
		{
			bool result = false;

			if (obj is SrtTransform)
			{
				result = Equals((SrtTransform)obj);
			}

			return result;
		}

		public static bool operator ==(SrtTransform pose1, SrtTransform pose2)
		{
			return pose1.Translation == pose2.Translation &&
				pose1.Rotation == pose2.Rotation &&
				pose1.Scale == pose2.Scale;
		}

		public static bool operator !=(SrtTransform pose1, SrtTransform pose2)
		{
			return pose1.Translation != pose2.Translation ||
				pose1.Rotation != pose2.Rotation ||
				pose1.Scale != pose2.Scale;
		}

		#endregion
	}
}