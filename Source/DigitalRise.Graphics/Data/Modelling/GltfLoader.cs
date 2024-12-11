using System;
using System.Collections.Generic;
using System.IO;
using AssetManagementBase;
using DigitalRise.Animation.Character;
using DigitalRise.Data.Meshes;
using DigitalRise.Mathematics;
using DigitalRise.ModelStorage;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise.Data.Modelling
{
	internal class GltfLoader
	{
		private delegate void SrtTransformSetter<T>(ref SrtTransform pose, T data);

		private AssetManager _assetManager;
		private string _assetName;
		private ModelContent _modelContent;
		private readonly List<VertexBuffer> _vertexBuffers = new List<VertexBuffer>();
		private IndexBuffer _indexBuffer;
		private readonly List<Skin> _skins = new List<Skin>();

		private void LoadVertexBuffers()
		{
			var binaryFile = Path.ChangeExtension(_assetName, "bin");

			using (var stream = _assetManager.Open(binaryFile))
			{
				for(var i = 0; i < _modelContent.VertexBuffers.Count; ++i)
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


/*		private void LoadAnimationTransforms<T>(SrtTransform defaultSrtTransform, SortedDictionary<float, SrtTransform> poses, SrtTransformSetter<T> poseSetter, float[] times, AnimationSampler sampler)
		{
			var data = GetAccessorAs<T>(sampler.Output);
			if (times.Length != data.Length)
			{
				throw new NotSupportedException("Translation length is different from times length");
			}

			for (var i = 0; i < times.Length; ++i)
			{
				var time = times[i];

				SrtTransform pose;
				if (!poses.TryGetValue(time, out pose))
				{
					pose = defaultSrtTransform;
				}

				poseSetter(ref pose, data[i]);

				poses[time] = pose;
			}
		}

		private Skin LoadSkin(int skinId)
		{
			var gltfSkin = _gltf.Skins[skinId];
			if (gltfSkin.Joints.Length > DRConstants.MaximumBones)
			{
				throw new Exception($"Skin {gltfSkin.Name} has {gltfSkin.Joints.Length} bones which exceeds maximum {DRConstants.MaximumBones}");
			}

			var transforms = GetAccessorAs<Matrix>(gltfSkin.InverseBindMatrices.Value);
			if (gltfSkin.Joints.Length != transforms.Length)
			{
				throw new Exception($"Skin {gltfSkin.Name} inconsistency. Joints amount: {gltfSkin.Joints.Length}, Inverse bind matrices amount: {transforms.Length}");
			}

			var joints = new List<SkinJoint>();
			for (var i = 0; i < gltfSkin.Joints.Length; ++i)
			{
				var jointIndex = gltfSkin.Joints[i];
				joints.Add(new SkinJoint(jointIndex, transforms[i]));
			}
			var result = new Skin(joints.ToArray());

			Debug.WriteLine($"Skin {gltfSkin.Name} has {gltfSkin.Joints.Length} joints");

			return result;
		}*/

		private static void RecursiveProcessNode(BoneContent root, Action<BoneContent> processor)
		{
			processor(root);

			if (root.Children != null)
			{
				foreach(var child in root.Children)
				{
					RecursiveProcessNode(child, processor);
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

					result.Mesh.Submeshes.Add(submesh);
				}
			}

			if (bone.Children != null)
			{
				foreach(var child in bone.Children)
				{
					result.Children.Add(LoadBone(child));
				}
			}

			return result;
		}

		public DrModel Load(AssetManager manager, string assetName)
		{
			_vertexBuffers.Clear();
			_skins.Clear();

			_assetManager = manager;
			_assetName = assetName;

			_modelContent = JsonSerialization.DeserializeFromString<ModelContent>(manager.ReadAsString(assetName));

			LoadVertexBuffers();

			var rootBoneDesc = LoadBone(_modelContent.RootBone);

			var model = DrModelBuilder.Create(rootBoneDesc, _skins);
/*			if (_gltf.Animations != null)
			{
				foreach (var gltfAnimation in _gltf.Animations)
				{
					var channelsDict = new Dictionary<int, List<PathInfo>>();
					foreach (var channel in gltfAnimation.Channels)
					{
						if (!channelsDict.TryGetValue(channel.Target.Node.Value, out List<PathInfo> targets))
						{
							targets = new List<PathInfo>();
							channelsDict[channel.Target.Node.Value] = targets;
						}

						targets.Add(new PathInfo(channel.Sampler, channel.Target.Path));
					}

					var channels = new List<AnimationChannel>();
					float time = 0;
					foreach (var pair in channelsDict)
					{
						var bone = model.Bones[pair.Key];
						var animationData = new SortedDictionary<float, SrtTransform>();

						var translationMode = InterpolationMode.None;
						var rotationMode = InterpolationMode.None;
						var scaleMode = InterpolationMode.None;
						foreach (var pathInfo in pair.Value)
						{
							var sampler = gltfAnimation.Samplers[pathInfo.Sampler];
							var times = GetAccessorAs<float>(sampler.Input);

							switch (pathInfo.Path)
							{
								case PathEnum.translation:
									LoadAnimationTransforms(bone.DefaultPose, animationData,
										(ref SrtTransform p, Vector3 d) => p.Translation = d,
										times, sampler);
									translationMode = sampler.Interpolation.ToInterpolationMode();
									break;
								case PathEnum.rotation:
									LoadAnimationTransforms(bone.DefaultPose, animationData,
										(ref SrtTransform p, Quaternion d) => p.Rotation = d,
										times, sampler);
									rotationMode = sampler.Interpolation.ToInterpolationMode();
									break;
								case PathEnum.scale:
									LoadAnimationTransforms(bone.DefaultPose, animationData,
										(ref SrtTransform p, Vector3 d) => p.Scale = d,
										times, sampler);
									scaleMode = sampler.Interpolation.ToInterpolationMode();
									break;
								case PathEnum.weights:
									break;
							}
						}

						var keyframes = new List<AnimationChannelKeyframe>();
						foreach (var pair2 in animationData)
						{
							keyframes.Add(new AnimationChannelKeyframe(TimeSpan.FromSeconds(pair2.Key), pair2.Value));

							if (pair2.Key > time)
							{
								time = pair2.Key;
							}
						}

						var animationChannel = new AnimationChannel(bone.Index, keyframes.ToArray())
						{
							TranslationMode = translationMode,
							RotationMode = rotationMode,
							ScaleMode = scaleMode
						};

						channels.Add(animationChannel);
					}

					var animation = new AnimationClip(gltfAnimation.Name, TimeSpan.FromSeconds(time), channels.ToArray());
					var id = animation.Name ?? "(default)";
					model.Animations[id] = animation;
				}
			}*/

			return model;
		}
	}
}