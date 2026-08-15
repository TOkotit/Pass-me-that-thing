Shader "Unlit/Fisheye"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Strength", Range(-1, 3)) = 0
        _Radius ("Radius", Range(0.1, 2.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Blend One Zero
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _UIE_OUTPUT_LINEAR

            #include "UnityCG.cginc"
            #include "UnityUIEFilter.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                uint rectIndex : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float _Strength;
                float _Radius;
            CBUFFER_END

            v2f vert (FilterVertexInput v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.rectIndex = GetFilterRectIndex(v);
                return o;
            }

            float2 NormalizeUVs(float2 uv, float4 uvRect)
            {
                return float2(
                    (uv.x - uvRect.x) / uvRect.z,
                    (uv.y - uvRect.y) / uvRect.w
                );
            }

            float2 MapToUVRect(float2 uv, float4 uvRect)
            {
                return float2(
                    uv.x * uvRect.z + uvRect.x,
                    uv.y * uvRect.w + uvRect.y
                );
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 uvRect = GetFilterUVRect(i.rectIndex);

                float2 uv = NormalizeUVs(i.uv, uvRect);
                float2 center = float2(0.5, 0.5);
                uv = uv - center;

                float elementWidth = uvRect.z * _MainTex_TexelSize.z;
                float elementHeight = uvRect.w * _MainTex_TexelSize.w;
                float aspect = elementHeight != 0.0 ? (elementWidth / elementHeight) : 1.0;
                float2 aspectUv = uv * float2(aspect, 1.0);

                float radius = length(aspectUv);
                
                float normalizedRadius = radius / _Radius;

                float newRadius = pow(saturate(normalizedRadius), 1.0 + _Strength) * _Radius;

                float2 distorted = newRadius * float2(cos(atan2(aspectUv.y, aspectUv.x)), sin(atan2(aspectUv.y, aspectUv.x)));
                distorted = distorted / float2(aspect, 1.0);

                uv = distorted + center;

                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return fixed4(0, 0, 0, 0);
                }

                uv = MapToUVRect(uv, uvRect);

                half4 col = tex2D(_MainTex, uv);

                #if _UIE_OUTPUT_LINEAR
                col.rgb = GammaToLinearSpace(col.rgb);
                #endif

                return col;
            }
            ENDCG
        }
    }
}