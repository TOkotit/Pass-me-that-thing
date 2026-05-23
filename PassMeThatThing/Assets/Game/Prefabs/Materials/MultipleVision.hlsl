#ifndef MULTIPLE_VISION_INCLUDED
#define MULTIPLE_VISION_INCLUDED

float4 _VisionZones[64];
int _VisionZonesCount;

void GetMultipleVision_float(float3 WorldPos, out float Visibility)
{
    float visibility = 0.0;
    int count = min(_VisionZonesCount, 64);

    for(int i = 0; i < count; i++)
    {
        float dist = distance(WorldPos, _VisionZones[i].xyz);
        float currentVis = step(dist, _VisionZones[i].w);
        visibility = max(visibility, currentVis);
    }

    Visibility = visibility;
}

#endif