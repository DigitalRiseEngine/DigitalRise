using DigitalRise.ModelStorage.Binary;
using System;
using System.Collections.Generic;
using System.IO;

namespace DigitalRise.ModelStorage
{
	public partial class ModelContent
	{
		private static readonly byte[] DrmSignature = { (byte)'D', (byte)'R', (byte)'M' };

		public string BinaryPath { get; set; }
		public List<VertexBufferContent> VertexBuffers { get; set; } = new List<VertexBufferContent>();

		public IndexBufferContent IndexBuffer { get; set; }

		public BoneContent RootBone { get; set; }

		public Dictionary<string, AnimationClipContent> Animations { get; set; } = new Dictionary<string, AnimationClipContent>();

		private void SaveBinaryData(WriteContext context)
		{
			// Write vertex buffers
			for (var i = 0; i < VertexBuffers.Count; ++i)
			{
				VertexBuffers[i].SaveBinaryData(context);
			}

			// Write index buffer
			if (IndexBuffer != null)
			{
				IndexBuffer.SaveBinaryData(context);
			}

			// Bones
			if (RootBone != null)
			{
				RootBone.RecursiveProcess(bone =>
				{
					if (bone.Mesh == null)
					{
						return;
					}

					foreach (var submesh in bone.Mesh.Submeshes)
					{
						if (submesh.Skin == null)
						{
							continue;
						}

						submesh.Skin.SaveBinaryData(context);
					}
				});
			}

			// Animations
			foreach (var pair in Animations)
			{
				foreach (var channel in pair.Value.Channels)
				{
					channel.SaveBinaryData(context);
				}
			}
		}


		/// <summary>
		/// Saves model in the json format
		/// </summary>
		/// <param name="jsonPath"></param>
		/// <param name="binaryPath"></param>
		/// <param name="logger"></param>
		public void SaveJsonToFile(string jsonPath, string binaryPath, Action<string> logger = null)
		{
			// Write binary data and set buffer ids
			logger?.Invoke($"Writing '{binaryPath}'...");
			using (var stream = File.OpenWrite(binaryPath))
			using (var writer = new BinaryWriter(stream))
			{
				SaveBinaryData(new WriteContext(writer));
			}

			BinaryPath = binaryPath;

			logger?.Invoke($"Writing '{jsonPath}'...");
			JsonSerialization.SerializeToFile(jsonPath, this);
		}

		/// <summary>
		/// Saves model in the binary format
		/// </summary>
		/// <param name="path"></param>
		/// <param name="logger"></param>
		public void SaveBinaryToFile(string path, Action<string> logger = null)
		{
			logger?.Invoke($"Writing '{path}'...");
			using (var stream = File.OpenWrite(path))
			using (var writer = new BinaryWriter(stream))
			{
				// Signature
				writer.Write(DrmSignature);

				// Write binary data and set buffer ids
				var writeContext = new WriteContext(writer);
				SaveBinaryData(writeContext);

				// Json data
				var jsonData = JsonSerialization.SerializeToString(this, false);
				writer.Write(ChunkTypes.StringChunkType);
				writer.Write(jsonData);
			}
		}

		private void LoadBinaryData(ReadContext context)
		{
			// Vertex buffers
			for (var i = 0; i < VertexBuffers.Count; ++i)
			{
				var buffer = VertexBuffers[i];
				buffer.LoadBinaryData(context);
			}

			// Index buffer
			if (IndexBuffer != null)
			{
				IndexBuffer.LoadBinaryData(context);
			}

			// Bones
			if (RootBone != null)
			{
				RootBone.RecursiveProcess(bone =>
				{
					if (bone.Mesh == null)
					{
						return;
					}

					foreach (var submesh in bone.Mesh.Submeshes)
					{
						if (submesh.Skin == null)
						{
							continue;
						}

						submesh.Skin.LoadBinaryData(context);
					}
				});
			}

			// Animations
			foreach (var pair in Animations)
			{
				foreach (var channel in pair.Value.Channels)
				{
					channel.LoadBinaryData(context);
				}
			}
		}

		/// <summary>
		/// Loads model from the json format
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static ModelContent LoadJsonFromString(string s, Func<string, Stream> binaryOpener)
		{
			var result = JsonSerialization.DeserializeFromString<ModelContent>(s);

			using (var stream = binaryOpener(result.BinaryPath))
			using (var reader = new BinaryReader(stream))
			{
				result.LoadBinaryData(new ReadContext(reader));
			}

			return result;
		}

		/// <summary>
		/// Loads model from the binary format
		/// </summary>
		/// <param name="binaryReader"></param>
		/// <returns></returns>
		public static ModelContent LoadBinary(BinaryReader binaryReader)
		{
			var signature = binaryReader.ReadBytes(DrmSignature.Length);
			for (var i = 0; i < signature.Length; ++i)
			{
				if (signature[i] != DrmSignature[i])
				{
					throw new Exception($"Not a drm file.");
				}
			}

			// Create read context, which will remember binary chunks positions and skip right to the string chunk
			var readContext = new ReadContext(binaryReader);

			// Load json
			var s = binaryReader.ReadString();
			var result = JsonSerialization.DeserializeFromString<ModelContent>(s);

			// Finally read binary data
			result.LoadBinaryData(readContext);

			return result;
		}
	}
}
