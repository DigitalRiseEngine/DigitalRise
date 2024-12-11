using DigitalRise.Animation.Character;
using DigitalRise.Data.Meshes;
using DigitalRise.Mathematics;
using System;
using System.Collections.Generic;

namespace DigitalRise.Data.Modelling
{
	/// <summary>
	/// Model bone descriptor
	/// </summary>
	public class DrModelBoneDesc
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
		public readonly List<DrModelBoneDesc> Children = new List<DrModelBoneDesc>();

		/// <summary>
		/// Skin of the bone meshes
		/// </summary>
		public int? SkinIndex { get; set; }
	}

	/// <summary>
	/// Grants ability to create a Model at the run-time
	/// </summary>
	public static class DrModelBuilder
	{
		private static DrModelBone CreateBone(DrModelBoneDesc desc, ref int boneIndex, List<Skin> skins)
		{
			var bone = new DrModelBone(boneIndex, desc.Name)
			{
				DefaultPose = desc.SrtTransform,
				Mesh = desc.Mesh,
			};

			++boneIndex;

			var children = new List<DrModelBone>();
			foreach (var child in desc.Children)
			{
				children.Add(CreateBone(child, ref boneIndex, skins));
			}

			bone.Children = children.ToArray();

			if (desc.SkinIndex != null)
			{
				bone.Skin = skins[desc.SkinIndex.Value];
			}

			return bone;

		}

		/// <summary>
		/// Creates the model
		/// </summary>
		/// <param name="rootBoneDesc"></param>
		/// <param name="skins">Skins of the model</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static DrModel Create(DrModelBoneDesc rootBoneDesc, List<Skin> skins)
		{
			if (rootBoneDesc == null)
			{
				throw new ArgumentNullException(nameof(rootBoneDesc));
			}

			// Root bone
			var boneIndex = 0;
			var rootBone = CreateBone(rootBoneDesc, ref boneIndex, skins);

			// Create the model
			return new DrModel(rootBone, skins != null ? skins.ToArray() : null);
		}
	}
}
