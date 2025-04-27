Shader "Custom/SpeedMS"
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

            StructuredBuffer<float2> positionBuffer;
            StructuredBuffer<float2> velocityBuffer;
            float radius;
            float maxSpeed;
            Texture2D speedColorMap;
            SamplerState linear_clamp_sampler
            {
                Filter = MIN_MAG_MIP_LINEAR;
                AddressU = CLAMP;
                AddressV = CLAMP;
                AddressW = CLAMP;
            };

            struct appdata
            {
                float4 vertex : POSITION;
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 center : TEXCOORD1;
                float2 colorUV : TEXCOORD2;
            };

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f output;

                float2 pos = v.vertex * radius;
                float2 center = positionBuffer[instanceID];
                float2 worldPos = center + pos;
                output.pos = UnityObjectToClipPos(float4(worldPos, 0, 1));
                output.uv = v.vertex;
                output.center = center;
                float speed = length(velocityBuffer[instanceID]);
                float speedT = saturate(speed / maxSpeed);
                output.colorUV = float2(speedT, 0.5);

                return output;
            }

            half4 frag(v2f input) : SV_Target
            {
                float dist = length(input.uv);
                float alpha = 1.0 - smoothstep(1.0, 1.0, dist);
                float3 speedColor = speedColorMap.Sample(linear_clamp_sampler, input.colorUV);
                return float4(speedColor, alpha);
            }
            ENDCG
        }
    }
}
