Shader "Custom/CircelMS"
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
            float radius;
            float4 color;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f output;

                float2 pos = v.vertex * radius;
                float2 center = positionBuffer[instanceID];
                float2 worldPos = center + pos;
                output.pos = UnityObjectToClipPos(float4(worldPos, 0, 1));
                output.uv = v.vertex;

                return output;
            }

            half4 frag(v2f input) : SV_Target
            {
                float dist = length(input.uv);
                float alpha = 1.0 - smoothstep(1.0, 1.0, dist);
                return float4(color.rgb, alpha);
            }
            ENDCG
        }
    }
}
