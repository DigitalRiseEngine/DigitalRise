using DigitalRise.Animation.Character;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRise.Data.Modelling
{
	public class DrModel
	{
		public DrModelBone Root { get; }
		public DrModelBone[] Bones { get; }
		public DrModelBone[] MeshBones { get; }

		public Dictionary<string, AnimationClip> Animations { get; } = new Dictionary<string, AnimationClip>();

		internal DrModel(DrModelBone root)
		{
			if (root == null)
			{
				throw new ArgumentNullException(nameof(root));
			}

			Root = root;

			// Build correct traverse order starting from root
			var traverseOrder = new List<DrModelBone>();
			TraverseNodes(traverseOrder.Add);
			Bones = traverseOrder.ToArray();

			MeshBones = (from bone in Bones where bone.Mesh != null select bone).ToArray();
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