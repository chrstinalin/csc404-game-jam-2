Shader "PSX/Dithering"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorDepth ("Color Depth", Float) = 16
        _DitherStrength ("Dither Strength", Range(0, 2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _ColorDepth;
            float _DitherStrength;

            float Bayer2(float2 a)
            {
                a = floor(a);
                return frac(dot(a, float2(0.5, a.y * 0.75)));
            }

            #define Bayer4(a)   (Bayer2(0.5 * (a)) * 0.25 + Bayer2(a))
            #define Bayer8(a)   (Bayer4(0.5 * (a)) * 0.25 + Bayer2(a))

            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float2 pixelPos = i.uv * _ScreenParams.xy;
                
                float dither = (Bayer8(pixelPos) - 0.5) * _DitherStrength;
                col.rgb += dither / _ColorDepth;
                
                col.rgb = floor(col.rgb * _ColorDepth) / _ColorDepth;
                
                return col;
            }
            ENDCG
        }
    }
}