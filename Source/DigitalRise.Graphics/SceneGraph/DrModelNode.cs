using AssetManagementBase;
using DigitalRise.Animation;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Modelling;
using DigitalRise.Geometry;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using DigitalRise.Rendering.Deferred;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DigitalRise.SceneGraph
{
	public class DrModelNode : SceneNode, ISkeleton
	{
		private class SkinInfo
		{
			public Skin Skin { get; }
			public Matrix[] Transforms { get; }

			public SkinInfo(Skin skin)
			{
				Skin = skin ?? throw new ArgumentNullException(nameof(skin));
				Transforms = new Matrix[skin.Joints.Length];
			}
		}

		private bool _transformsDirty = true;
		private Matrix[] _localTransforms;
		private Matrix[] _worldTransforms;
		private SkinInfo[] _skinInfos;

		private DrModel _model;

		[JsonIgnore]
		public DrModel Model
		{
			get => _model;

			set
			{
				if (value == _model)
				{
					return;
				}

				_model = value;

				_localTransforms = null;
				_worldTransforms = null;
				_skinInfos = null;
				if (_model != null)
				{
					_localTransforms = new Matrix[_model.Bones.Length];
					_worldTransforms = new Matrix[_model.Bones.Length];

					var skinInfos = new List<SkinInfo>();
					_model.TraverseNodes(n =>
					{
						if (n.Mesh == null)
						{
							return;
						}

						foreach (var submesh in n.Mesh.Submeshes)
						{
							if (submesh.Skin != null)
							{
								skinInfos.Add(new SkinInfo(submesh.Skin));
							}
						}
					});

					_skinInfos = skinInfos.ToArray();

					ResetTransforms();

					Shape = CalculateBoundingBox().CreateShape();
				}
				else
				{
					Shape = Shape.Empty;
				}
			}
		}

		[Browsable(false)]
		public string ModelPath { get; set; }

		[Browsable(false)]
		public IMaterial[] Materials { get; set; }

		public override bool IsRenderable => true;


		public DrModelNode()
		{
			CastsShadows = true;
		}

		public void ResetTransforms()
		{
			if (Model == null)
			{
				return;
			}

			for (var i = 0; i < Model.Bones.Length; i++)
			{
				var bone = Model.Bones[i];
				_localTransforms[bone.Index] = bone.CalculateDefaultLocalTransform();
			}

			_transformsDirty = true;
		}

		private void UpdateTransforms()
		{
			if (!_transformsDirty)
			{
				return;
			}

			for (var i = 0; i < Model.Bones.Length; i++)
			{
				var bone = Model.Bones[i];

				if (bone.Parent == null)
				{
					_worldTransforms[bone.Index] = _localTransforms[bone.Index];
				}
				else
				{
					_worldTransforms[bone.Index] = _localTransforms[bone.Index] * _worldTransforms[bone.Parent.Index];
				}
			}

			// Update skin transforms
			if (_skinInfos != null)
			{
				for (var i = 0; i < _skinInfos.Length; ++i)
				{
					var skinInfo = _skinInfos[i];
					for (var j = 0; j < skinInfo.Skin.Joints.Length; ++j)
					{
						var joint = skinInfo.Skin.Joints[j];

						skinInfo.Transforms[j] = joint.InverseBindTransform * _worldTransforms[joint.BoneIndex];
					}
				}

			}

			_transformsDirty = false;
		}

		public override void BatchJobs(IRenderList list)
		{
			base.BatchJobs(list);

			if (Model == null)
			{
				return;
			}

			UpdateTransforms();

			// Render meshes
			var rootTransform = CalculateGlobalTransform();
			for (var i = 0; i < _model.MeshBones.Length; ++i)
			{
				var bone = _model.MeshBones[i];

				for (var j = 0; j < bone.Mesh.Submeshes.Count; ++j)
				{
					var submesh = bone.Mesh.Submeshes[j];

					// If mesh has bones, then parent node transform had been already
					// applied to bones transform
					// Thus to avoid applying parent transform twice, we use
					// ordinary Transform(not AbsoluteTransform) for parts with bones
					Matrix transform = submesh.Skin != null ? rootTransform : _worldTransforms[bone.Index] * rootTransform;

					// Apply the effect and render items
					Matrix[] bones = null;
					if (submesh.Skin != null)
					{
						bones = _skinInfos[submesh.Skin.SkinIndex].Transforms;
					}

					list.AddJob(submesh, Materials[submesh.MaterialIndex], transform, bones);
				}
			}
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (Materials != null)
			{
				foreach (var material in Materials)
				{
					var hasExternalAssets = material as IHasExternalAssets;
					if (hasExternalAssets != null)
					{
						hasExternalAssets.Load(assetManager);
					}
				}
			}

			Model = assetManager.LoadJDRM(ModelPath);

			// Update skinning
			if (Model != null)
			{
				Model.TraverseNodes(n =>
				{
					if (n.Mesh == null)
					{
						return;
					}

					foreach (var submesh in n.Mesh.Submeshes)
					{
						var material = Materials[submesh.MaterialIndex];
						var supportsSkinning = material as ISupportsSkinning;
						if (supportsSkinning != null)
						{
							supportsSkinning.Skinning = submesh.Skin != null;
						}

					}
				});
			}
		}

		private BoundingBox CalculateBoundingBox()
		{
			UpdateTransforms();

			var boundingBox = new BoundingBox();
			foreach (var bone in _model.MeshBones)
			{
				foreach (var submesh in bone.Mesh.Submeshes)
				{
					var m = submesh.Skin != null ? Matrix.Identity : _worldTransforms[bone.Index];
					var bb = submesh.BoundingBox.Transform(ref m);
					boundingBox = BoundingBox.CreateMerged(boundingBox, bb);
				}
			}

			return boundingBox;
		}

		public Matrix GetBoneLocalTransform(int boneIndex) => _localTransforms[boneIndex];

		public void SetBoneLocalTransform(int boneIndex, Matrix transform)
		{
			_localTransforms[boneIndex] = transform;
			_transformsDirty = true;
		}

		public Matrix GetBoneGlobalTransform(int boneIndex)
		{
			UpdateTransforms();

			return _worldTransforms[boneIndex];
		}

		public new DrModelNode Clone() => (DrModelNode)base.Clone();

		protected override SceneNode CreateInstanceCore() => new DrModelNode();

		protected override void CloneCore(SceneNode source)
		{
			base.CloneCore(source);

			var src = (DrModelNode)source;
			ModelPath = src.ModelPath;
			Model = src.Model;

			if (src.Materials != null)
			{
				Materials = new IMaterial[src.Materials.Length];

				for (var i = 0; i < Materials.Length; ++i)
				{
					Materials[i] = src.Materials[i].Clone();
				}
			}
			else
			{
				Materials = null;
			}
		}

		public AnimationClip GetClip(string name) => Model.Animations[name];

		public SrtTransform GetDefaultPose(int boneIndex) => Model.Bones[boneIndex].DefaultPose;

		public void SetPose(int boneIndex, SrtTransform pose) => SetBoneLocalTransform(boneIndex, pose);
	}
}
