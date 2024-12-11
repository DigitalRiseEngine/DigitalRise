using AssetManagementBase;
using DigitalRise.Animation.Character;
using DigitalRise.Attributes;
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
using System.IO;

namespace DigitalRise.SceneGraph
{
	public class MeshMaterials
	{
		public string MeshName { get; set; }
		public IMaterial[] Materials { get; set; }

		public MeshMaterials Clone()
		{
			var result = new MeshMaterials
			{
				MeshName = MeshName
			};

			if (Materials != null)
			{
				result.Materials = new IMaterial[Materials.Length];
				for (var i = 0; i < Materials.Length; ++i)
				{
					result.Materials[i] = Materials[i].Clone();
				}
			}

			return result;
		}
	}

	[EditorInfo("Model")]
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

				SetModel(value, true);
			}
		}

		[Browsable(false)]
		public string ModelPath { get; set; }

		[Browsable(false)]
		public MeshMaterials[] MeshMaterials { get; set; }

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

				for (var j = 0; j < bone.Mesh.Submeshes.Count; ++j)
				{
					var submesh = bone.Mesh.Submeshes[j];
					list.AddJob(submesh, MeshMaterials[i].Materials[j], transform, bones);
				}
			}
		}

		private static string UpdateMaterialPath(string texturePath, string modelFolder)
		{
			if (!string.IsNullOrEmpty(texturePath) && !Path.IsPathRooted(texturePath))
			{
				texturePath = Path.Combine(modelFolder, texturePath);
			}

			return texturePath;
		}

		private void SetModel(DrModel model, bool setMaterialsFromModel)
		{
			_model = model;

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

				if (setMaterialsFromModel)
				{
					var meshMaterials = new List<MeshMaterials>();
					foreach (var meshBone in _model.MeshBones)
					{
						var mesh = meshBone.Mesh;
						var materials = new List<IMaterial>();
						foreach (var submesh in mesh.Submeshes)
						{
							var material = submesh.Material.Clone();

							// Make texture paths relative to the node
							if (!string.IsNullOrEmpty(ModelPath))
							{
								var asDefaultMaterial = material as DefaultMaterial;
								var modelFolder = Path.GetDirectoryName(ModelPath);
								if (asDefaultMaterial != null && !string.IsNullOrEmpty(modelFolder))
								{
									asDefaultMaterial.DiffuseTexturePath = UpdateMaterialPath(asDefaultMaterial.DiffuseTexturePath, modelFolder);
									asDefaultMaterial.SpecularTexturePath = UpdateMaterialPath(asDefaultMaterial.SpecularTexturePath, modelFolder);
									asDefaultMaterial.NormalTexturePath = UpdateMaterialPath(asDefaultMaterial.NormalTexturePath, modelFolder);
								}
							}

							materials.Add(material);
						}

						meshMaterials.Add(new MeshMaterials
						{
							MeshName = meshBone.Name,
							Materials = materials.ToArray()
						});
					}

					MeshMaterials = meshMaterials.ToArray();
				}
			}
			else
			{
				Shape = Shape.Empty;

				if (setMaterialsFromModel)
				{
					MeshMaterials = null;
				}
			}
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (MeshMaterials != null)
			{
				foreach (var meshMaterials in MeshMaterials)
				{
					foreach(var material in meshMaterials.Materials)
					{
						var hasExternalAssets = material as IHasExternalAssets;
						if (hasExternalAssets != null)
						{
							hasExternalAssets.Load(assetManager);
						}
					}
				}
			}

			var model = assetManager.LoadGltf(ModelPath);

			SetModel(model, MeshMaterials == null);
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
			SetModel(src.Model, src.MeshMaterials == null);

			if (src.MeshMaterials != null)
			{
				MeshMaterials = new MeshMaterials[src.MeshMaterials.Length];

				for (var i = 0; i < MeshMaterials.Length; ++i)
				{
					MeshMaterials[i] = src.MeshMaterials[i].Clone();
				}
			} else
			{
				MeshMaterials = null;
			}
		}

		public AnimationClip GetClip(string name) => Model.Animations[name];

		public SrtTransform GetDefaultPose(int boneIndex) => Model.Bones[boneIndex].DefaultPose;

		public void SetPose(int boneIndex, SrtTransform pose) => SetBoneLocalTransform(boneIndex, pose);
	}
}
