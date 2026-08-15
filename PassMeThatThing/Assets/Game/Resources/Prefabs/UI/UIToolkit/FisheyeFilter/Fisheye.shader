Shader "Unlit/Fisheye"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha ("Double Sphere Alpha", Float) = 0.5
        _Chi ("Double Sphere Chi", Float) = 0.5
        _FocalLength ("Double Sphere Focal Length", Float) = 1.0
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
                float _Alpha;
                float _Chi;
                float _FocalLength;
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
                
                float elementWidth = uvRect.z * _MainTex_TexelSize.z;
                float elementHeight = uvRect.w * _MainTex_TexelSize.w;
                float aspect = elementHeight != 0.0 ? (elementWidth / elementHeight) : 1.0;

                float2 centered = uv - 0.5;
                centered.x *= aspect;

                float cx = 0.0;
                float cy = 0.0;
                float fx = _FocalLength;
                float fy = _FocalLength;
                float alpha = _Alpha;
                float chi = _Chi;

                float mx = (centered.x - cx) / fx;
                float my = (centered.y - cy) / fy;

                float r2 = mx * mx + my * my;
                float beta1 = 1.0 - (2.0 * alpha - 1.0) * r2;
                if (beta1 < 0.0) {
                    return fixed4(0, 0, 0, 0);
                }

                float mz = (1.0 - alpha * alpha * r2) / (alpha * sqrt(beta1) + 1.0 - alpha);
                float beta2 = mz * mz + (1.0 - chi * chi) * r2;

                if (beta2 < 0.0) {
                    return fixed4(0, 0, 0, 0);
                }

                float denom = mz * mz + r2;
                if (denom == 0.0) {
                    return fixed4(0, 0, 0, 0);
                }

                float3 fisheye_ray = (mz * chi + sqrt(beta2)) / denom * float3(mx, my, mz) - float3(0, 0, chi);

                float rayLen = length(fisheye_ray.xy);
                float maxLen = length(float3(mx, my, mz));
                float normFactor = maxLen > 0.0 ? (rayLen / maxLen) : 0.0;

                float2 distorted = float2(mx, my) * normFactor;
                distorted.x /= aspect;

                uv = distorted + 0.5;

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