Shader "Custom/FluidMS"
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

            #include "Common.hlsl"
            StructuredBuffer<Parcel> parcelBuffer;
            float4 _Color;
            float _Radius;

            struct VertInput
            {
                uint vertexID : SV_VertexID;
                uint instanceID : SV_InstanceID;
            };

            struct FragInput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 center : TEXCOORD1;
            };

            FragInput vert(VertInput vertexData)
            {
                FragInput output;

                float2 quad[6] = {
                    float2(-1, -1), // 0
                    float2( 1, -1), // 1
                    float2(-1,  1), // 2

                    float2(-1,  1), // 2
                    float2( 1, -1), // 1
                    float2( 1,  1)  // 3
                };

                float2 pos = quad[vertexData.vertexID];

                float2 center = parcelBuffer[vertexData.instanceID].Position;

                pos *= _Radius;

                float2 worldPos = center + pos;
                output.pos = UnityObjectToClipPos(float4(worldPos, 0, 1));
                output.uv = pos / _Radius;
                output.center = center;

                return output;
            }

            half4 frag(FragInput input) : SV_Target
            {
                float dist = length(input.uv);

                float alpha = smoothstep(0.95, 1.0, dist);
                alpha = 1.0 - alpha;

                return _Color * alpha;
            }
            ENDCG
        }
    }
}
