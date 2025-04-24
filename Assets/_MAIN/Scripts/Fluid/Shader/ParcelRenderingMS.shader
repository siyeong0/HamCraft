Shader "Custom/ParcelRenderingMS"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual
        PASS
        {
            CGPROGRAM
            #pragma vertex vert;
            #pragma fragment frag;
            #pragma multi_compile_instancing

            #include "Common.hlsl"
            StructuredBuffer<Parcel> parcelBuffer;
            float radius;
            float4 color;

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

                pos *= radius;

                float2 worldPos = center + pos;
                output.pos = UnityObjectToClipPos(float4(worldPos, 0, 1));
                output.uv = pos / radius;
                output.center = center;

                return output;
            }

            half4 frag(FragInput input) : SV_Target
            {
                float dist = length(input.uv);

                float alpha = smoothstep(1.0, 1.0, dist);
                alpha = 1.0 - alpha;

                return color * alpha;
            }
            ENDCG
        }
    }
}
