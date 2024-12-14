using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DigitalRise.ModelStorage
{
	public class BoneContent
	{
		public string Name { get; set; }
		public MeshContent Mesh { get; set; }
		public Vector3 Scale = Vector3.One;
		public Quaternion Rotation = Quaternion.Identity;
		public Vector3 Translation = Vector3.Zero;
		public List<BoneContent> Children { get; set; } = new List<BoneContent>();

		internal void RecursiveProcess(Action<BoneContent> processor)
		{
			processor(this);

			foreach (var child in Children)
			{
				child.RecursiveProcess(processor);
			}
		}
	}
}
