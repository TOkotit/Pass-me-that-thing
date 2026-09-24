#ifndef MULTIPLE_VISION_INCLUDED
#define MULTIPLE_VISION_INCLUDED

StructuredBuffer<float4> _VisionConesPosRange; // xyz = позиция, w = дальность
StructuredBuffer<float4> _VisionConesDirAngle; // xyz = направление, w = cos(halfAngle)
int _VisionConesCount;

bool IsInsideCone(float3 O, float3 coneDir, float cosHalfAngle, float range, float3 P)
{
    float3 toPoint = P - O;
    float dist = length(toPoint);

    if (dist > range || dist < 0.0001)
        return false;

    float3 dirToPoint = toPoint / dist;
    float cosAngle = dot(dirToPoint, coneDir);

    return cosAngle >= cosHalfAngle;
}

void GetMultipleVision_float(float3 WorldPos, out float Visibility)
{
    float visibility = 0.0;

    for (int c = 0; c < _VisionConesCount; c++)
    {
        float3 coneOrigin = _VisionConesPosRange[c].xyz;
        float coneRange = _VisionConesPosRange[c].w;
        float3 coneDir = _VisionConesDirAngle[c].xyz;
        float cosHalfAngle = _VisionConesDirAngle[c].w;

        if (IsInsideCone(coneOrigin, coneDir, cosHalfAngle, coneRange, WorldPos))
        {
            visibility = 1.0;
        }
    }

    Visibility = visibility;
}

#endif