using DigitalRise.Mathematics;

namespace DigitalRise.Animation.Character
{
	public interface ISkeleton
	{
		void ResetTransforms();
		AnimationClip GetClip(string name);
		SrtTransform GetDefaultPose(int boneIndex);
		void SetPose(int boneIndex, SrtTransform pose);
	}
}
