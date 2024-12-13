using DigitalRise.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public partial class ModelContent
	{
		private static readonly byte[] DrmSignature = { (byte)'D', (byte)'R', (byte)'M' };
		private const int DrmVersion = 2;

		public List<VertexBufferContent> VertexBuffers { get; } = new List<VertexBufferContent>();

		public IndexBufferContent IndexBuffer { get; set; }

		public BoneContent RootBone { get; set; }

		public Dictionary<string, AnimationClipContent> Animations { get; } = new Dictionary<string, AnimationClipContent>();


		/// <summary>
		/// Saves model in the json format
		/// </summary>
		/// <param name="path"></param>
		public void SaveJsonToFile(string path)
		{
			path = Path.ChangeExtension(path, "jdrm");
			JsonSerialization.SerializeToFile(path, this);
		}

		/// <summary>
		/// Loads model from the json string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static ModelContent LoadJsonFromString(string s)
		{
			return JsonSerialization.DeserializeFromString<ModelContent>(s);
		}

		/// <summary>
		/// Loads model from the json format
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static ModelContent LoadJsonFromFile(string path)
		{
			var s = File.ReadAllText(path);

			return JsonSerialization.DeserializeFromString<ModelContent>(s);
		}

		/// <summary>
		/// Saves model in the binary format
		/// </summary>
		/// <param name="bw"></param>
		public void SaveBinary(BinaryWriter bw)
		{
			bw.Write(DrmSignature);
			bw.Write(DrmVersion);

			bw.WriteCollection(VertexBuffers);
			bw.WriteIfNotNull(IndexBuffer);
			bw.WriteIfNotNull(RootBone);
			bw.WriteCollection(Animations.Values);
		}

		public void SaveBinaryToFile(string path)
		{
			path = Path.ChangeExtension(path, "drm");

			using (var stream = File.OpenWrite(path))
			using (var writer = new BinaryWriter(stream))
			{
				SaveBinary(writer);
			}
		}

		public static ModelContent LoadBinary(BinaryReader reader)
		{
			var result = new ModelContent();

			var signature = reader.ReadBytes(3);
			if (signature[0] != DrmSignature[0] || signature[1] != DrmSignature[1] || signature[2] != DrmSignature[2])
			{
				throw new Exception("Wrong signature");
			}

			var version = reader.ReadInt32();
			if (version != DrmVersion)
			{
				throw new Exception($"Wrong version. Reader version={DrmVersion}, file version={version}.");
			}

			result.VertexBuffers.AddRange(reader.ReadCollection<VertexBufferContent>());
			result.IndexBuffer = reader.ReadIfNotNull<IndexBufferContent>();
			result.RootBone = reader.ReadIfNotNull<BoneContent>();

			var animations = reader.ReadCollection<AnimationClipContent>();
			foreach (var animation in animations)
			{
				result.Animations[animation.Name] = animation;
			}

			return result;
		}
	}
}
