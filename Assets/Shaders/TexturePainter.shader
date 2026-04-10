Shader "TNTC/TexturePainter"{   

    Properties{
        _PainterColor ("Painter Color", Color) = (0, 0, 0, 0)
    }

    SubShader{
        Cull Off ZWrite Off ZTest Off

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float3 _PainterPosition;
            float2 _PainterUV;
            float _Radius;
            float _Hardness;
            float _Strength;
            float4 _PainterColor;
            float _PrepareUV;

            sampler2D _OriginalTex;
            float _UseOriginal; // 0 = paint, 1 = restore

            struct appdata{
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            };

            struct v2f{
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float mask(float3 position, float3 center, float radius, float hardness){
                float m = distance(center, position);
                return 1 - smoothstep(radius * hardness, radius, m);    
            }

            float maskUV(float2 uv, float2 center, float radius, float hardness)
            {
                float dist = distance(uv, center);
                return 1.0 - smoothstep(radius * hardness, radius, dist);
            }

            v2f vert (appdata v){
                v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
				float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2( 2, 2) - float2(1, 1));
				o.vertex = uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{   

                float4 col; 

                if(_PrepareUV > 0 ){
                    return float4(0, 0, 1, 1);
                }         

                float4 original = tex2D(_OriginalTex, i.uv);

                if (_UseOriginal > 0.5)
                {
                    col = original;
                }
                else{

                    col = tex2D(_MainTex, i.uv);
                }

                float f = maskUV(i.uv, _PainterUV, _Radius, _Hardness);
                //float f = mask(i.worldPos, _PainterPosition, _Radius, _Hardness);
                float edge = f * _Strength;
                return lerp(col, _PainterColor, edge);
            }
            ENDCG
        }
    }
}