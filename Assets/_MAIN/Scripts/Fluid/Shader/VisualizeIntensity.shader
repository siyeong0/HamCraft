Shader "Custom/VisualizeIntensity"
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

            #include "Common.hlsl"
            StructuredBuffer<float2> positionBuffer;
            StructuredBuffer<float2> velocityBuffer;
            int numParcels;
            float smoothingRadius;
            float targetDensity;
            float4 negativeColor;
            float4 positiveColor;
            float4 zeroColor;

            struct FragInput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            FragInput vert(uint id : SV_VertexID)
            {
                float2 positions[6] = {
                    float2(-1, -1),
                    float2( 1, -1),
                    float2(-1,  1),
                    float2(-1,  1),
                    float2( 1, -1),
                    float2( 1,  1)
                };

                FragInput output;
                output.pos = float4(positions[id], 0, 1);
                output.uv = (positions[id] + 1) * 0.5;

                return output;
            }

            half4 frag(FragInput input) : SV_Target
            {
                // uv to world
                float2 uv = input.uv;
                float4 clipPos;
                clipPos.xy = uv * 2.0 - 1.0;
                clipPos.y *= -1;
                clipPos.z = 0;
                clipPos.w = 1.0;

                float4 worldPos = mul(unity_CameraInvProjection, clipPos);
                worldPos /= worldPos.w; // perspective divide
                worldPos = mul(unity_CameraToWorld, worldPos);
                float2 samplePos = worldPos.xy;

                // calc density
                float density = 0.0;
                [loop]
                for (int i = 0; i < numParcels; ++i)
                {
                    float distance = length(samplePos - positionBuffer[i]);
                    float influence = spikyKernel(distance, smoothingRadius);
                    density += influence;
                }

                // draw intensity
                float intensity = (density - targetDensity) / targetDensity * 0.5;
                if (intensity > 0)
                {
                    float4 diff = positiveColor - zeroColor;
                    return lerp(zeroColor, positiveColor + diff, intensity);
                }
                else
                {
                    float4 diff = negativeColor - zeroColor;
                    return lerp(zeroColor, negativeColor + diff, -intensity);
                }

                return zeroColor;
            }
            ENDCG
        }
    }
}
