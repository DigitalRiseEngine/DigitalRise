// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using DigitalRise.PostProcessing.Processing;
using DigitalRise.Misc.TextureAtlas;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using DigitalRise.Data.Billboards;


namespace DigitalRise.Rendering.Billboards
{
	partial class BillboardRenderer
	{
		//--------------------------------------------------------------
		#region Nested Types
		//--------------------------------------------------------------

		/// <summary>
		/// Stores the index of a particle and its distance for per-particle depth sorting.
		/// </summary>
		private struct ParticleIndex
		{
			public float Distance;
			public int Index;
			public Vector3 Position;
		}


		/// <summary>
		/// Sorts particles back-to-front.
		/// </summary>
		private sealed class ParticleIndexComparer : IComparer<ParticleIndex>
		{
			public readonly static ParticleIndexComparer Instance = new ParticleIndexComparer();

			public int Compare(ParticleIndex indexA, ParticleIndex indexB)
			{
				if (indexA.Distance > indexB.Distance)
					return -1;

				if (indexA.Distance < indexB.Distance)
					return +1;

				return 0;
			}
		}
		#endregion


		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		/// <summary>
		/// The blend state for rendering billboards into the off-screen buffer.
		/// </summary>
		private static readonly BlendState BlendStateOffscreen = new BlendState
		{
			Name = "BillboardRenderer.BlendStateOffscreen",
			ColorBlendFunction = BlendFunction.Add,
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.InverseSourceAlpha,

			// Separate alpha blend function (requires HiDef profile!).
			AlphaBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.Zero,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
		};


		// ----- Not used: Combine pass is done in shader without alpha blending.
		///// <summary>
		///// The blend state for upsampling the off-screen buffer and combining the result with the
		///// current render target.
		///// </summary>
		//private static readonly BlendState BlendStateCombine = new BlendState
		//{
		//  Name = "BillboardRenderer.BlendStateCombine",
		//  ColorBlendFunction = BlendFunction.Add,
		//  ColorSourceBlend = Blend.One,
		//  ColorDestinationBlend = Blend.SourceAlpha,

		//  // Separate alpha blend function (requires HiDef profile!).
		//  AlphaBlendFunction = BlendFunction.Add,
		//  AlphaSourceBlend = Blend.Zero,
		//  AlphaDestinationBlend = Blend.One,
		//};

		private bool _hiDef;

		private IBillboardBatch _billboardBatch;

		// A white 1x1 texture that is used if no other texture is specified.
		// (Can be used for debugging.)
		private PackedTexture _debugTexture;

		// Billboard.fx (HiDef profile)
		private RenderTarget2D _offscreenBuffer;
		private readonly UpsampleFilter _upsampleFilter = new UpsampleFilter();

		// true, if rendering billboards or particles. (The value is set in 
		// BeginBillboards() and reset in EndBillboards().)
		private bool _billboardMode;

		// For depth-sorting of particles, created on demand.
		private List<ParticleIndex> _particleIndices;

		private BillboardEffectWrapper _hiDefEffect;
		private BasicEffect _reachEffect;
		#endregion

		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void InitializeBillboards()
		{
			_debugTexture = new PackedTexture(Resources.DefaultTexture2DWhite);

			var graphicsDevice = DR.GraphicsDevice;
			_hiDef = (graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef);

			if (_hiDef)
			{
				// ----- HiDef profile
				_hiDefEffect = new BillboardEffectWrapper();
				_billboardBatch = new BillboardBatchHiDef(graphicsDevice, BufferSize);
			}
			else
			{
				// ----- Reach profile
				_reachEffect = new BasicEffect(graphicsDevice)
				{
					FogEnabled = false,
					LightingEnabled = false,
					TextureEnabled = true,
					VertexColorEnabled = true,
					World = Matrix.Identity,
				};

				_billboardBatch = new BillboardBatchReach(graphicsDevice, BufferSize);
			}
		}


		private void DisposeBillboards()
		{
			_billboardBatch.Dispose();

			// Note: Do not expose effect in HiDef profile. The effect is managed by
			// the ContentManager and may be shared.
			if (!_hiDef)
				_reachEffect.Dispose();
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		// Prepare effect for rendering billboards.
		private void PrepareBillboards(RenderContext context)
		{
			var graphicsDevice = DR.GraphicsDevice;
			var cameraNode = context.CameraNode;
			if (_hiDef)
			{
				// ----- HiDef profile
				var effect = _hiDefEffect;
				effect.View.SetValue((Matrix)cameraNode.View);
				effect.ViewInverse.SetValue((Matrix)cameraNode.ViewInverse);
				effect.ViewProjection.SetValue((Matrix)(cameraNode.Camera.Projection * cameraNode.View));
				effect.Projection.SetValue(cameraNode.Camera.Projection);
				effect.CameraPosition.SetValue((Vector3)cameraNode.PoseWorld.Position);
				effect.CameraNear.SetValue(cameraNode.Camera.Projection.Near);
				effect.CameraFar.SetValue(cameraNode.Camera.Projection.Far);

				// Select effect technique.
				if (EnableOffscreenRendering || EnableSoftParticles)
					effect.CurrentTechnique = graphicsDevice.IsCurrentRenderTargetHdr() ? effect.TechniqueSoftLinear : effect.TechniqueSoftGamma;
				else
					effect.CurrentTechnique = graphicsDevice.IsCurrentRenderTargetHdr() ? effect.TechniqueHardLinear : effect.TechniqueHardGamma;

				if (!EnableOffscreenRendering && !EnableSoftParticles)
				{
					// Render at full resolution.
					effect.ViewportSize.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
				}
				else if (!EnableOffscreenRendering && EnableSoftParticles)
				{
					// Render at full resolution with depth test in pixel shader.
					context.ThrowIfGBuffer0Missing();
					effect.DepthBuffer.SetValue(context.GBuffer0);
					effect.ViewportSize.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
				}
				else if (EnableOffscreenRendering)
				{
					// Render at half resolution into off-screen buffer.
					var depthBufferHalf = context.DepthBufferHalf;
					effect.DepthBuffer.SetValue(depthBufferHalf);
					effect.ViewportSize.SetValue(new Vector2(depthBufferHalf.Width, depthBufferHalf.Height));
				}
			}
			else
			{
				// ----- Reach profile
				var basicEffect = _reachEffect;
				basicEffect.View = (Matrix)cameraNode.View;
				basicEffect.Projection = cameraNode.Camera.Projection;
			}
		}


		// Sets the render states for rendering billboards.
		private void BeginBillboards(RenderContext context)
		{
			if (_billboardMode)
				return;

			_billboardMode = true;

			var graphicsDevice = DR.GraphicsDevice;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;

			if (_hiDef)
			{
				// ----- HiDef profile
				if (!EnableOffscreenRendering && !EnableSoftParticles)
				{
					// Render at full resolution.
					graphicsDevice.BlendState = BlendState.AlphaBlend;
				}
				else if (!EnableOffscreenRendering && EnableSoftParticles)
				{
					// Render at full resolution with depth test in pixel shader.
					graphicsDevice.BlendState = BlendState.AlphaBlend;
					graphicsDevice.DepthStencilState = DepthStencilState.None;
				}
				else if (EnableOffscreenRendering)
				{
					// Render at half resolution into off-screen buffer.
					graphicsDevice.BlendState = BlendStateOffscreen;
					graphicsDevice.DepthStencilState = DepthStencilState.None;

					var sceneRenderTarget = graphicsDevice.GetCurrentRenderTarget();

					var depthBufferHalf = context.DepthBufferHalf;
					_offscreenBuffer = context.RenderTargetPool.Obtain2D(
					  new RenderTargetFormat(depthBufferHalf.Width, depthBufferHalf.Height, false, sceneRenderTarget.Format, DepthFormat.None));
					graphicsDevice.SetRenderTarget(_offscreenBuffer);

					graphicsDevice.Clear(Color.Black);
				}
			}
			else
			{
				// ----- Reach profile
				graphicsDevice.BlendState = BlendState.AlphaBlend;
			}
		}


		private void EndBillboards(RenderContext context)
		{
			if (!_billboardMode)
				return;

			_billboardMode = false;

			// Reset texture to prevent "memory leak".
			SetTexture(null);

			if (EnableOffscreenRendering)
			{
				// ----- Combine off-screen buffer with scene.
				var graphicsDevice = DR.GraphicsDevice;
				graphicsDevice.BlendState = BlendState.Opaque;

				// The previous scene render target is bound as texture.
				// --> Switch scene render targets!
				var sceneTexture = (RenderTarget2D)graphicsDevice.GetCurrentRenderTarget();
				var renderTargetPool = context.RenderTargetPool;
				var renderTarget = renderTargetPool.Obtain2D(new RenderTargetFormat(sceneTexture));
				context.SourceTexture = _offscreenBuffer;
				context.SceneTexture = sceneTexture;

				graphicsDevice.SetRenderTarget(renderTarget);

				_upsampleFilter.Mode = UpsamplingMode;
				_upsampleFilter.DepthThreshold = DepthThreshold;
				_upsampleFilter.RebuildZBuffer = true;

				_upsampleFilter.Process(context);

				context.SourceTexture = null;
				context.SceneTexture = null;
				renderTargetPool.Recycle(_offscreenBuffer);
				renderTargetPool.Recycle(sceneTexture);
				_offscreenBuffer = null;
			}
		}


		private void DrawBillboards(int index, int endIndex, RenderContext context)
		{
			// Update and apply effect.
			SetTexture(index);

			Effect _billboardEffect = _hiDef ? _hiDefEffect.Effect : _reachEffect;
			_billboardEffect.CurrentTechnique.Passes[0].Apply();

			_billboardBatch.Begin(context.CameraNode);
			++context.Statistics.EffectsSwitches;

			while (index < endIndex)
			{
				var node = _jobs[index].Node;

				/*				var particleSystemData = jobs[index].ParticleSystemData;
								if (particleSystemData != null)
									Draw((ParticleSystemNode)node, particleSystemData);
								else
									Draw((BillboardNode)node);*/

				Draw((BillboardNode)node, ref context.Statistics);

				index++;
			}

			_billboardBatch.End(ref context.Statistics);
		}


		private void SetTexture(int index)
		{
			PackedTexture texture;

			/*			var particleSystemData = _jobs[index].ParticleSystemData;
						if (particleSystemData != null)
						{
							// Particles
							texture = particleSystemData.Texture;
						}
						else*/
			{
				// Billboard
				var billboardNode = (BillboardNode)_jobs[index].Node;
				var billboard = (ImageBillboard)billboardNode.Billboard;
				texture = billboard.Texture;
			}

			// Fallback
			if (texture == null)
				texture = _debugTexture;

			ResetTextureId(texture);

			SetTexture(texture.TextureAtlas);
		}


		private void SetTexture(Texture2D texture)
		{
			if (_hiDef)
			{
				// ----- HiDef profile
				_hiDefEffect.Texture.SetValue(texture);
			}
			else
			{
				// ----- Reach profile
				var basicEffect = _reachEffect;
				basicEffect.Texture = texture;
			}
		}


		private void Draw(BillboardNode node, ref RenderStatistics statistics)
		{
			var billboard = (ImageBillboard)node.Billboard;
			var data = new BillboardArgs
			{
				Position = node.PoseWorld.Position,
				Normal = (billboard.Orientation.Normal == BillboardNormal.ViewPlaneAligned) ? _defaultNormal : node.Normal,
				Axis = node.Axis,
				Orientation = billboard.Orientation,
				Size = node.ScaleWorld.Y * billboard.Size, // Assume uniform scale for size.
				Softness = Numeric.IsNaN(billboard.Softness) ? -1 : billboard.Softness,
				Color = node.Color * billboard.Color,
				Alpha = node.Alpha * billboard.Alpha,
				ReferenceAlpha = billboard.AlphaTest,
				AnimationTime = (Numeric.IsNaN(node.AnimationTime)) ? billboard.AnimationTime : node.AnimationTime,
				BlendMode = billboard.BlendMode,
			};

			var texture = billboard.Texture ?? _debugTexture;
			_billboardBatch.DrawBillboard(ref data, texture, ref statistics);
		}


		/*		private void Draw(ParticleSystemNode node, ParticleSystemData particleSystemData)
				{
					// Scale and pose.
					Vector3 scale = Vector3.One;
					Pose pose = Pose.Identity;
					bool requiresTransformation = (particleSystemData.ReferenceFrame == ParticleReferenceFrame.Local);
					if (requiresTransformation)
					{
						scale = node.ScaleWorld;
						pose = node.PoseWorld * particleSystemData.Pose;
					}

					// Tint color and alpha.
					Vector3 color = node.Color;
					float alpha = node.Alpha;
					float angleOffset = node.AngleOffset;

					if (particleSystemData.IsRibbon)
					{
						if (particleSystemData.AxisParameter == null)
						{
							// Ribbons with automatic axis.
							DrawParticleRibbonsAuto(particleSystemData, requiresTransformation, ref scale, ref pose, ref color, alpha);
						}
						else
						{
							// Ribbons with fixed axis.
							DrawParticleRibbonsFixed(particleSystemData, requiresTransformation, ref scale, ref pose, ref color, alpha);
						}
					}
					else if (particleSystemData.IsDepthSorted)
					{
						// Particles sorted by depth.
						DrawParticlesBackToFront(particleSystemData, requiresTransformation, ref scale, ref pose, ref color, alpha, angleOffset);
					}
					else
					{
						// Particles sorted by age.
						DrawParticlesOldToNew(particleSystemData, requiresTransformation, ref scale, ref pose, ref color, alpha, angleOffset);
					}
				}


				#region ----- Particles -----

				private void DrawParticlesOldToNew(ParticleSystemData particleSystemData, bool requiresTransformation, ref Vector3 scale, ref Pose pose, ref Vector3 color, float alpha, float angleOffset)
				{
					var b = new BillboardArgs
					{
						Orientation = particleSystemData.BillboardOrientation,
						Softness = particleSystemData.Softness,
						ReferenceAlpha = particleSystemData.AlphaTest,
					};

					int numberOfParticles = particleSystemData.Particles.Count;
					var particles = particleSystemData.Particles.Array;
					bool isViewPlaneAligned = (particleSystemData.BillboardOrientation.Normal == BillboardNormal.ViewPlaneAligned);
					bool isAxisInViewSpace = particleSystemData.BillboardOrientation.IsAxisInViewSpace;

					for (int i = 0; i < numberOfParticles; i++)
					{
						if (particles[i].IsAlive) // Skip dead particles.
						{
							if (requiresTransformation)
							{
								b.Position = pose.ToWorldPosition(particles[i].Position * scale);
								b.Normal = isViewPlaneAligned ? _defaultNormal : pose.ToWorldDirection(particles[i].Normal);
								b.Axis = isAxisInViewSpace ? particles[i].Axis : pose.ToWorldDirection(particles[i].Axis);
								b.Size = particles[i].Size * scale.Y; // Assume uniform scale for size.
							}
							else
							{
								b.Position = particles[i].Position;
								b.Normal = isViewPlaneAligned ? _defaultNormal : particles[i].Normal;
								b.Axis = particles[i].Axis;
								b.Size = particles[i].Size;
							}

							b.Angle = particles[i].Angle + angleOffset;
							b.Color = particles[i].Color * color;
							b.Alpha = particles[i].Alpha * alpha;
							b.AnimationTime = particles[i].AnimationTime;
							b.BlendMode = particles[i].BlendMode;

							var texture = particleSystemData.Texture ?? _debugTexture;
							_billboardBatch.DrawBillboard(ref b, texture);
						}
					}
				}


				private void DrawParticlesBackToFront(ParticleSystemData particleSystemData, bool requiresTransformation, ref Vector3 scale, ref Pose pose, ref Vector3 color, float alpha, float angleOffset)
				{
					var b = new BillboardArgs
					{
						Orientation = particleSystemData.BillboardOrientation,
						Softness = particleSystemData.Softness,
						ReferenceAlpha = particleSystemData.AlphaTest,
					};

					int numberOfParticles = particleSystemData.Particles.Count;
					var particles = particleSystemData.Particles.Array;

					if (_particleIndices == null)
					{
						_particleIndices = new ArrayList<ParticleIndex>(numberOfParticles);
					}
					else
					{
						_particleIndices.Clear();
						_particleIndices.EnsureCapacity(numberOfParticles);
					}

					// Use linear distance for viewpoint-oriented and world-oriented billboards.
					bool useLinearDistance = (particleSystemData.BillboardOrientation.Normal != BillboardNormal.ViewPlaneAligned);

					// Compute positions and distance to camera.
					for (int i = 0; i < numberOfParticles; i++)
					{
						if (particles[i].IsAlive) // Skip dead particles.
						{
							var particleIndex = new ParticleIndex();
							particleIndex.Index = i;
							if (requiresTransformation)
								particleIndex.Position = pose.ToWorldPosition(particles[i].Position * scale);
							else
								particleIndex.Position = particles[i].Position;

							// Planar distance: Project vector onto look direction.
							Vector3 cameraToParticle = particleIndex.Position - _cameraPose.Position;
							particleIndex.Distance = Vector3.Dot(cameraToParticle, _cameraForward);
							if (useLinearDistance)
								particleIndex.Distance = cameraToParticle.Length() * Math.Sign(particleIndex.Distance);

							_particleIndices.Add(ref particleIndex);
						}
					}

					// Sort particles back-to-front.
					_particleIndices.Sort(ParticleIndexComparer.Instance);

					bool isViewPlaneAligned = (particleSystemData.BillboardOrientation.Normal == BillboardNormal.ViewPlaneAligned);
					bool isAxisInViewSpace = particleSystemData.BillboardOrientation.IsAxisInViewSpace;

					// Draw sorted particles.
					var indices = _particleIndices.Array;
					numberOfParticles = _particleIndices.Count; // Dead particles have been removed.
					for (int i = 0; i < numberOfParticles; i++)
					{
						int index = indices[i].Index;
						b.Position = indices[i].Position;
						if (requiresTransformation)
						{
							b.Normal = isViewPlaneAligned ? _defaultNormal : pose.ToWorldDirection(particles[index].Normal);
							b.Axis = isAxisInViewSpace ? particles[index].Axis : pose.ToWorldDirection(particles[index].Axis);
							b.Size = particles[index].Size * scale.Y; // Assume uniform scale for size.
						}
						else
						{
							b.Normal = isViewPlaneAligned ? _defaultNormal : particles[index].Normal;
							b.Axis = particles[index].Axis;
							b.Size = particles[index].Size;
						}

						b.Angle = particles[index].Angle + angleOffset;
						b.Color = particles[index].Color * color;
						b.Alpha = particles[index].Alpha * alpha;
						b.AnimationTime = particles[index].AnimationTime;
						b.BlendMode = particles[index].BlendMode;

						var texture = particleSystemData.Texture ?? _debugTexture;
						_billboardBatch.DrawBillboard(ref b, texture);
					}
				}
				#endregion


				#region ----- Ribbons -----

				// Particle ribbons:
				// Particles can be rendered as ribbons (a.k.a. trails, lines). Subsequent living 
				// particles are connected using rectangles.
				//   +--------------+--------------+
				//   |              |              |
				//   p0             p1             p2
				//   |              |              |
				//   +--------------+--------------+
				// At least two living particles are required to create a ribbon. Dead particles 
				// ("NormalizedAge" ≥ 1) can be used as delimiters to terminate one ribbon and 
				// start the next ribbon.
				// 
				// p0 and p1 can have different colors and alpha values to create color gradients 
				// or a ribbon that fades in/out.

				private void DrawParticleRibbonsFixed(ParticleSystemData particleSystemData, bool requiresTransformation, ref Vector3 scale, ref Pose pose, ref Vector3 color, float alpha)
				{
					// At least two particles are required to create a ribbon.
					int numberOfParticles = particleSystemData.Particles.Count;
					if (numberOfParticles < 2)
						return;

					var particles = particleSystemData.Particles.Array;
					bool isAxisInViewSpace = particleSystemData.BillboardOrientation.IsAxisInViewSpace;
					int index = 0;
					do
					{
						// ----- Skip dead particles.
						while (index < numberOfParticles && !particles[index].IsAlive)
							index++;

						// ----- Start of new ribbon.
						int endIndex = index + 1;
						while (endIndex < numberOfParticles && particles[endIndex].IsAlive)
							endIndex++;

						int numberOfSegments = endIndex - index - 1;

						var p0 = new RibbonArgs
						{
							// Uniform parameters
							Softness = particleSystemData.Softness,
							ReferenceAlpha = particleSystemData.AlphaTest
						};

						var p1 = new RibbonArgs
						{
							// Uniform parameters
							Softness = particleSystemData.Softness,
							ReferenceAlpha = particleSystemData.AlphaTest
						};

						p0.Axis = particles[index].Axis;
						if (requiresTransformation)
						{
							p0.Position = pose.ToWorldPosition(particles[index].Position * scale);
							if (!isAxisInViewSpace)
								p0.Axis = pose.ToWorldDirection(p0.Axis);

							p0.Size = particles[index].Size.Y * scale.Y;
						}
						else
						{
							p0.Position = particles[index].Position;
							p0.Size = particles[index].Size.Y;
						}

						p0.Color = particles[index].Color * color;
						p0.Alpha = particles[index].Alpha * alpha;
						p0.AnimationTime = particles[index].AnimationTime;
						p0.BlendMode = particles[index].BlendMode;
						p0.TextureCoordinateU = 0;

						index++;
						while (index < endIndex)
						{
							p1.Axis = particles[index].Axis;
							if (requiresTransformation)
							{
								p1.Position = pose.ToWorldPosition(particles[index].Position * scale);
								if (!isAxisInViewSpace)
									p1.Axis = pose.ToWorldDirection(p1.Axis);

								p1.Size = particles[index].Size.Y * scale.Y;
							}
							else
							{
								p1.Position = particles[index].Position;
								p1.Size = particles[index].Size.Y;
							}

							p1.Color = particles[index].Color * color;
							p1.Alpha = particles[index].Alpha * alpha;
							p1.AnimationTime = particles[index].AnimationTime;
							p1.BlendMode = particles[index].BlendMode;
							p1.TextureCoordinateU = GetTextureCoordinateU1(index - 1, numberOfSegments, particleSystemData.TextureTiling);

							// Draw ribbon segment.
							var texture = particleSystemData.Texture ?? _debugTexture;
							_billboardBatch.DrawRibbon(ref p0, ref p1, texture);

							p0 = p1;
							p0.TextureCoordinateU = GetTextureCoordinateU0(index, numberOfSegments, particleSystemData.TextureTiling);
							index++;
						}
					} while (index < numberOfParticles);
				}


				private void DrawParticleRibbonsAuto(ParticleSystemData particleSystemData, bool requiresTransformation, ref Vector3 scale, ref Pose pose, ref Vector3 color, float alpha)
				{
					// At least two particles are required to create a ribbon.
					int numberOfParticles = particleSystemData.Particles.Count;
					if (numberOfParticles < 2)
						return;

					// The up axis is not defined and needs to be derived automatically:
					// - Compute tangents along the ribbon curve.
					// - Build cross-products of normal and tangent vectors.

					// Is normal uniform across all particles?
					Vector3? uniformNormal;
					switch (particleSystemData.BillboardOrientation.Normal)
					{
						case BillboardNormal.ViewPlaneAligned:
							uniformNormal = _defaultNormal;
							break;

						case BillboardNormal.ViewpointOriented:
							var v = _cameraPose.Position - pose.Position;
							if (!v.TryNormalize())
								v = _defaultNormal;
							uniformNormal = v;
							break;

						default:
							var normalParameter = particleSystemData.NormalParameter;
							if (normalParameter == null)
							{
								uniformNormal = _defaultNormal;
							}
							else if (normalParameter.IsUniform)
							{
								uniformNormal = normalParameter.DefaultValue;
								if (requiresTransformation)
									uniformNormal = pose.ToWorldDirection(uniformNormal.Value);
							}
							else
							{
								// Normal is set in particle data.
								uniformNormal = null;
							}
							break;
					}

					var texture = particleSystemData.Texture ?? _debugTexture;
					var particles = particleSystemData.Particles.Array;
					int index = 0;
					do
					{
						// ----- Skip dead particles.
						while (index < numberOfParticles && !particles[index].IsAlive)
							index++;

						// ----- Start of new ribbon.
						int endIndex = index + 1;
						while (endIndex < numberOfParticles && particles[endIndex].IsAlive)
							endIndex++;

						int numberOfSegments = endIndex - index - 1;

						var p0 = new RibbonArgs
						{
							// Uniform parameters
							Softness = particleSystemData.Softness,
							ReferenceAlpha = particleSystemData.AlphaTest
						};

						var p1 = new RibbonArgs
						{
							// Uniform parameters
							Softness = particleSystemData.Softness,
							ReferenceAlpha = particleSystemData.AlphaTest
						};

						// Compute axes and render ribbon.
						// First particle.
						if (requiresTransformation)
						{
							p0.Position = pose.ToWorldPosition(particles[index].Position * scale);
							p0.Size = particles[index].Size.Y * scale.Y;
						}
						else
						{
							p0.Position = particles[index].Position;
							p0.Size = particles[index].Size.Y;
						}

						p0.Color = particles[index].Color * color;
						p0.Alpha = particles[index].Alpha * alpha;
						p0.AnimationTime = particles[index].AnimationTime;
						p0.BlendMode = particles[index].BlendMode;
						p0.TextureCoordinateU = 0;

						index++;
						Vector3 nextPosition;
						if (requiresTransformation)
							nextPosition = pose.ToWorldPosition(particles[index].Position * scale);
						else
							nextPosition = particles[index].Position;

						Vector3 normal;
						if (uniformNormal.HasValue)
						{
							// Uniform normal.
							normal = uniformNormal.Value;
						}
						else
						{
							// Varying normal.
							normal = particles[index].Normal;
							if (requiresTransformation)
								normal = pose.ToWorldDirection(normal);
						}

						Vector3 previousDelta = nextPosition - p0.Position;
						p0.Axis = Vector3.Cross(normal, previousDelta);
						p0.Axis.TryNormalize();

						// Intermediate particles.
						while (index < endIndex - 1)
						{
							p1.Position = nextPosition;

							if (requiresTransformation)
							{
								nextPosition = pose.ToWorldPosition(particles[index + 1].Position * scale);
								p1.Size = particles[index].Size.Y * scale.Y;
							}
							else
							{
								nextPosition = particles[index + 1].Position;
								p1.Size = particles[index].Size.Y;
							}

							if (uniformNormal.HasValue)
							{
								// Uniform normal.
								normal = uniformNormal.Value;
							}
							else
							{
								// Varying normal.
								normal = particles[index].Normal;
								if (requiresTransformation)
									normal = pose.ToWorldDirection(normal);
							}

							Vector3 delta = nextPosition - p1.Position;
							Vector3 tangent = delta + previousDelta; // Note: Should we normalize vectors for better average?
							p1.Axis = Vector3.Cross(normal, tangent);
							p1.Axis.TryNormalize();

							p1.Color = particles[index].Color * color;
							p1.Alpha = particles[index].Alpha * alpha;
							p1.AnimationTime = particles[index].AnimationTime;
							p1.BlendMode = particles[index].BlendMode;
							p1.TextureCoordinateU = GetTextureCoordinateU1(index - 1, numberOfSegments, particleSystemData.TextureTiling);

							// Draw ribbon segment.
							_billboardBatch.DrawRibbon(ref p0, ref p1, texture);

							p0 = p1;
							p0.TextureCoordinateU = GetTextureCoordinateU0(index, numberOfSegments, particleSystemData.TextureTiling);
							previousDelta = delta;
							index++;
						}

						// Last particle.
						p1.Position = nextPosition;

						if (uniformNormal.HasValue)
						{
							// Uniform normal.
							normal = uniformNormal.Value;
						}
						else
						{
							// Varying normal.
							normal = particles[index].Normal;
							if (requiresTransformation)
								normal = pose.ToWorldDirection(normal);
						}

						p1.Axis = Vector3.Cross(normal, previousDelta);
						p1.Axis.TryNormalize();

						if (requiresTransformation)
							p1.Size = particles[index].Size.Y * scale.Y;
						else
							p1.Size = particles[index].Size.Y;

						p1.Color = particles[index].Color * color;
						p1.Alpha = particles[index].Alpha * alpha;
						p1.AnimationTime = particles[index].AnimationTime;
						p1.BlendMode = particles[index].BlendMode;
						p1.TextureCoordinateU = GetTextureCoordinateU1(index - 1, numberOfSegments, particleSystemData.TextureTiling);

						// Draw last ribbon segment.
						_billboardBatch.DrawRibbon(ref p0, ref p1, texture);
						index++;

					} while (index < numberOfParticles);
				}


				/// <summary>
				/// Gets the u texture coordinate at the start of a ribbon segment.
				/// </summary>
				/// <param name="i">The index of the segment in the current ribbon.</param>
				/// <param name="n">The number of segments in the current ribbon.</param>
				/// <param name="k">The tiling distance.</param>
				/// <returns>The u texture coordinate at the start of the ribbon segment.</returns>
				private static float GetTextureCoordinateU0(int i, int n, int k)
				{
					float texCoordU;
					if (k == 0)
					{
						// Texture is stretched along ribbon.
						texCoordU = (float)i / n;
					}
					else
					{
						// Texture repeats every k segments.
						texCoordU = (float)(i % k) / k;
					}

					return texCoordU;
				}


				/// <summary>
				/// Gets the u texture coordinate at the end of a ribbon segment.
				/// </summary>
				/// <param name="i">The index of the segment in the current ribbon.</param>
				/// <param name="n">The number of segments in the current ribbon.</param>
				/// <param name="k">The tiling distance.</param>
				/// <returns>The u texture coordinate at the end of the ribbon segment.</returns>
				private static float GetTextureCoordinateU1(int i, int n, int k)
				{
					float texCoordU;
					if (k == 0)
					{
						// Texture is stretched along ribbon.
						texCoordU = (float)(i + 1) / n;
					}
					else
					{
						// Texture repeats every k segments.
						texCoordU = (float)((i % k) + 1) / k;
					}

					return texCoordU;
				}
				#endregion
		*/
		#endregion
	}
}
