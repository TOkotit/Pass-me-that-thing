#ifndef MULTIPLE_VISION_INCLUDED
#define MULTIPLE_VISION_INCLUDED

StructuredBuffer<float4> _VisionZonesBuffer;
int _VisionZonesCount;

void GetMultipleVision_float(float3 WorldPos, out float Visibility)
{
    float visibility = 0.0;

    for (int i = 0; i < _VisionZonesCount; i++)
    {
        float dist = distance(WorldPos, _VisionZonesBuffer[i].xyz);
        float currentVis = step(dist, _VisionZonesBuffer[i].w);
        visibility = max(visibility, currentVis);
    }

    Visibility = visibility;
}

#endif