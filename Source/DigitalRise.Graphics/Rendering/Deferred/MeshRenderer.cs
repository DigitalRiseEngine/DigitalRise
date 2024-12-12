using DigitalRise.Data.Materials;
using DigitalRise.Data.Meshes;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DigitalRise.Rendering.Deferred
{
	public enum RenderPass
	{
		GBuffer,
		ShadowMap,
		Material
	}

	public interface IRenderList
	{
		void AddJob(Submesh mesh, IMaterial material, Matrix transform, Matrix[] bones = null);
	}


	public static class MeshRenderer
	{
		private struct RenderJob
		{
			public readonly Submesh Mesh;
			public readonly IMaterial Material;
			public readonly Matrix Transform;
			public readonly Matrix[] Bones;
			public readonly int GBufferBatchId;
			public readonly int ShadowMapBatchId;
			public readonly int MaterialBatchId;

			public RenderJob(Submesh mesh, IMaterial material, Matrix transform, Matrix[] bones)
			{
				Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
				Material = material ?? throw new ArgumentNullException(nameof(material));
				Transform = transform;
				Bones = bones;

				GBufferBatchId = material.EffectGBuffer.BatchId;
				ShadowMapBatchId = material.EffectShadowMap.BatchId;
				MaterialBatchId = material.EffectMaterial.BatchId;
			}
		}

		private class RenderList : IRenderList
		{
			public List<RenderJob> Jobs { get; } = new List<RenderJob>();

			public void AddJob(Submesh mesh, IMaterial material, Matrix transform, Matrix[] bones = null)
			{
				var job = new RenderJob(mesh, material, transform, bones);

				Jobs.Add(job);
			}

			public void Clear() => Jobs.Clear();
		}

		private class DefaultGBufferComparer : IComparer<RenderJob>
		{
			public static readonly DefaultGBufferComparer Instance = new DefaultGBufferComparer();
			public int Compare(RenderJob x, RenderJob y)
			{
				if (x.GBufferBatchId < y.GBufferBatchId)
					return -1;
				if (x.GBufferBatchId > y.GBufferBatchId)
					return +1;

				return 0;
			}
		}

		private class DefaultShadowMapComparer : IComparer<RenderJob>
		{
			public static readonly DefaultShadowMapComparer Instance = new DefaultShadowMapComparer();
			public int Compare(RenderJob x, RenderJob y)
			{
				if (x.ShadowMapBatchId < y.ShadowMapBatchId)
					return -1;
				if (x.ShadowMapBatchId > y.ShadowMapBatchId)
					return +1;

				return 0;
			}
		}

		private class DefaultMaterialComparer : IComparer<RenderJob>
		{
			public static readonly DefaultMaterialComparer Instance = new DefaultMaterialComparer();
			public int Compare(RenderJob x, RenderJob y)
			{
				if (x.MaterialBatchId < y.MaterialBatchId)
					return -1;
				if (x.MaterialBatchId > y.MaterialBatchId)
					return +1;

				return 0;
			}
		}

		private static readonly RenderList _renderList = new RenderList();

		public static void Render(RenderContext context, RenderPass renderPass, IList<SceneNode> nodes)
		{
			_renderList.Clear();

			// Batch jobs
			foreach (var node in nodes)
			{
				node.BatchJobs(_renderList);
			}

			var jobs = _renderList.Jobs;

			// Sort
			switch (renderPass)
			{
				case RenderPass.GBuffer:
					jobs.Sort(DefaultGBufferComparer.Instance);
					break;
				case RenderPass.ShadowMap:
					jobs.Sort(DefaultShadowMapComparer.Instance);
					break;
				case RenderPass.Material:
					jobs.Sort(DefaultMaterialComparer.Instance);
					break;
			}

			var device = DR.GraphicsDevice;
			BatchEffectBinding lastBinding = null;
			foreach (var job in jobs)
			{
				BatchEffectBinding effectBinding = null;
				switch (renderPass)
				{
					case RenderPass.GBuffer:
						effectBinding = job.Material.EffectGBuffer;
						break;
					case RenderPass.ShadowMap:
						effectBinding = job.Material.EffectShadowMap;
						break;
					case RenderPass.Material:
						effectBinding = job.Material.EffectMaterial;
						break;
				}

				if (lastBinding == null || effectBinding.BatchId != lastBinding.BatchId)
				{
					// Effect level params
					var viewportSize = new Vector2(device.Viewport.Width, device.Viewport.Height);
					effectBinding.View?.SetValue((Matrix)context.CameraNode.View);
					effectBinding.Projection?.SetValue((Matrix)context.CameraNode.ViewVolume.Projection);
					effectBinding.CameraNear?.SetValue(context.CameraNode.ViewVolume.Near);
					effectBinding.CameraFar?.SetValue(context.CameraNode.ViewVolume.Far);
					effectBinding.ViewportSize?.SetValue(viewportSize);
					effectBinding.SceneNodeType?.SetValue(0.0f);
					effectBinding.NormalsFittingTexture?.SetValue(Resources.NormalsFittingTexture);
					effectBinding.LightBuffer0?.SetValue(context.LightBuffer0);
					effectBinding.LightBuffer1?.SetValue(context.LightBuffer1);

					lastBinding = effectBinding;

					if (effectBinding.Techniques.Count > 1)
					{
						var technique = !string.IsNullOrEmpty(context.Technique) ? effectBinding.Techniques[context.Technique] : effectBinding.Techniques["Default"];
						if (effectBinding.CurrentTechnique != technique)
						{
							effectBinding.CurrentTechnique = technique;
						}
					}

					++context.Statistics.EffectsSwitches;
				}

				effectBinding.World?.SetValue(job.Transform);

				if (effectBinding.Bones != null && job.Bones != null)
				{
					effectBinding.Bones.SetValue(job.Bones);
				}

				switch (renderPass)
				{
					case RenderPass.GBuffer:
						job.Material.SetGBufferParameters();
						break;
					case RenderPass.ShadowMap:
						job.Material.SetShadowMapParameters();
						break;
					case RenderPass.Material:
						job.Material.SetMaterialParameters();
						break;
				}

				var mesh = job.Mesh;
				device.SetVertexBuffer(mesh.VertexBuffer);
				device.Indices = mesh.IndexBuffer;

				foreach (var pass in effectBinding.CurrentTechnique.Passes)
				{
					pass.Apply();
					device.DrawIndexedPrimitives(mesh.PrimitiveType, 
						mesh.StartVertex,
						0,
						mesh.VertexCount,
						mesh.StartIndex,
						mesh.PrimitiveCount);

					context.Statistics.VerticesDrawn += mesh.VertexCount;
					context.Statistics.PrimitivesDrawn += mesh.PrimitiveCount;
					++context.Statistics.DrawCalls;
				}
			}

			jobs.Clear();
		}
	}
}
