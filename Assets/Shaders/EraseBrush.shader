Shader "Custom/Brush"
{
    Properties
    {
        _MainTex ("Base", 2D) = "white" {}
        _BrushUV ("Brush UV", Vector) = (0,0,0,0)
        _BrushSize ("Size", Float) = 0.1
        _Strength ("Strength", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            float4 _BrushUV;
            float _BrushSize;
            float _Strength;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float circle(float2 uv, float2 center, float radius)
            {
                return smoothstep(radius, radius * 0.8, distance(uv, center));
            }

           half4 frag(Varyings i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float dist = distance(i.uv, _BrushUV.xy);

                // Proper circular mask
                float mask = 1 - smoothstep(_BrushSize, _BrushSize * 1.2, dist);

                // Apply only inside circle
                col.r = saturate(col.r + mask * _Strength);

                return col;
            }

            ENDHLSL
        }
    }
}