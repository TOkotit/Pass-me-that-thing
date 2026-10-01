#ifndef LIGHTING_CEL_SHADED_INCLUDED
#define LIGHTING_CEL_SHADED_INCLUDED



#ifndef SHADERGRAPH_PREVIEW

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct SurfaceVariables
{
    float3 normal;
    float3 view;
    float smoothness;
    float shininess;
    float rimThreshold;
    float steps;
    float3 ambientColor;
};
#endif

#ifndef SHADERGRAPH_PREVIEW
float3 CalculateCelShading(Light l, SurfaceVariables s)
{
    float attenuation = l.distanceAttenuation * l.shadowAttenuation;
    
    float NdotL = saturate(dot(s.normal, l.direction));

    float lightIntensity = NdotL * attenuation;

    float celStep = floor(lightIntensity * s.steps) / (s.steps - 1.0);
    
    float diffuse = saturate(celStep);

    float3 totalDiffuse = max(s.ambientColor, l.color * diffuse);
    
    float3 h = SafeNormalize(float3(l.direction) + s.view);
    float specular = saturate(dot(s.normal, h));
    specular = pow(specular, s.shininess);
    specular = step(0.5, specular) * diffuse * s.smoothness;

    float rim = 1.0 - dot(s.view, s.normal);
    rim = step(1.0 - s.rimThreshold, rim) * diffuse;

    return totalDiffuse + l.color * (max(specular, rim));
}
#endif

void LightingCelShaded_float(
    float3 AmbientColor,
    float Steps,
    float Smoothness, 
    float RimThreshold,
    float3 Position,
    float3 Normal, 
    float3 View,
    out float3 Color)
{
#if defined(SHADERGRAPH_PREVIEW)
    Color = float3(0.5, 0.5, 0.5);
#else
    SurfaceVariables s;
    s.normal = normalize(Normal);
    s.view = SafeNormalize(View);
    s.smoothness = Smoothness;
    s.shininess = exp2(10 * Smoothness + 1);
    s.rimThreshold = RimThreshold;
    s.steps = Steps;
    s.ambientColor = AmbientColor;
    
#if SHADOWS_SCREEN
    float4 clipPos = TransformWorldToHClip(Position);
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(Position);
#endif
    
    Light light = GetMainLight(shadowCoord);
    Color = CalculateCelShading(light, s);
    
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; i++)
    {
        half4 shadowMask = half4(1, 1, 1, 1);
        light = GetAdditionalLight(i, Position, shadowMask);
        Color += CalculateCelShading(light, s);
    }
#endif
}

#endif