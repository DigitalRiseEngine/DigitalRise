using System;
using System.Collections.Generic;
using System.IO;
using AssetManagementBase;
using DigitalRise.Animation;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Meshes;
using DigitalRise.Mathematics;
using DigitalRise.ModelStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DigitalRise.Data.Modelling
{
	internal class DrModelLoader
	{
		private struct SrtTransformOptional
		{
			public Vector3? Translation;
			public Vector3? Scale;
			public Quaternion? Rotation;
		}

		private delegate void SrtTransformSetter<T>(ref SrtTransform pose, T data);

		private AssetManager _assetManager;
		private string _assetName;
		private ModelContent _modelContent;
		private DrModel _model;
		private readonly List<VertexBuffer> _vertexBuffers = new List<VertexBuffer>();
		private IndexBuffer _indexBuffer;
		private int _skinIndex;

		private void LoadVertexBuffers()
		{
			var binaryFile = Path.ChangeExtension(_assetName, "bin");

			using (var stream = _assetManager.Open(binaryFile))
			{
				for (var i = 0; i < _modelContent.VertexBuffers.Count; ++i)
				{
					var vertexBufferContent = _modelContent.VertexBuffers[i];
					var vertexElements = new List<VertexElement>();

					var offset = 0;
					foreach (var e in vertexBufferContent.Elements)
					{
						var vertexElement = new VertexElement(offset, e.Format, e.Usage, e.UsageIndex);
						vertexElements.Add(vertexElement);

						offset += e.Format.GetSize();
					}

					var vertexDeclaration = new VertexDeclaration(vertexElements.ToArray());

					stream.Seek(vertexBufferContent.BufferOffset, SeekOrigin.Begin);
					var size = vertexBufferContent.VertexCount * vertexBufferContent.VertexStride;
					var data = new byte[size];
					var result = stream.Read(data, 0, size);

					if (result != size)
					{
						throw new Exception($"Can't read {i}th vertex buffer");
					}

					var vertexBuffer = new VertexBuffer(DR.GraphicsDevice, vertexDeclaration, vertexBufferContent.VertexCount, BufferUsage.None);
					vertexBuffer.SetData(data);

					_vertexBuffers.Add(vertexBuffer);
				}

				if (_modelContent.IndexBuffer != null)
				{
					stream.Seek(_modelContent.IndexBuffer.BufferOffset, SeekOrigin.Begin);
					var size = _modelContent.IndexBuffer.IndexCount * _modelContent.IndexBuffer.IndexType.GetSize();
					var data = new byte[size];
					var result = stream.Read(data, 0, size);

					if (result != size)
					{
						throw new Exception($"Can't read the index buffer");
					}

					_indexBuffer = new IndexBuffer(DR.GraphicsDevice, _modelContent.IndexBuffer.IndexType, _modelContent.IndexBuffer.IndexCount, BufferUsage.None);
					_indexBuffer.SetData(data);
				}
			}
		}

		private DrModelBoneDesc LoadBone(BoneContent bone)
		{
			var result = new DrModelBoneDesc
			{
				Name = bone.Name,
				SrtTransform = bone.DefaultPose
			};

			if (bone.Mesh != null)
			{
				result.Mesh = new Mesh
				{
					Name = bone.Mesh.Name
				};

				foreach (var submeshContent in bone.Mesh.Submeshes)
				{
					var submesh = new Submesh
					{
						PrimitiveType = submeshContent.PrimitiveType,
						VertexBuffer = _vertexBuffers[submeshContent.VertexBufferIndex],
						StartVertex = submeshContent.StartVertex,
						VertexCount = submeshContent.VertexCount,
						IndexBuffer = _indexBuffer,
						StartIndex = submeshContent.StartIndex,
						PrimitiveCount = submeshContent.PrimitiveCount,
					};

					if (submeshContent.Skin != null)
					{
						var joints = new List<SkinJoint>();
						foreach(var skinJointContent in submeshContent.Skin.Joints)
						{
							joints.Add(new SkinJoint(skinJointContent.BoneIndex, skinJointContent.InverseBindTransform));
						}

						submesh.Skin = new Skin(joints.ToArray());
						submesh.Skin.SkinIndex = _skinIndex;
						++_skinIndex;
					}

					var material = new DefaultMaterial
					{
						Skinning = submeshContent.Skin != null
					};
					submesh.Material = material;

					result.Mesh.Submeshes.Add(submesh);
				}
			}

			if (bone.Children != null)
			{
				foreach (var child in bone.Children)
				{
					result.Children.Add(LoadBone(child));
				}
			}

			return result;
		}


		private void LoadAnimations()
		{
			if (_modelContent.Animations == null)
			{
				return;
			}

			foreach (var animationContent in _modelContent.Animations)
			{
				var channels = new List<AnimationChannel>();
				double time = 0;
				foreach (var channelContent in animationContent.Value.Channels)
				{
					var animationData = new SortedDictionary<double, SrtTransformOptional>();

					var bone = _model.Bones[channelContent.BoneIndex];

					// First run: gather times and transforms
					if (channelContent.Translations != null)
					{
						for (var i = 0; i < channelContent.Translations.Count; ++i)
						{
							var translation = channelContent.Translations[i];

							SrtTransformOptional transform;
							animationData.TryGetValue(translation.Time, out transform);
							transform.Translation = translation.Value;
							animationData[translation.Time] = transform;
						}
					}

					if (channelContent.Scales != null)
					{
						for (var i = 0; i < channelContent.Scales.Count; ++i)
						{
							var scale = channelContent.Scales[i];

							SrtTransformOptional transform;
							animationData.TryGetValue(scale.Time, out transform);
							transform.Scale = scale.Value;
							animationData[scale.Time] = transform;
						}
					}

					if (channelContent.Rotations != null)
					{
						for (var i = 0; i < channelContent.Rotations.Count; ++i)
						{
							var rotation = channelContent.Rotations[i];

							SrtTransformOptional transform;
							animationData.TryGetValue(rotation.Time, out transform);
							transform.Rotation = rotation.Value;
							animationData[rotation.Time] = transform;
						}
					}

					// Second run: set key frames
					var keyframes = new List<AnimationChannelKeyframe>();

					var currentTransform = bone.DefaultPose;
					foreach (var pair2 in animationData)
					{
						var optionalTransform = pair2.Value;
						if (optionalTransform.Translation != null)
						{
							currentTransform.Translation = optionalTransform.Translation.Value;
						}

						if (optionalTransform.Scale != null)
						{
							currentTransform.Scale = optionalTransform.Scale.Value;
						}

						if (optionalTransform.Rotation != null)
						{
							currentTransform.Rotation = optionalTransform.Rotation.Value;
						}

						keyframes.Add(new AnimationChannelKeyframe(TimeSpan.FromMilliseconds(pair2.Key), currentTransform));

						if (pair2.Key > time)
						{
							time = pair2.Key;
						}
					}

					var animationChannel = new AnimationChannel(bone.Index, keyframes.ToArray())
					{
						TranslationMode = InterpolationMode.Linear,
						RotationMode = InterpolationMode.Linear,
						ScaleMode = InterpolationMode.Linear
					};

					channels.Add(animationChannel);
				}

				var animation = new AnimationClip(animationContent.Key, TimeSpan.FromMilliseconds(time), channels.ToArray());
				var id = animation.Name ?? "(default)";
				_model.Animations[id] = animation;
			}
		}

		public DrModel Load(AssetManager manager, string assetName)
		{
			_vertexBuffers.Clear();
			_skinIndex = 0;

			_assetManager = manager;
			_assetName = assetName;

			_modelContent = JsonSerialization.DeserializeFromString<ModelContent>(manager.ReadAsString(assetName));

			LoadVertexBuffers();

			var rootBoneDesc = LoadBone(_modelContent.RootBone);

			_model = DrModelBuilder.Create(rootBoneDesc);

			LoadAnimations();

			return _model;
		}
	}
}