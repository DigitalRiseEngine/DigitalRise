using AssetManagementBase;
using DigitalRise.Attributes;
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
	[EditorInfo("Model")]
	public class DrModelNode : SceneNode
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
					if (_model.Skins != null && _model.Skins.Length > 0)
					{
						_skinInfos = new SkinInfo[_model.Skins.Length];
						for (var i = 0; i < _model.Skins.Length; ++i)
						{
							_skinInfos[i] = new SkinInfo(_model.Skins[i]);
						}
					}

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

		public DrModelNode()
		{
			IsRenderable = true;
			CastsShadows = true;
		}

		public void ResetTransforms()
		{
			if (Model == null)
			{
				return;
			}

			for (var i = 0; i < Model.OrderedBones.Length; i++)
			{
				var bone = Model.OrderedBones[i];
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

			for (var i = 0; i < Model.OrderedBones.Length; i++)
			{
				var bone = Model.OrderedBones[i];

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

						skinInfo.Transforms[j] = joint.InverseBindTransform * _worldTransforms[joint.Bone.Index];
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
			foreach (var bone in _model.MeshBones)
			{
				// If mesh has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				Matrix transform = bone.Skin != null ? rootTransform : _worldTransforms[bone.Index] * rootTransform;

				// Apply the effect and render items
				Matrix[] bones = null;
				if (bone.Skin != null)
				{
					bones = _skinInfos[bone.Skin.SkinIndex].Transforms;
				}

				foreach (var submesh in bone.Mesh.Submeshes)
				{
					list.AddJob(submesh, transform, bones);
				}
			}
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			Model = assetManager.LoadGltf(ModelPath);
		}

		private BoundingBox CalculateBoundingBox()
		{
			UpdateTransforms();

			var boundingBox = new BoundingBox();
			foreach (var bone in _model.MeshBones)
			{
				var m = bone.Skin != null ? Matrix.Identity : _worldTransforms[bone.Index];
				var bb = bone.Mesh.BoundingBox.Transform(ref m);
				boundingBox = BoundingBox.CreateMerged(boundingBox, bb);
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
		}
	}
}
