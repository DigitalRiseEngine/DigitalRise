using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRise.Animation;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise.Data.Modelling
{
	public class DrModel
	{
		public DrModelBone[] Bones { get; }
		public DrModelBone[] MeshBones { get; }
		public Skin[] Skins { get; }
		public DrModelBone Root { get; }
		public DrModelBone[] OrderedBones { get; }

		public Dictionary<string, AnimationClip> Animations { get; } = new Dictionary<string, AnimationClip>();

		internal DrModel(DrModelBone[] bones, Skin[] skins, int rootIndex = 0)
		{
			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (bones.Length == 0)
			{
				throw new ArgumentException(nameof(bones), "no bones");
			}

			if (rootIndex < 0 || rootIndex >= bones.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(rootIndex));
			}

			Bones = bones;
			MeshBones = (from bone in bones where bone.Mesh != null select bone).ToArray();
			Skins = skins;
			Root = bones[rootIndex];

			// Build correct traverse order starting from root
			var traverseOrder = new List<DrModelBone>();
			TraverseNodes(n =>
			{
				traverseOrder.Add(n);
			});
			OrderedBones = traverseOrder.ToArray();
		}

		private void TraverseNodes(DrModelBone root, Action<DrModelBone> action)
		{
			action(root);

			foreach (var child in root.Children)
			{
				TraverseNodes(child, action);
			}
		}

		public void TraverseNodes(Action<DrModelBone> action)
		{
			TraverseNodes(Root, action);
		}
	}
}