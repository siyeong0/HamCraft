// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FluidPS"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Radius("Radius", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        PASS
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert;
            #pragma fragment frag;
            #pragma multi_compile_instancing

            StructuredBuffer<float2> positions;
            float4 _Color;
            float _Radius;

            struct appdata
            {
                uint vertexID : SV_VertexID;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 center : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float2 quad[6] = {
                    float2(-1, -1), // 0
                    float2( 1, -1), // 1
                    float2(-1,  1), // 2

                    float2(-1,  1), // 2
                    float2( 1, -1), // 1
                    float2( 1,  1)  // 3
                };

                float2 pos = quad[v.vertexID];

                float2 center = positions[v.instanceID];

                pos *= _Radius;

                float2 worldPos = center + pos;
                o.pos = UnityObjectToClipPos(float4(worldPos, 0, 1));
                o.uv = pos / _Radius;
                o.center = center;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float dist = length(i.uv);

                float alpha = smoothstep(0.95, 1.0, dist);
                alpha = 1.0 - alpha;

                return _Color * alpha;
            }
            ENDCG
        }
    }
}
