using DigitalRise.Animation.Character;
using DigitalRise.Data.Meshes;
using DigitalRise.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRise.Data.Modelling
{
	/// <summary>
	/// Model bone descriptor
	/// </summary>
	public class NursiaModelBoneDesc
	{
		/// <summary>
		/// Name of the model bone
		/// </summary>
		public string Name { get; set; }

		public SrtTransform SrtTransform = SrtTransform.Identity;

		/// <summary>
		/// Mesh of the model bone
		/// </summary>
		public Mesh Mesh { get; set; }

		/// <summary>
		/// Children of the model bone
		/// </summary>
		public readonly List<int> ChildrenIndices = new List<int>();

		/// <summary>
		/// Skin of the bone meshes
		/// </summary>
		public int? SkinIndex { get; set; }

		internal int Index { get; set; }
	}

	/// <summary>
	/// Grants ability to create a Model at the run-time
	/// </summary>
	public static class NursiaModelBuilder
	{
		/// <summary>
		/// Creates the model
		/// </summary>
		/// <param name="bones">Bones of the model</param>
		/// <param name="skins">Skins of the model</param>
		/// <param name="rootBoneIndex">Index of the root node</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static DrModel Create(List<NursiaModelBoneDesc> bones, List<Skin> skins, int rootBoneIndex = 0)
		{
			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (bones.Count == 0)
			{
				throw new ArgumentException(nameof(bones), "no bones");
			}

			if (rootBoneIndex < 0 || rootBoneIndex >= bones.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rootBoneIndex));
			}

			// Assign indexes
			for (var i = 0; i < bones.Count; ++i)
			{
				bones[i].Index = i;
			}

			// Create bones
			var allBones = new List<DrModelBone>();
			for (var i = 0; i < bones.Count; ++i)
			{
				var desc = bones[i];
				var bone = new DrModelBone(i, desc.Name)
				{
					DefaultPose = desc.SrtTransform,
					Mesh = desc.Mesh,
				};

				allBones.Add(bone);
			}

			// Assign children and skins
			for (var i = 0; i < bones.Count; ++i)
			{
				var desc = bones[i];
				var bone = allBones[i];

				var childrenArray = (from c in desc.ChildrenIndices select allBones[c]).ToArray();
				bone.Children = childrenArray;

				if (desc.SkinIndex != null)
				{
					bone.Skin = skins[desc.SkinIndex.Value];
				}
			}

			// Create the model
			return new DrModel(allBones.ToArray(), skins != null ? skins.ToArray() : null, rootBoneIndex);
		}
	}
}
