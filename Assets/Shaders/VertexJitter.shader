Shader "PSX/VertexJitter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _JitterAmount ("Jitter Amount", Float) = 100.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _JitterAmount;

            v2f vert (appdata v)
            {
                v2f o;
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                
                // Snap vertices to a grid in screen space
                float2 snappedPos = clipPos.xy / clipPos.w;
                snappedPos *= _JitterAmount;
                snappedPos = floor(snappedPos);
                snappedPos /= _JitterAmount;
                
                clipPos.xy = snappedPos * clipPos.w;
                o.vertex = clipPos;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}