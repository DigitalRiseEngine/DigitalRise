//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ProjectorLight.fx
/// Renders a projector light into the light buffer for deferred lighting.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Lighting.fxh"
#include "../Noise.fxh"
#include "../ShadowMap.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Type of light texture.
static const int TextureRgb = 1;     // RGB texture.
static const int TextureAlpha = 2;   // Alpha-only texture.


float4x4 WorldViewProjection : WORLDVIEWPROJECTION;  // (Only for clip geometry.)
float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);

float3 ProjectorLightDiffuse : PROJECTORLIGHTDIFFUSE;
float3 ProjectorLightSpecular : PROJECTORLIGHTSPECULAR;
float3 ProjectorLightPosition : PROJECTORLIGHTPOSITION;   // Position in world space relative to camera!
float ProjectorLightRange : PROJECTORLIGHTRANGE;
float ProjectorLightAttenuation : PROJECTORLIGHTATTENUATION;
DECLARE_UNIFORM_LIGHTTEXTURE(ProjectorLightTexture, PROJECTORLIGHTTEXTURE);  // Matrix is also relative to camera!
float4x4 ProjectorLightTextureMatrix;   // Converts from view to light texture space.

float4 ShadowMaskChannel;
DECLARE_UNIFORM_SHADOWMASK(ShadowMask);


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

float4 VSClip(float4 position : POSITION) : SV_Position
{
  return mul(position, WorldViewProjection);
}

float4 PSClip() : COLOR0
{
  return 0;
}


VSFrustumRayOutput VS(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize);
}


void PS(float2 texCoord : TEXCOORD0,
        float3 frustumRay : TEXCOORD1,
        out float4 lightBuffer0 : COLOR0,
        out float4 lightBuffer1 : COLOR1,
        uniform const bool hasShadow,
        uniform const int textureType)
{
  lightBuffer0 = 0;
  lightBuffer1 = 0;
  
  // Get depth.
  float4 gBuffer0Sample = tex2D(GBuffer0Sampler, texCoord);
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Abort for skybox pixels.
  clip(0.9999f - depth);
  
  // Reconstruct view space position.
  float3 cameraToPixel = frustumRay * depth;
  
  // Get normal.
  float4 gBuffer1Sample = tex2D(GBuffer1Sampler, texCoord);
  float3 normal = GetGBufferNormal(gBuffer1Sample);
  
  // Compute light distance and normalized direction.
  float3 lightDirection = cameraToPixel - ProjectorLightPosition;
  float lightDistance = length(lightDirection);
  lightDirection = lightDirection / lightDistance;
  
  // Compute N.L and distance attenuation. Abort if the light is attenuated to 0.
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, ProjectorLightRange, ProjectorLightAttenuation);
  float nDotLAttenuated = dot(normal, -lightDirection) * distanceAttenuation;
  clip(nDotLAttenuated - 0.0001f);
  
  // Blinn-Phong
  float3 viewDirection = normalize(cameraToPixel);
  float specularPower = GetGBufferSpecularPower(gBuffer0Sample, gBuffer1Sample);
  float3 h = -normalize(lightDirection + viewDirection);
  float specular = pow(0.000001 + saturate(dot(normal, h)), specularPower);
  
  // Projected texture
  float4 lightTexCoord = mul(float4(cameraToPixel, 1), ProjectorLightTextureMatrix);
  
  // Clip back projection.
  clip(lightTexCoord.w);
  
  lightTexCoord.xy /= lightTexCoord.w;
  
  // Clip if pixel is outside the texture.
  clip(float4(lightTexCoord.x, lightTexCoord.y, 1 - lightTexCoord.x, 1 - lightTexCoord.y));
  
  // Sample texture of projector light.
  float3 textureColor;
  if (textureType == TextureRgb)
    textureColor = FromGamma(tex2D(ProjectorLightTextureSampler, lightTexCoord.xy).rgb);
  else
    textureColor = FromGamma(tex2D(ProjectorLightTextureSampler, lightTexCoord.xy).aaa);
  
  // Shadow map
  float shadowTerm = 1;
  if (hasShadow)
    shadowTerm = dot(tex2D(ShadowMaskSampler, texCoord), ShadowMaskChannel);
  
  lightBuffer0.rgb = ProjectorLightDiffuse * textureColor * nDotLAttenuated * shadowTerm;
  lightBuffer1.rgb = ProjectorLightSpecular * textureColor * specular * nDotLAttenuated * shadowTerm;
}

void PSRgb(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
           out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, false, TextureRgb);
}

void PSAlpha(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
             out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, false, TextureAlpha);
}

void PSShadowRgb(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
                 out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, true, TextureRgb);
}

void PSShadowAlpha(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
                   out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, true, TextureAlpha);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_3
#define PSTARGET ps_4_0_level_9_3
#endif

technique
{
  pass Clip
  {
    VertexShader = compile VSTARGET VSClip();
    PixelShader = compile PSTARGET PSClip();
  }
  pass DefaultRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSRgb();
  }
  pass DefaultAlpha
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSAlpha();
  }
  pass ShadowedRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSShadowRgb();
  }
  pass ShadowedAlpha
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSShadowAlpha();
  }
}
