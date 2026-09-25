#ifndef MULTIPLE_VISION_SHADOWS_INCLUDED
#define MULTIPLE_VISION_SHADOWS_INCLUDED

StructuredBuffer<float4> _VisionConesPosRange;
StructuredBuffer<float4> _VisionConesDirAngle;

Texture2D _VisionShadowMap;
SamplerState sampler_VisionShadowMap;

float4x4 _VisionWorldToLightMatrices[16];
int _VisionSourcesCount;

bool IsInsideCone(float3 O, float3 coneDir, float cosHalfAngle, float range, float3 P)
{
    float3 toPoint = P - O;
    float dist = length(toPoint);

    if (dist > range || dist < 0.0001) return false;

    float3 dirToPoint = toPoint / dist;
    return dot(dirToPoint, coneDir) >= cosHalfAngle;
}

void GetMultipleVision_float(float3 WorldPos, out float Visibility)
{
    float visibility = 0.0;

    for (int i = 0; i < _VisionSourcesCount; i++)
    {
        float3 coneOrigin = _VisionConesPosRange[i].xyz;
        float coneRange = _VisionConesPosRange[i].w;
        float3 coneDir = _VisionConesDirAngle[i].xyz;
        float cosHalfAngle = _VisionConesDirAngle[i].w;

        if (IsInsideCone(coneOrigin, coneDir, cosHalfAngle, coneRange, WorldPos))
        {
            float4 shadowCoord = mul(_VisionWorldToLightMatrices[i], float4(WorldPos, 1.0));
            
            if (shadowCoord.w > 0.0)
            {
                float3 lightNDC = shadowCoord.xyz / shadowCoord.w;
                float2 shadowUV = lightNDC.xy * 0.5 + 0.5;

                #if UNITY_UV_STARTS_AT_TOP
                    shadowUV.y = 1.0 - shadowUV.y;
                #endif

                float2 clampedUV = clamp(shadowUV, 0.001, 0.999);

                float sampledDepth = _VisionShadowMap.SampleLevel(sampler_VisionShadowMap, clampedUV, 0).r;
                float currentDepth = lightNDC.z;
                float bias = 0.002;

                #if defined(UNITY_REVERSED_Z)
                    bool notOccluded = (currentDepth + bias) >= sampledDepth;
                #else
                    bool notOccluded = (currentDepth - bias) <= sampledDepth;
                #endif

                if (notOccluded)
                {
                    visibility = 1.0;
                    break;
                }
            }
        }
    }

    Visibility = visibility;
}

#endif