Shader "Custom/VisionOutlineFixed"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", Float) = 0.05
        _Enabled("Enabled", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }

        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 originalWorldPos : TEXCOORD0; 
            };

            float _OutlineWidth;
            float4 _OutlineColor;
            float _Enabled;
            
            float3 _GlobalVisionPos;
            float _Radius;

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.originalWorldPos = worldPos; 
                
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                worldPos += worldNormal * _OutlineWidth;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_Enabled < 0.5) discard;

                float d = distance(i.originalWorldPos, _GlobalVisionPos); 
                
                if (d < _Radius) discard;

                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}