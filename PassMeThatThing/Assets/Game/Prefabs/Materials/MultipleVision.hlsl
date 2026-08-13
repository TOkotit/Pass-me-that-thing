#ifndef MULTIPLE_VISION_INCLUDED
#define MULTIPLE_VISION_INCLUDED

#define MAX_BOUNDARY_POINTS 96
#define VERTICAL_LAYERS 5
#define TOTAL_LAYERS 11

#define MERIDIAN_COUNT 8
#define MERIDIAN_BOUNDARY_POINTS 88

#define PI 3.14159265359
#define TWO_PI 6.28318530718

StructuredBuffer<float4> _VisionZonesBuffer;
int _VisionZonesCount;

StructuredBuffer<float2> _VisionBoundaryBuffer;
StructuredBuffer<float2> _VisionMeridianBuffer;
float _VisionVerticalStep;

bool IsInsideSegment(float2 origin2D, float2 p0, float2 p1, float2 point2D)
{
    float2 edge = p1 - p0;
    float2 toPoint = point2D - p0;
    float2 toOrigin = origin2D - p0;

    float crossPoint = edge.x * toPoint.y - edge.y * toPoint.x;
    float crossOrigin = edge.x * toOrigin.y - edge.y * toOrigin.x;

    return (crossPoint * crossOrigin) >= 0.0;
}

void GetMultipleVision_float(float3 WorldPos, out float Visibility)
{
    float visibility = 0.0;

    for (int i = 0; i < _VisionZonesCount; i++)
    {
        float3 O = _VisionZonesBuffer[i].xyz;
        float zoneRadius = _VisionZonesBuffer[i].w;

        float3 delta3D = WorldPos - O;
        float dist3D = length(delta3D);

        if (dist3D > zoneRadius || dist3D < 0.0001)
            continue;

        float2 delta2D = float2(delta3D.x, delta3D.z);
        float horizontalDist = length(delta2D);
        float theta = atan2(delta2D.y, delta2D.x);
        if (theta < 0) theta += TWO_PI;

        float relativeHeight = WorldPos.y - O.y;
        int layerOffset = clamp((int)round(relativeHeight / _VisionVerticalStep), -VERTICAL_LAYERS, VERTICAL_LAYERS);
        int layerArrayIndex = layerOffset + VERTICAL_LAYERS;
        int ringBase = (i * TOTAL_LAYERS + layerArrayIndex) * MAX_BOUNDARY_POINTS;

        int rIdx0 = MAX_BOUNDARY_POINTS - 1;
        int rIdx1 = 0;
        for (int a = 0; a < MAX_BOUNDARY_POINTS; a++)
        {
            float2 pt = _VisionBoundaryBuffer[ringBase + a];
            if (pt.x > theta)
            {
                rIdx1 = a;
                rIdx0 = (a - 1 + MAX_BOUNDARY_POINTS) % MAX_BOUNDARY_POINTS;
                break;
            }
        }

        float2 rb0 = _VisionBoundaryBuffer[ringBase + rIdx0];
        float2 rb1 = _VisionBoundaryBuffer[ringBase + rIdx1];

        float2 rp0 = O.xz + float2(cos(rb0.x), sin(rb0.x)) * rb0.y;
        float2 rp1 = O.xz + float2(cos(rb1.x), sin(rb1.x)) * rb1.y;

        bool ringInside = IsInsideSegment(O.xz, rp0, rp1, WorldPos.xz);

        float thetaModPi = theta >= PI ? theta - PI : theta; // [0, PI)
        int mIdx = (int)round(thetaModPi / (PI / MERIDIAN_COUNT)) % MERIDIAN_COUNT;
        float planeAzimuth = mIdx * (PI / MERIDIAN_COUNT);

        float r = horizontalDist * cos(theta - planeAzimuth);
        float h = delta3D.y;
        float phiPoint = atan2(h, r);
        if (phiPoint < 0) phiPoint += TWO_PI;

        int meridianBase = (i * MERIDIAN_COUNT + mIdx) * MERIDIAN_BOUNDARY_POINTS;

        int mIdx0 = MERIDIAN_BOUNDARY_POINTS - 1;
        int mIdx1 = 0;
        for (int b = 0; b < MERIDIAN_BOUNDARY_POINTS; b++)
        {
            float2 pt = _VisionMeridianBuffer[meridianBase + b];
            if (pt.x > phiPoint)
            {
                mIdx1 = b;
                mIdx0 = (b - 1 + MERIDIAN_BOUNDARY_POINTS) % MERIDIAN_BOUNDARY_POINTS;
                break;
            }
        }

        float2 mb0 = _VisionMeridianBuffer[meridianBase + mIdx0];
        float2 mb1 = _VisionMeridianBuffer[meridianBase + mIdx1];

        float2 mp0 = float2(cos(mb0.x), sin(mb0.x)) * mb0.y;
        float2 mp1 = float2(cos(mb1.x), sin(mb1.x)) * mb1.y;
        float2 mPoint = float2(r, h);

        bool meridianInside = IsInsideSegment(float2(0, 0), mp0, mp1, mPoint);

        bool lightVisible = ringInside && meridianInside;
        visibility = max(visibility, lightVisible ? 1.0 : 0.0);
    }

    Visibility = visibility;
}

#endif