using Assimp;
using Assimp.Configs;
using DigitalRise.Mathematics;
using DigitalRise.ModelStorage;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DigitalRise.ModelConverter
{
	internal class Converter
	{
		private ModelContent _model;
		private readonly List<uint> _indices = new List<uint>();
		private readonly List<SubmeshContent> _submeshes = new List<SubmeshContent>();
		private readonly List<BoneContent> _bones = new List<BoneContent>();

		static void Log(string message) => Console.WriteLine(message);

		private int FindVertexBuffer(List<VertexElementContent> vertexElements)
		{
			for (var i = 0; i < _model.VertexBuffers.Count; ++i)
			{
				var vertexBuffer = _model.VertexBuffers[i];
				if (vertexBuffer.Elements.Count != vertexElements.Count ||
					vertexBuffer.VertexStride != vertexElements.CalculateStride())
				{
					continue;
				}

				var match = true;
				for (var j = 0; j < vertexElements.Count; ++j)
				{
					if (vertexBuffer.Elements[j] != vertexElements[j])
					{
						match = false;
						break;
					}
				}

				if (match)
				{
					return i;
				}
			}

			// Create new vertex buffer
			var newVertexBuffer = new VertexBufferContent();
			for (var i = 0; i < vertexElements.Count; ++i)
			{
				newVertexBuffer.Elements.Add(vertexElements[i]);
			}

			_model.VertexBuffers.Add(newVertexBuffer);

			return _model.VertexBuffers.Count - 1;
		}

		private List<VertexElementContent> BuildVertexElement(Mesh mesh)
		{
			if (!mesh.HasVertices)
			{
				throw new Exception($"Mesh {mesh.Name} has no vertices. Such meshes aren't supported.");
			}

			var vertexElements = new List<VertexElementContent>();
			vertexElements.Add(new VertexElementContent(VertexElementUsage.Position, VertexElementFormat.Vector3));

			if (mesh.HasNormals)
			{
				vertexElements.Add(new VertexElementContent(VertexElementUsage.Normal, VertexElementFormat.Vector3));
			}

			for (var i = 0; i < mesh.VertexColorChannelCount; ++i)
			{
				vertexElements.Add(new VertexElementContent(VertexElementUsage.Color, VertexElementFormat.Color, i));
			}

			for (var i = 0; i < mesh.TextureCoordinateChannelCount; ++i)
			{
				switch (mesh.UVComponentCount[i])
				{
					case 2:
						vertexElements.Add(new VertexElementContent(VertexElementUsage.TextureCoordinate, VertexElementFormat.Vector2, i));
						break;

					default:
						throw new Exception($"UWComponentCount {mesh.UVComponentCount[i]} isn't supported.");
				}
			}

			if (mesh.HasTangentBasis)
			{

			}


			return vertexElements;
		}

		private byte[] BuildVertexBufferData(Mesh mesh)
		{
			var vertexCount = mesh.Vertices.Count;

			using (var ms = new MemoryStream())
			using (var writer = new BinaryWriter(ms))
			{
				for (var i = 0; i < vertexCount; ++i)
				{
					writer.Write(mesh.Vertices[i].ToXna());

					if (mesh.HasNormals)
					{
						writer.Write(mesh.Normals[i].ToXna());
					}

					for (var j = 0; j < mesh.VertexColorChannelCount; ++j)
					{
						writer.Write(mesh.VertexColorChannels[j][i].ToXna());
					}

					for (var j = 0; j < mesh.TextureCoordinateChannelCount; ++j)
					{
						switch (mesh.UVComponentCount[j])
						{
							case 2:
								writer.Write(mesh.TextureCoordinateChannels[j][i].ToXnaVector2());
								break;

							default:
								throw new Exception($"UWComponentCount {mesh.UVComponentCount[j]} isn't supported.");
						}
					}

					if (mesh.HasTangentBasis)
					{

					}
				}

				return ms.ToArray();
			}
		}

		private void ProcessMeshes(Scene scene)
		{
			foreach (var mesh in scene.Meshes)
			{
				if (mesh.PrimitiveType != Assimp.PrimitiveType.Triangle)
				{
					throw new Exception("Only triangle primitive type is supported");
				}

				var vertexElements = BuildVertexElement(mesh);
				var vertexBufferIndex = FindVertexBuffer(vertexElements);
				var vertexBuffer = _model.VertexBuffers[vertexBufferIndex];

				var startVertex = vertexBuffer.MemoryVertexCount;

				var data = BuildVertexBufferData(mesh);
				vertexBuffer.Write(data);

				var startIndex = _indices.Count;
				var indices = mesh.GetUnsignedIndices();
				_indices.AddRange(indices);

				var submesh = new SubmeshContent
				{
					VertexBufferIndex = vertexBufferIndex,
					StartVertex = startVertex,
					VertexCount = mesh.Vertices.Count,
					StartIndex = startIndex,
					PrimitiveCount = indices.Length / 3
				};

				_submeshes.Add(submesh);
			}

			foreach(var vertexBuffer in _model.VertexBuffers)
			{
				vertexBuffer.VertexCount = vertexBuffer.MemoryVertexCount;
			}

			_model.IndexBuffer = new IndexBufferContent(_indices);
		}

		BoneContent Convert(Node node)
		{
			var result = new BoneContent
			{
				Name = node.Name,
				DefaultPose = new SrtTransform(node.Transform.ToXna())
			};

			_bones.Add(result);

			if (node.HasMeshes)
			{
				result.Mesh = new MeshContent();

				foreach (var meshIndex in node.MeshIndices)
				{
					result.Mesh.Submeshes.Add(_submeshes[meshIndex]);
				}
			}

			if (node.Children != null)
			{
				foreach (var child in node.Children)
				{
					result.Children.Add(Convert(child));
				}
			}

			return result;
		}

		private void ProcessAnimations(Scene scene)
		{
			foreach (var animation in scene.Animations)
			{
				if (animation.HasMeshAnimations)
				{
					throw new Exception($"Mesh animations aren't supported. Animaton name='{animation.Name}'.");
				}

				var animationClip = new AnimationClipContent();
				foreach (var sourceChannel in animation.NodeAnimationChannels)
				{
					var bone = (from b in _bones where b.Name == sourceChannel.NodeName select b).FirstOrDefault();
					if (bone == null)
					{
						throw new Exception($"Unable to find bone {sourceChannel.NodeName}");
					}

					var channel = new AnimationChannelContent
					{
						BoneIndex = _bones.IndexOf(bone)
					};

					// Translations
					if (sourceChannel.HasPositionKeys)
					{
						for (var i = 0; i < sourceChannel.PositionKeyCount; ++i)
						{
							var pos = sourceChannel.PositionKeys[i];
							channel.Translations.Add(new VectorKeyframeContent(pos.Time, pos.Value.ToXna()));
						}
					}

					// Scales
					if (sourceChannel.HasScalingKeys)
					{
						for (var i = 0; i < sourceChannel.ScalingKeyCount; ++i)
						{
							var scale = sourceChannel.ScalingKeys[i];
							channel.Scales.Add(new VectorKeyframeContent(scale.Time, scale.Value.ToXna()));
						}
					}

					// Rotations
					if (sourceChannel.HasRotationKeys)
					{
						for (var i = 0; i < sourceChannel.RotationKeyCount; ++i)
						{
							var rotation = sourceChannel.RotationKeys[i];
							channel.Rotations.Add(new QuaternionKeyframeContent(rotation.Time, rotation.Value.ToXna()));
						}
					}

					animationClip.Channels.Add(channel);
				}

				_model.Animations[animation.Name] = animationClip;
			}
		}

		public void Convert(string[] args)
		{
			var inputModel = args[0];

			Log($"Input file: {inputModel}");

			_model = new ModelContent();

			using (AssimpContext importer = new AssimpContext())
			{
				importer.SetConfig(new VertexBoneWeightLimitConfig(4));

				var steps = PostProcessSteps.LimitBoneWeights | PostProcessSteps.GenerateUVCoords;
				steps |= PostProcessSteps.Triangulate;
				var scene = importer.ImportFile(inputModel, steps);

				ProcessMeshes(scene);

				_model.RootBone = Convert(scene.RootNode);

				ProcessAnimations(scene);
			}

			_model.Save(@".", Path.GetFileNameWithoutExtension(inputModel));
		}
	}
}
