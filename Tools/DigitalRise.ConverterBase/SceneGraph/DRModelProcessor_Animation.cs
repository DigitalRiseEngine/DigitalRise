// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Globalization;
using DigitalRise.ConverterBase.Animations;
using DigitalRise.Mathematics;


#if ANIMATION
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRise.Animation.Character;
using DigitalRise.Linq;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRise.ConverterBase.SceneGraph
{
	partial class DRModelProcessor
	{
		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		// Merge animations defined in other files.
		private void MergeAnimationFiles()
		{
			if (_modelDescription != null)
			{
				var animationDescription = _modelDescription.Animation;
				if (animationDescription != null)
				{
					var animationFiles = animationDescription.MergeFiles;
					AnimationMerger.Merge(animationFiles, _rootBone.Animations, Logger);
				}
			}
		}


		// Initialize _skeleton from BoneContents.
		private void BuildSkeleton()
		{
			// Get an array of all bones in depth-first order.
			// (Same as MeshHelper.FlattenSkeleton(root).)
			var bones = TreeHelper.GetSubtree(_rootBone, n => n.Children.OfType<BoneContent>(), true)
								  .ToList();

			// Create list of parent indices, bind pose transformations and bone names.
			var boneParents = new List<int>();
			var bindTransforms = new List<SrtTransform>();
			var boneNames = new List<string>();
			int numberOfWarnings = 0;
			foreach (var bone in bones)
			{
				int parentIndex = bones.IndexOf(bone.Parent as BoneContent);
				boneParents.Add(parentIndex);

				// Log warning for invalid transform matrices - but not too many warnings.
				if (numberOfWarnings < 2)
				{
					if (!SrtTransform.IsValid((Matrix44F)bone.Transform))
					{
						if (numberOfWarnings < 1)
							Log("Bone transform is not supported. Bone transform matrices may only contain scaling, rotation and translation.");
						else
							Log("More unsupported bone transform found.");

						numberOfWarnings++;
					}
				}

				bindTransforms.Add(SrtTransform.FromMatrix(bone.Transform));

				if (boneNames.Contains(bone.Name))
				{
					string message = String.Format(CultureInfo.InvariantCulture, "Duplicate bone name (\"{0}\") found.", bone.Name);
					throw new InvalidContentException(message, _input.Identity);
				}

				boneNames.Add(bone.Name);
			}

			// Create and return a new skeleton instance.
			_skeleton = new Skeleton(boneParents, boneNames, bindTransforms);
		}


		// Extracts all animations and stores them in _animations.
		private void BuildAnimations()
		{
			SplitAnimations();

			_animations = new Dictionary<string, SkeletonKeyFrameAnimation>();
			foreach (var item in _rootBone.Animations)
			{
				string animationName = item.Key;
				AnimationContent animationContent = item.Value;

				// Convert the AnimationContent to a SkeletonKeyFrameAnimation.
				var skeletonAnimation = BuildAnimation(animationContent);
				if (skeletonAnimation != null)
					_animations.Add(animationName, skeletonAnimation);
			}
		}


		// Split animation into separate animations based on a split definition defined in XML file.
		private void SplitAnimations()
		{
			if (_modelDescription != null)
			{
				var animationDescription = _modelDescription.Animation;
				if (animationDescription != null)
				{
					var animationsSplits = animationDescription.Splits;
					AnimationSplitter.Split(_rootBone.Animations, animationsSplits, Logger);
				}
			}
		}


		// Converts an AnimationContent to a SkeletonKeyFrameAnimation.
		private SkeletonKeyFrameAnimation BuildAnimation(AnimationContent animationContent)
		{
			string name = animationContent.Name;

			// Add loop frame?
			bool addLoopFrame = false;
			if (_modelDescription != null)
			{
				var animationDescription = _modelDescription.Animation;
				if (animationDescription != null)
				{
					addLoopFrame = animationDescription.AddLoopFrame ?? false;

					if (animationDescription.Splits != null)
					{
						foreach (var split in animationDescription.Splits)
						{
							if (split.Name == name)
							{
								if (split.AddLoopFrame.HasValue)
									addLoopFrame = split.AddLoopFrame.Value;

								break;
							}
						}
					}
				}
			}

			var animation = new SkeletonKeyFrameAnimation { EnableInterpolation = true };

			// Process all animation channels (each channel animates a bone).
			int numberOfKeyFrames = 0;
			foreach (var item in animationContent.Channels)
			{
				string channelName = item.Key;
				AnimationChannel channel = item.Value;

				int boneIndex = _skeleton.GetIndex(channelName);
				if (boneIndex != -1)
				{
					SrtTransform? loopFrame = null;

					var bindPoseRelativeInverse = _skeleton.GetBindPoseRelative(boneIndex).Inverse;
					foreach (AnimationKeyframe keyframe in channel)
					{
						TimeSpan time = keyframe.Time;
						SrtTransform transform = SrtTransform.FromMatrix(keyframe.Transform);

						// The matrix in the key frame is the transformation in the coordinate space of the
						// parent bone. --> Convert it to a transformation relative to the animated bone.
						transform = bindPoseRelativeInverse * transform;

						// To start with minimal numerical errors, we normalize the rotation quaternion.
						transform.Rotation.Normalize();

						if (loopFrame == null)
							loopFrame = transform;

						if (!addLoopFrame || time < animationContent.Duration)
							animation.AddKeyFrame(boneIndex, time, transform);

						numberOfKeyFrames++;
					}

					if (addLoopFrame && loopFrame.HasValue)
						animation.AddKeyFrame(boneIndex, animationContent.Duration, loopFrame.Value);
				}
				else
				{
					Log(string.Format("Found animation for bone \"{0}\", which is not part of the skeleton.", channelName));
				}
			}

			if (numberOfKeyFrames == 0)
			{
				Log("Animation is ignored because it has no keyframes.");
				return null;
			}

			// Compress animation to save memory.
			if (_modelDescription != null)
			{
				var animationDescription = _modelDescription.Animation;
				if (animationDescription != null)
				{
					float removedKeyFrames = animation.Compress(
					  animationDescription.ScaleCompression,
					  animationDescription.RotationCompression,
					  animationDescription.TranslationCompression);

					if (removedKeyFrames > 0)
					{
						Log(string.Format("{0}: Compression removed {1:P} of all key frames.",
							string.IsNullOrEmpty(name) ? "Unnamed" : name,
							removedKeyFrames));
					}
				}
			}

			// Finalize the animation. (Optimizes the animation data for fast runtime access.)
			animation.Freeze();

			return animation;
		}
		#endregion
	}
}
#endif
