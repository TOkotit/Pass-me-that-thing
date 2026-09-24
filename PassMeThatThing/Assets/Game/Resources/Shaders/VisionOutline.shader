Shader "Custom/VisionOutline"
{
   Properties
    {
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", Float) = 0.05
        _Enabled("Enabled", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "OUTLINE"

            Cull Front      
            ZWrite Off
            ZTest LEqual
            Offset 1, 1
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "MultipleVision.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float _Enabled;

            v2f vert(appdata v)
            {
                v2f o;
                
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldPos = worldPos;
                
                float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                worldPos += worldNormal * (_OutlineWidth * 0.5);
                o.pos = UnityWorldToClipPos(worldPos);
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_Enabled < 0.5)
                    discard;

                float visibility;
                GetMultipleVision_float(i.worldPos, visibility);

                if (visibility > 0.01)
                    discard;

                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}